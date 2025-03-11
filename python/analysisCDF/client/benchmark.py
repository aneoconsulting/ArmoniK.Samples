import statistics
import time
from datetime import timedelta
from typing import Any, Dict, List

import grpc
from armonik.client import ArmoniKEvents, ArmoniKResults, ArmoniKSessions, ArmoniKTasks
from armonik.common import TaskDefinition, TaskOptions
from reporting import generate_detailed_report, print_summary, save_results_to_csv
from utils import logger, retry_operation
from visualization import (
    generate_latency_percentile_graph,
    generate_matplotlib_latency_graph,
    generate_matplotlib_throughput_graph,
    generate_throughput_graph,
)


def run_batch(
    task_client: ArmoniKTasks,
    result_client: ArmoniKResults,
    session_client: ArmoniKSessions,
    events_client: ArmoniKEvents,
    partition: str,
    batch_size: int,
    iteration: int,
    scenario: str,
) -> Dict[str, Any]:
    """Run a batch of tasks and measure performance metrics, reusing the same clients."""
    metrics = {
        "batch_size": batch_size,
        "submission_time": 0.0,
        "processing_time": 0.0,
        "total_time": 0.0,
        "scenario": scenario,
        "iteration": iteration + 1,
    }
    start_total = time.time()

    task_options = TaskOptions(
        max_duration=timedelta(hours=1),
        max_retries=2,
        priority=1,
        partition_id=partition,
        options={},
    )

    # Create session with retry (you can also reuse a single session if desired)
    session_id = retry_operation(
        lambda: session_client.create_session(task_options, partition_ids=[partition]),
        operation_name="Session creation",
    )

    task_definitions = []

    # Create outputs metadata
    outputs = result_client.create_results_metadata(
        result_names=[f"output_{i}" for i in range(batch_size)],
        session_id=session_id,
    )

    # Ensure result_ids is populated
    result_ids = [outputs[f"output_{i}"].result_id for i in range(batch_size)]

    # Create payloads data
    payloads_data = {f"payload_{i}": "".encode() for i in range(batch_size)}

    payloads = result_client.create_results(
        results_data=payloads_data, session_id=session_id
    )

    # Ensure payloads are assigned correctly
    for i in range(batch_size):
        payload_id = payloads[f"payload_{i}"].result_id
        output_id = outputs[f"output_{i}"].result_id

        task_definitions.append(
            TaskDefinition(payload_id=payload_id, expected_output_ids=[output_id])
        )

    submission_start = time.time()
    task_client.submit_tasks(session_id, task_definitions)
    submission_timestamp = time.time()  # Record exactly when tasks were submitted
    metrics["submission_time"] = submission_timestamp - submission_start

    processing_start = time.time()
    try:
        downloaded_results = events_client.wait_for_result_availability_and_download(
            result_ids=result_ids,
            session_id=session_id,
            submission_timestamp=submission_timestamp,
        )

        # Verify we received all expected results
        if len(downloaded_results) != len(result_ids):
            logger.warning(
                "Expected %d results but only received %d",
                len(result_ids),
                len(downloaded_results),
            )

        task_times = []
        task_download_times = []
        task_completion_timestamps = []
        earliest_completion = float("inf")
        latest_completion = 0

        for result_id, result_data in downloaded_results.items():
            if "processing_time" in result_data:
                task_times.append(result_data["processing_time"])

            if "download_time" in result_data:
                task_download_times.append(result_data["download_time"])

            if "completion_timestamp" in result_data:
                completion_ts = result_data["completion_timestamp"]
                task_completion_timestamps.append(completion_ts)
                earliest_completion = min(earliest_completion, completion_ts)
                latest_completion = max(latest_completion, completion_ts)

        if task_times:
            metrics["min_task_time"] = min(task_times)
            metrics["max_task_time"] = max(task_times)
            metrics["avg_task_time"] = sum(task_times) / len(task_times)
            metrics["median_task_time"] = statistics.median(task_times)

            sorted_times = sorted(task_times)
            if len(sorted_times) >= 10:
                metrics["p90_task_time"] = sorted_times[int(len(sorted_times) * 0.9)]
                metrics["p95_task_time"] = sorted_times[int(len(sorted_times) * 0.95)]
                metrics["p99_task_time"] = sorted_times[int(len(sorted_times) * 0.99)]

            if len(task_times) > 1:
                metrics["stddev_task_time"] = statistics.stdev(task_times)
                metrics["cv_task_time"] = (
                    metrics["stddev_task_time"] / metrics["avg_task_time"]
                    if metrics["avg_task_time"] > 0
                    else 0
                )

                outlier_threshold = (
                    metrics["avg_task_time"] + 2 * metrics["stddev_task_time"]
                )
                outliers = [t for t in task_times if t > outlier_threshold]
                metrics["outlier_count"] = len(outliers)
                metrics["outlier_percentage"] = (
                    (len(outliers) / len(task_times)) * 100 if task_times else 0
                )

            logger.debug(
                "Task timing stats - Min: %.3fs, Max: %.3fs, Avg: %.3fs, StdDev: %.3fs, Outliers: %d/%d",
                metrics["min_task_time"],
                metrics["max_task_time"],
                metrics["avg_task_time"],
                metrics.get("stddev_task_time", 0),
                metrics.get("outlier_count", 0),
                len(task_times),
            )

        # Add download time metrics if available
        if task_download_times:
            metrics["min_download_time"] = min(task_download_times)
            metrics["max_download_time"] = max(task_download_times)
            metrics["avg_download_time"] = sum(task_download_times) / len(
                task_download_times
            )
            metrics["total_download_time"] = sum(task_download_times)

            if len(task_download_times) > 1:
                metrics["stddev_download_time"] = statistics.stdev(task_download_times)

            logger.debug(
                "Download time stats - Min: %.3fs, Max: %.3fs, Avg: %.3fs, Total: %.3fs",
                metrics["min_download_time"],
                metrics["max_download_time"],
                metrics["avg_download_time"],
                metrics["total_download_time"],
            )

        if task_completion_timestamps and len(task_completion_timestamps) > 1:
            metrics["completion_spread"] = latest_completion - earliest_completion
            metrics["first_result_time"] = earliest_completion - submission_timestamp
            metrics["last_result_time"] = latest_completion - submission_timestamp

            if metrics["completion_spread"] > 0:
                metrics["completion_rate"] = (
                    len(task_completion_timestamps) / metrics["completion_spread"]
                )

            logger.debug(
                "Task completion spread: %.3fs (first: %.3fs, last: %.3fs, rate: %.1f results/sec)",
                metrics["completion_spread"],
                metrics["first_result_time"],
                metrics["last_result_time"],
                metrics.get("completion_rate", 0),
            )

        metrics["processing_time"] = time.time() - processing_start

        if "avg_task_time" in metrics and batch_size > 0:
            metrics["total_task_time"] = sum(task_times)
            metrics["ideal_parallel_time"] = metrics["avg_task_time"]
            metrics["parallelization_efficiency"] = (
                metrics["ideal_parallel_time"] / metrics["processing_time"]
                if metrics["processing_time"] > 0
                else 0
            )
            metrics["system_utilization"] = (
                metrics["total_task_time"] / (batch_size * metrics["processing_time"])
                if metrics["processing_time"] > 0
                else 0
            )

            logger.debug(
                "System efficiency - Utilization: %.2f%%, Parallelization efficiency: %.2f%%",
                metrics["system_utilization"] * 100,
                metrics["parallelization_efficiency"] * 100,
            )

    except Exception as e:
        logger.error("Error waiting for results: %s", e)

    metrics["total_time"] = time.time() - start_total
    metrics["tasks_per_second"] = (
        (batch_size / metrics["total_time"]) if metrics["total_time"] > 0 else 0
    )
    return metrics


def run_benchmarks(endpoint: str, partition: str) -> List[Dict[str, Any]]:
    """Runs the benchmarking scenarios sequentially without parallelization,
    but reuses the same gRPC channel and client objects across all runs.
    """
    all_results = []

    # Create one gRPC channel for all scenarios
    with grpc.insecure_channel(endpoint) as channel:
        task_client = ArmoniKTasks(channel)
        result_client = ArmoniKResults(channel)
        session_client = ArmoniKSessions(channel)
        events_client = ArmoniKEvents(channel)

        scenarios = [
            {"name": "1x1000", "batch_size": 1, "iterations": 1000},
            {"name": "10x700", "batch_size": 10, "iterations": 700},
            {"name": "100x300", "batch_size": 100, "iterations": 300},
        ]

        for scenario in scenarios:
            logger.info("Starting Scenario: %s", scenario["name"])
            scenario_results = []
            start_time = time.time()
            successful_runs = 0
            failed_runs = 0

            # Process iterations one by one
            for i in range(scenario["iterations"]):
                try:
                    result = run_batch(
                        task_client=task_client,
                        result_client=result_client,
                        session_client=session_client,
                        events_client=events_client,
                        partition=partition,
                        batch_size=scenario["batch_size"],
                        iteration=i,
                        scenario=scenario["name"],
                    )
                    scenario_results.append(result)
                    successful_runs += 1

                    # Log progress every 10 iterations or at the end
                    if (i + 1) % 10 == 0 or (i + 1) == scenario["iterations"]:
                        elapsed = time.time() - start_time
                        progress = (i + 1) / scenario["iterations"] * 100
                        est_remaining = (
                            (elapsed / (i + 1)) * (scenario["iterations"] - i - 1)
                            if i > 0
                            else 0
                        )
                        logger.info(
                            "Progress: %.1f%% - Completed %d/%d iterations (%d successful, %d failed) - Est. remaining time: %.1fs",
                            progress,
                            i + 1,
                            scenario["iterations"],
                            successful_runs,
                            failed_runs,
                            est_remaining,
                        )
                except Exception as e:
                    logger.error(
                        "Error in batch execution (iteration %d): %s", i + 1, str(e)
                    )
                    failed_runs += 1

            all_results.extend(scenario_results)
            logger.info(
                "Completed scenario %s with %d successful and %d failed runs in %.1fs",
                scenario["name"],
                successful_runs,
                failed_runs,
                time.time() - start_time,
            )

    save_results_to_csv(all_results)
    generate_latency_percentile_graph(all_results)
    generate_throughput_graph(all_results)
    generate_matplotlib_latency_graph(all_results)
    generate_matplotlib_throughput_graph(all_results)
    print_summary(all_results)
    generate_detailed_report(all_results)

    return all_results

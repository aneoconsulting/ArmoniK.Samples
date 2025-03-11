import time
from datetime import timedelta
from typing import Any, Dict, List

import grpc
from armonik.client import ArmoniKEvents, ArmoniKResults, ArmoniKSessions, ArmoniKTasks
from armonik.common import TaskDefinition, TaskOptions
from reporting import save_results_to_csv
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
    metrics["submission_time"] = time.time() - submission_start

    processing_start = time.time()
    try:
        events_client.wait_for_result_availability(
            result_ids=result_ids,
            session_id=session_id,
        )
    except Exception as e:
        logger.error("Error waiting for results: %s", e)
    metrics["processing_time"] = time.time() - processing_start
    metrics["total_time"] = time.time() - start_total
    #
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
                        "Error in batch execution (iteration %d): %s", i + 1, e
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

    return all_results

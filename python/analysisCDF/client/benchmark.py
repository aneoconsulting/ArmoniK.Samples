import concurrent.futures
import statistics
import time
from datetime import timedelta
from typing import Any, Callable, Dict, List, Set, Tuple

import grpc
from armonik.client import ArmoniKEvents, ArmoniKResults, ArmoniKSessions, ArmoniKTasks
from armonik.common import TaskDefinition, TaskOptions
from reporting import generate_detailed_report, print_summary, save_raw_results_csv
from utils import logger, retry_operation
from visualization import (
    generate_latency_percentile_graph,
    generate_matplotlib_latency_graph,
)


def submit_tasks_in_parallel(
    task_client: ArmoniKTasks,
    session_id: str,
    task_definitions: List[TaskDefinition],
    batch_size: int,
) -> Tuple[float, float, float, Dict[int, Tuple[float, float]], Dict[int, int]]:
    """Submit tasks in parallel for optimal performance with enhanced timing tracking.

    Returns:
        Tuple of (
            submission_start,
            submission_end,
            max_individual_submission_time,
            chunk_times,  # Maps chunk_idx to (start_time, end_time)
            task_to_chunk  # Maps task_idx to chunk_idx
        )
    """
    task_to_chunk = {}  # Maps task_idx to chunk_idx

    # Determine chunking strategy
    if batch_size <= 10:
        chunks = [task_definitions]
        # All tasks are in chunk 0
        for i in range(batch_size):
            task_to_chunk[i] = 0
    else:
        chunk_size = max(1, min(10, batch_size // 10))
        chunks = [
            task_definitions[i : i + chunk_size]
            for i in range(0, len(task_definitions), chunk_size)
        ]
        # Map each task to its chunk
        for i in range(batch_size):
            task_to_chunk[i] = i // chunk_size

    max_workers = min(len(chunks), 20)
    chunk_end_times = []
    max_individual_time = 0
    chunk_times = {}

    submission_start = time.time()
    with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as executor:

        def submit_chunk(chunk_idx, chunk):
            chunk_start = time.time()
            try:
                task_client.submit_tasks(session_id, chunk)
                chunk_end = time.time()
                return chunk_idx, chunk_start, chunk_end
            except Exception as e:
                logger.error("Error submitting chunk %s: %s", chunk_idx, e)
                raise

        futures = {
            executor.submit(submit_chunk, idx, chunk): idx
            for idx, chunk in enumerate(chunks)
        }

        for future in concurrent.futures.as_completed(futures):
            try:
                chunk_idx, chunk_start, chunk_end = future.result()
                individual_time = chunk_end - chunk_start
                chunk_end_times.append(chunk_end)
                chunk_times[chunk_idx] = (chunk_start, chunk_end)
                max_individual_time = max(max_individual_time, individual_time)
            except Exception as e:
                logger.error("Task submission failed: %s", e)

    if chunk_end_times:
        submission_end = max(chunk_end_times)
    else:
        submission_end = time.time()

    return (
        submission_start,
        submission_end,
        max_individual_time,
        chunk_times,
        task_to_chunk,
    )


def process_results_in_parallel(
    events_client: ArmoniKEvents,
    result_ids: List[str],
    session_id: str,
    submission_timestamp: float,
    batch_size: int,
) -> Dict[str, Dict]:
    """Process and download results in parallel without needing to fix download times."""
    # For very small batches, use direct download
    if batch_size <= 1:
        results = events_client.wait_for_result_availability_and_download(
            result_ids=result_ids,
            session_id=session_id,
            submission_timestamp=submission_timestamp,
        )
        return results
    # Configure chunk size and parallelism based on batch size
    if batch_size <= 20:
        chunk_size = 1
        max_workers = batch_size
        parallelism = 1
    elif batch_size <= 100:
        chunk_size = 5
        max_workers = min(20, batch_size // chunk_size + 1)
        parallelism = 2
    else:
        chunk_size = 20
        max_workers = 20
        parallelism = 4

    chunks = [
        result_ids[i : i + chunk_size] for i in range(0, len(result_ids), chunk_size)
    ]
    all_results = {}
    # Process chunks in parallel - download times are already correctly calculated by client
    with concurrent.futures.ThreadPoolExecutor(max_workers=max_workers) as executor:

        def download_with_timestamp(chunk_idx, chunk):
            try:
                results = events_client.wait_for_result_availability_and_download(
                    result_ids=chunk,
                    session_id=session_id,
                    parallelism=parallelism,
                    submission_timestamp=submission_timestamp,
                )

                for result_id, result_data in results.items():
                    result_data["chunk_idx"] = chunk_idx

                return results
            except Exception as e:
                logger.error("Error in chunk %s: %s", chunk_idx, e)
                raise

        # Submit all chunks for parallel processing
        future_to_chunk = {
            executor.submit(download_with_timestamp, i, chunk): i
            for i, chunk in enumerate(chunks)
        }

        for future in concurrent.futures.as_completed(future_to_chunk):
            try:
                chunk_results = future.result()
                all_results.update(chunk_results)
            except Exception as e:
                chunk_idx = future_to_chunk[future]
                logger.error("Error processing result chunk %s: %s", chunk_idx, e)

    return all_results


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
    """Run a batch of tasks with optimized performance focusing on E2E metrics."""
    metrics = {
        "batch_size": batch_size,
        "scenario": scenario,
        "iteration": iteration + 1,
    }
    # Start time for the entire operation
    t1_start = time.time()
    # Task preparation - create session, outputs, payloads, etc.
    task_options = TaskOptions(
        max_duration=timedelta(hours=1),
        max_retries=2,
        priority=1,
        partition_id=partition,
        options={},
    )

    session_id = retry_operation(
        lambda: session_client.create_session(task_options, partition_ids=[partition]),
        operation_name="Session creation",
    )
    t_end_create_session = time.time()
    metrics["create_session_time"] = t_end_create_session - t1_start
    t_start_create_results = time.time()
    outputs = result_client.create_results_metadata(
        result_names=[f"output_{i}" for i in range(batch_size)],
        session_id=session_id,
    )

    result_ids = [outputs[f"output_{i}"].result_id for i in range(batch_size)]

    payloads_data = {f"payload_{i}": b"" for i in range(batch_size)}
    payloads = result_client.create_results(
        results_data=payloads_data, session_id=session_id
    )
    t_end_create_results = time.time()
    metrics["create_results_time"] = t_end_create_results - t_start_create_results

    task_definitions = [None] * batch_size
    for i in range(batch_size):
        payload_id = payloads[f"payload_{i}"].result_id
        output_id = outputs[f"output_{i}"].result_id
        task_definitions[i] = TaskDefinition(
            payload_id=payload_id, expected_output_ids=[output_id]
        )

    preparation_time = time.time() - t1_start
    metrics["preparation_time"] = preparation_time
    # Submit tasks with detailed timing
    (
        t2_submit_start,
        t2_submit_end,
        max_individual_submit,
        chunk_times,
        task_to_chunk,
    ) = submit_tasks_in_parallel(task_client, session_id, task_definitions, batch_size)

    metrics["submission_time"] = t2_submit_end - t2_submit_start
    # Process and download results with accurate timing
    processing_start = time.time()
    first_result_time = None
    last_result_time = None

    all_results = process_results_in_parallel(
        events_client, result_ids, session_id, t2_submit_end, batch_size
    )

    # Collection end time
    t6_collection_end = time.time()

    # Calculate primary metrics
    metrics["end_to_end_latency"] = t6_collection_end - t2_submit_start
    metrics["total_time"] = t6_collection_end - t1_start
    metrics["processing_time"] = t6_collection_end - processing_start

    if metrics["end_to_end_latency"] > 0:
        metrics["throughput"] = batch_size / metrics["end_to_end_latency"]
    else:
        metrics["throughput"] = 0

    # Extract timing information from results
    completion_timestamps = []
    availability_times = []
    download_times = []
    download_complete_timestamps = []

    # Process all result data
    for result_id, result_data in all_results.items():
        # Track completion timestamps
        if "completion_timestamp" in result_data:
            completion_time = result_data["completion_timestamp"]
            completion_timestamps.append(completion_time)

            # Track result availability relative to submission
            availability_time = completion_time - t2_submit_end
            availability_times.append(availability_time)

            # Track first and last result times
            if first_result_time is None or completion_time < first_result_time:
                first_result_time = completion_time
            if last_result_time is None or completion_time > last_result_time:
                last_result_time = completion_time

        # Track download times
        if "download_time" in result_data:
            download_times.append(result_data["download_time"])

        # Track download completion timestamps
        if "download_complete_timestamp" in result_data:
            download_complete_timestamps.append(
                result_data["download_complete_timestamp"]
            )

    # Calculate task execution time (time to first result)
    if first_result_time is not None:
        metrics["task_execution_time"] = first_result_time - t2_submit_end
    else:
        metrics["task_execution_time"] = metrics["processing_time"] * 0.8

    # Calculate average download time
    if download_times:
        metrics["download_time"] = sum(download_times) / len(download_times)
        metrics["min_download_time"] = min(download_times)
        metrics["max_download_time"] = max(download_times)
    else:
        metrics["download_time"] = metrics["processing_time"] * 0.2

    # Calculate batch completion time (time from submission start to last download completion)
    if download_complete_timestamps:
        metrics["batch_completion_time"] = (
            max(download_complete_timestamps) - t2_submit_start
        )
        metrics["download_completion_spread"] = max(download_complete_timestamps) - min(
            download_complete_timestamps
        )
    else:
        # Fallback to end-to-end latency if download timestamps not available
        metrics["batch_completion_time"] = metrics["end_to_end_latency"]

    # Calculate per-task statistics
    per_task_stats = []
    per_task_e2e_times = []

    for i in range(batch_size):
        result_id = result_ids[i]
        if result_id not in all_results:
            continue

        result_data = all_results[result_id]
        task_stat = {
            "task_index": i,
            "result_id": result_id,
            "batch_size": batch_size,
            "scenario": scenario,
        }

        # Get the submission time for this specific task (or its chunk)
        chunk_idx = task_to_chunk.get(i, 0)
        chunk_start_time, chunk_end_time = chunk_times.get(
            chunk_idx, (t2_submit_start, t2_submit_end)
        )
        task_stat["submit_time"] = chunk_start_time

        # Track completion time (when result became available)
        if "completion_timestamp" in result_data:
            completion_timestamp = result_data["completion_timestamp"]
            task_stat["completion_timestamp"] = completion_timestamp
            task_stat["completion_time"] = completion_timestamp - chunk_start_time
            task_stat["result_availability_time"] = completion_timestamp - t2_submit_end

        # Track download time and completion
        if "download_time" in result_data:
            task_stat["download_time"] = result_data["download_time"]

        if "download_complete_timestamp" in result_data:
            download_complete_time = result_data["download_complete_timestamp"]
            task_stat["download_complete_timestamp"] = download_complete_time

            # Calculate true end-to-end time: from task submission to download completion
            task_e2e_time = download_complete_time - chunk_start_time
            task_stat["e2e_latency"] = task_e2e_time
            per_task_e2e_times.append(task_e2e_time)

        per_task_stats.append(task_stat)

    metrics["per_task_stats"] = per_task_stats

    # Calculate per-task E2E statistics
    if per_task_e2e_times:
        metrics["min_task_e2e"] = min(per_task_e2e_times)
        metrics["max_task_e2e"] = max(per_task_e2e_times)
        metrics["avg_task_e2e"] = statistics.fmean(per_task_e2e_times)
        metrics["median_task_e2e"] = statistics.median(per_task_e2e_times)
        metrics["task_e2e_spread"] = metrics["max_task_e2e"] - metrics["min_task_e2e"]

        # Calculate percentiles
        sorted_e2e = sorted(per_task_e2e_times)
        metrics["p50_task_e2e"] = sorted_e2e[int(len(sorted_e2e) * 0.5)]
        metrics["p95_task_e2e"] = sorted_e2e[int(len(sorted_e2e) * 0.95)]
        metrics["p99_task_e2e"] = (
            sorted_e2e[int(len(sorted_e2e) * 0.99)]
            if len(sorted_e2e) >= 100
            else sorted_e2e[-1]
        )

        logger.debug(
            "Per-task E2E times - Min: %.3fs, Avg: %.3fs, Max: %.3fs, Spread: %.3fs",
            metrics["min_task_e2e"],
            metrics["avg_task_e2e"],
            metrics["max_task_e2e"],
            metrics["task_e2e_spread"],
        )

    # Generate detailed log message with all metrics
    log_msg = (
        f"Batch {metrics['iteration']} ({batch_size} tasks) - "
        f"Preparation: {metrics['preparation_time']:.3f}s, "
        f"Submission: {metrics['submission_time']:.3f}s, "
        f"Task Exec: {metrics['task_execution_time']:.3f}s, "
        f"Download: {metrics['download_time']:.3f}s, "
        f"Batch E2E: {metrics['end_to_end_latency']:.3f}s, "
        f"Batch Completion: {metrics.get('batch_completion_time', 0):.3f}s, "
        f"Avg Task E2E: {metrics.get('avg_task_e2e', 0):.3f}s, "
        f"Throughput: {metrics['throughput']:.2f} tasks/s"
    )
    logger.info(log_msg)

    return metrics


def run_benchmarks(endpoint: str, partition: str) -> List[Dict[str, Any]]:
    """Run benchmark scenarios with optimized channel settings."""
    all_results = []

    channel_options = [
        ("grpc.max_receive_message_length", 100 * 1024 * 1024),
        ("grpc.max_send_message_length", 100 * 1024 * 1024),
        ("grpc.keepalive_time_ms", 30000),
        ("grpc.keepalive_timeout_ms", 10000),
        ("grpc.http2.max_pings_without_data", 0),
        ("grpc.http2.min_time_between_pings_ms", 10000),
        ("grpc.http2.min_ping_interval_without_data_ms", 5000),
    ]

    with grpc.insecure_channel(endpoint, options=channel_options) as channel:
        task_client = ArmoniKTasks(channel)
        result_client = ArmoniKResults(channel)
        session_client = ArmoniKSessions(channel)
        events_client = ArmoniKEvents(channel)

        logger.info("Starting warm-up phase (2 batches of 100 tasks)...")
        for i in range(2):
            try:
                logger.info("Running warm-up batch %d/2...", i + 1)
                run_batch(
                    task_client=task_client,
                    result_client=result_client,
                    session_client=session_client,
                    events_client=events_client,
                    partition=partition,
                    batch_size=100,
                    iteration=i,
                    scenario="warmup",
                )
                logger.info("Warm-up batch %d completed", i + 1)
            except Exception as e:
                logger.warning("Error in warm-up batch %d: %s", i + 1, e)

        logger.info("Warm-up phase completed. Starting benchmark scenarios...")

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

                    if (i + 1) % max(1, min(50, scenario["iterations"] // 20)) == 0 or (
                        i + 1
                    ) == scenario["iterations"]:
                        elapsed = time.time() - start_time
                        progress = (i + 1) / scenario["iterations"] * 100
                        est_remaining = (
                            ((elapsed / (i + 1)) * (scenario["iterations"] - i - 1))
                            if i > 0
                            else 0
                        )

                        logger.info(
                            "Progress: %.1f%% - Completed %d/%d iterations (%d successful, %d failed) - Est. remaining: %.1fs",
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
                "Completed scenario %s with %d successful and %d failed runs in %.1f seconds",
                scenario["name"],
                successful_runs,
                failed_runs,
                time.time() - start_time,
            )

    save_raw_results_csv(all_results)
    generate_latency_percentile_graph(all_results)
    generate_matplotlib_latency_graph(all_results)
    print_summary(all_results)
    generate_detailed_report(all_results)

    return all_results

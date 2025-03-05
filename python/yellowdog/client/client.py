import argparse
import csv
import logging
import sys
import time
from concurrent.futures import ThreadPoolExecutor
from datetime import timedelta
from typing import Any, Dict, List

import grpc
import matplotlib.pyplot as plt
import numpy as np
from armonik.client import ArmoniKEvents, ArmoniKResults, ArmoniKSessions, ArmoniKTasks
from armonik.common import TaskDefinition, TaskOptions

# Configure logging
logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)


def run_batch(
    endpoint: str, partition: str, batch_size: int, iteration: int, scenario: str
) -> Dict[str, Any]:
    logger.info(f"Running {scenario} - Iteration {iteration+1}")
    with grpc.insecure_channel(endpoint) as channel:
        task_client = ArmoniKTasks(channel)
        result_client = ArmoniKResults(channel)
        session_client = ArmoniKSessions(channel)
        events_client = ArmoniKEvents(channel)

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

        # Retry logic for session creation
        max_session_attempts = 3
        for attempt in range(1, max_session_attempts + 1):
            try:
                session_id = session_client.create_session(
                    task_options, partition_ids=[partition]
                )
                logger.info(f"Created session with ID: {session_id}")
                break
            except Exception as e:
                logger.error(f"Session creation attempt {attempt} failed: {e}")
                if attempt == max_session_attempts:
                    raise ValueError("Max retries reached for session creation") from e
                time.sleep(2**attempt)

        task_definitions = []
        result_ids = []
        for _ in range(batch_size):
            results = result_client.create_results_metadata(
                result_names=["payload", "result"], session_id=session_id
            )
            payload_id = results["payload"].result_id
            result_id = results["result"].result_id
            result_ids.append(result_id)

            payload = "".encode()

            # Retry logic for payload upload
            max_upload_attempts = 3
            for attempt in range(1, max_upload_attempts + 1):
                try:
                    result_client.upload_result_data(payload_id, session_id, payload)
                    break
                except Exception as e:
                    logger.error(f"Payload upload attempt {attempt} failed: {e}")
                    if attempt == max_upload_attempts:
                        raise ValueError(
                            "Max retries reached during payload upload"
                        ) from e
                    time.sleep(2**attempt)

            task_definitions.append(
                TaskDefinition(payload_id=payload_id, expected_output_ids=[result_id])
            )

        submission_start = time.time()
        task_client.submit_tasks(session_id, task_definitions)
        metrics["submission_time"] = time.time() - submission_start

        processing_start = time.time()
        try:
            events_client.wait_for_result_availability(
                result_ids=result_ids, session_id=session_id
            )
        except Exception as e:
            logger.error(f"Error waiting for results: {e}")
        metrics["processing_time"] = time.time() - processing_start
        metrics["total_time"] = time.time() - start_total

        return metrics


def run_benchmarks(
    endpoint: str, partition: str, max_workers: int = 100
) -> List[Dict[str, Any]]:
    """
    Runs the benchmarking scenarios in parallel.

    Args:
        endpoint: The endpoint for the connection to ArmoniK control plane.
        partition: The name of the partition to which tasks are submitted.
        max_workers: Maximum number of concurrent worker threads.

    Returns:
        List of benchmark results.
    """
    all_results = []

    # Updated scenarios:
    # "1x1000": job with 1 task per run, run 1000 times.
    # "10x500": job with 10 tasks per run, run 1000 times.
    # "100x200": job with 100 tasks per run, run 200 times.
    scenarios = [
        {"name": "1x1000", "batch_size": 1, "iterations": 1000},
        {"name": "10x500", "batch_size": 10, "iterations": 500},
        {"name": "100x200", "batch_size": 100, "iterations": 200},
    ]

    # Process each scenario in sequence, parallelizing iterations within each scenario.
    for scenario in scenarios:
        logger.info(f"Running Scenario: {scenario['name']}")
        scenario_results = []

        # Calculate an appropriate number of workers based on the batch size.
        workers = max(1, min(max_workers, 100 // scenario["batch_size"]))
        logger.info(f"Using {workers} parallel workers for this scenario")

        with ThreadPoolExecutor(max_workers=workers) as executor:
            futures = [
                executor.submit(
                    run_batch,
                    endpoint,
                    partition,
                    scenario["batch_size"],
                    i,
                    scenario["name"],
                )
                for i in range(scenario["iterations"])
            ]
            # Collect results as they complete.
            for future in futures:
                try:
                    result = future.result()
                    scenario_results.append(result)
                except Exception as e:
                    logger.error(f"Error in batch execution: {e}")

        all_results.extend(scenario_results)
        logger.info(f"Completed scenario {scenario['name']}")

    # Save results and plot CDF of scheduling overhead.
    save_results_to_csv(all_results)
    generate_latency_percentile_graph(all_results)

    return all_results


def save_results_to_csv(results: List[Dict[str, Any]]) -> None:
    """Save benchmark results to a CSV file."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename = f"benchmark_results_{timestamp}.csv"

    with open(filename, "w", newline="") as csvfile:
        fieldnames = [
            "scenario",
            "batch_size",
            "iteration",
            "submission_time",
            "processing_time",
            "total_time",
        ]
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for result in results:
            writer.writerow(
                {
                    "scenario": result["scenario"],
                    "batch_size": result["batch_size"],
                    "iteration": result["iteration"],
                    "submission_time": result["submission_time"],
                    "processing_time": result["processing_time"],
                    "total_time": result["total_time"],
                }
            )

    logger.info(f"Results saved to {filename}")


def generate_latency_percentile_graph(results: List[Dict[str, Any]]) -> None:
    """
    Generate a graph showing the CDF of scheduling overhead.
    The x-axis shows the end-to-end latency (from job submission until all results are returned).
    The y-axis shows the percentile (fraction of jobs completed within the given latency).
    Different line styles are used:
      - Solid for "1x1000" (full line)
      - Dashed for "10x500"
      - Dotted for "100x200"

    Args:
        results: List of benchmark results.
    """
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename = f"latency_percentile_{timestamp}.png"

    plt.figure(figsize=(12, 8))

    # Define mapping of scenario names to line styles.
    line_styles = {
        "1x1000": "solid",
        "10x500": "dashed",
        "100x200": "dotted",
    }

    for scenario in ["1x1000", "10x500", "100x200"]:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if not scenario_results:
            logger.warning(f"No results found for scenario {scenario}")
            continue

        latencies = sorted([r["total_time"] for r in scenario_results])
        percentiles = np.linspace(0, 100, len(latencies))
        plt.plot(
            latencies,
            percentiles / 100,
            label=f"Scenario {scenario}",
            linestyle=line_styles[scenario],
            linewidth=2,
        )

    plt.xlabel("Client perceived latency (seconds)")
    plt.ylabel("Percentiles CDF")
    plt.title("End-to-End Latency Distribution (Scheduling Overhead)")
    plt.grid(True, linestyle="--", alpha=0.7)
    plt.legend()
    plt.tight_layout()

    plt.savefig(filename)
    logger.info(f"Latency percentile graph saved to {filename}")


def main(args: List[str]) -> None:
    """
    Parses command-line arguments and runs the benchmarking scenarios.

    Args:
        args: Command-line arguments.
    """
    parser = argparse.ArgumentParser(
        description="Benchmark for YellowDog scheduling overhead using zero-work tasks."
    )
    parser.add_argument(
        "--endpoint",
        type=str,
        default="localhost:5001",
        help="Endpoint for the connection to ArmoniK control plane.",
    )
    parser.add_argument(
        "--partition",
        type=str,
        default="default",
        help="Name of the partition to which tasks are submitted.",
    )
    parser.add_argument(
        "--workers",
        type=int,
        default=10,
        help="Maximum number of parallel workers to use.",
    )
    parsed_args = parser.parse_args(args)

    results = run_benchmarks(
        parsed_args.endpoint, parsed_args.partition, parsed_args.workers
    )

    # Print summary for each scenario.
    for scenario in ["1x1000", "10x500", "100x200"]:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if not scenario_results:
            continue
        avg_submission = sum(r["submission_time"] for r in scenario_results) / len(
            scenario_results
        )
        avg_processing = sum(r["processing_time"] for r in scenario_results) / len(
            scenario_results
        )
        avg_total = sum(r["total_time"] for r in scenario_results) / len(
            scenario_results
        )

        logger.info(f"Scenario {scenario} - Average times:")
        logger.info(f"  Submission: {avg_submission:.4f} seconds")
        logger.info(f"  Processing: {avg_processing:.4f} seconds")
        logger.info(f"  Total: {avg_total:.4f} seconds")


if __name__ == "__main__":
    main(sys.argv[1:])

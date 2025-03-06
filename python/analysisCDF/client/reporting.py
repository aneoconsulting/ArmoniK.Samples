import csv
import time
from typing import Any, Dict, List

from utils import logger


def save_results_to_csv(results: List[Dict[str, Any]]) -> None:
    """Save benchmark results to a CSV file."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename = f"benchmark_results_{timestamp}.csv"

    with open(filename, "w", newline="", encoding="utf-8") as csvfile:
        fieldnames = [
            "scenario",
            "batch_size",
            "iteration",
            "submission_time",
            "processing_time",
            "total_time",
            "tasks_per_second",
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
                    "tasks_per_second": result.get("tasks_per_second", 0),
                }
            )

    logger.info("Results saved to %s", filename)


def print_summary(results: List[Dict[str, Any]]) -> None:
    """Print a summary of benchmark results."""
    print("\n" + "=" * 50)
    print("BENCHMARK SUMMARY")
    print("=" * 50)

    for scenario in ["1x1000", "10x700", "100x300"]:
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
        avg_throughput = sum(
            r["batch_size"] / r["total_time"]
            for r in scenario_results
            if r["total_time"] > 0
        ) / len(scenario_results)

        # Calculate percentiles for more detailed statistics
        total_times = sorted([r["total_time"] for r in scenario_results])
        p50 = total_times[len(total_times) // 2]
        p95 = total_times[int(len(total_times) * 0.95)]
        p99 = total_times[int(len(total_times) * 0.99)]

        print(f"\nScenario {scenario} - {len(scenario_results)} runs:")
        print(f"  Submission time: {avg_submission:.4f}s")
        print(f"  Processing time: {avg_processing:.4f}s")
        print(
            f"  Total latency: {avg_total:.4f}s (p50: {p50:.4f}s, p95: {p95:.4f}s, p99: {p99:.4f}s)"
        )
        print(f"  Throughput: {avg_throughput:.2f} tasks/second")

    print("\n" + "=" * 50)

import csv
import time
from typing import Any, Dict, List

from utils import logger


def save_results_to_csv(results: List[Dict[str, Any]]) -> None:
    """Save benchmark results to a CSV file with all captured metrics."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename = f"benchmark_results_{timestamp}.csv"

    # Get all possible field names from all results (metrics may vary between runs)
    fieldnames = set()
    for result in results:
        fieldnames.update(result.keys())

    # Ensure core fields come first in a specific order
    core_fields = [
        "scenario",
        "batch_size",
        "iteration",
        "submission_time",
        "processing_time",
        "total_time",
        "tasks_per_second",
    ]

    # Add task timing fields
    task_timing_fields = [
        "min_task_time",
        "max_task_time",
        "avg_task_time",
        "median_task_time",
        "p90_task_time",
        "p95_task_time",
        "p99_task_time",
        "stddev_task_time",
        "cv_task_time",
        "outlier_count",
        "outlier_percentage",
    ]

    # Add download metrics fields
    download_fields = [
        "min_download_time",
        "max_download_time",
        "avg_download_time",
        "total_download_time",
        "stddev_download_time",
    ]

    # Add completion timing fields
    completion_fields = [
        "completion_spread",
        "first_result_time",
        "last_result_time",
        "completion_rate",
    ]

    # Add system efficiency fields
    efficiency_fields = [
        "total_task_time",
        "ideal_parallel_time",
        "parallelization_efficiency",
        "system_utilization",
    ]

    # Sort the fields in a logical order
    ordered_fieldnames = []
    for field_list in [
        core_fields,
        task_timing_fields,
        download_fields,
        completion_fields,
        efficiency_fields,
    ]:
        for field in field_list:
            if field in fieldnames:
                ordered_fieldnames.append(field)
                fieldnames.discard(field)

    # Add any remaining fields
    ordered_fieldnames.extend(sorted(fieldnames))

    with open(filename, "w", newline="", encoding="utf-8") as csvfile:
        writer = csv.DictWriter(csvfile, fieldnames=ordered_fieldnames)
        writer.writeheader()
        for result in results:
            writer.writerow(result)

    logger.info("Detailed results saved to %s", filename)

    # Also save a simplified version with just core metrics for easy analysis
    save_simplified_results(results, timestamp)


def save_simplified_results(results: List[Dict[str, Any]], timestamp: str) -> None:
    """Save a simplified version of the results with just the core metrics."""
    filename = f"simplified_results_{timestamp}.csv"

    core_fields = [
        "scenario",
        "batch_size",
        "iteration",
        "submission_time",
        "processing_time",
        "total_time",
        "tasks_per_second",
        "avg_task_time",
        "p95_task_time",
        "system_utilization",
    ]

    with open(filename, "w", newline="", encoding="utf-8") as csvfile:
        writer = csv.DictWriter(csvfile, fieldnames=core_fields)
        writer.writeheader()
        for result in results:
            # Create a new dict with only the core fields
            row = {field: result.get(field, "") for field in core_fields}
            writer.writerow(row)

    logger.info("Simplified results saved to %s", filename)


def print_summary(results: List[Dict[str, Any]]) -> None:
    """Print a summary of benchmark results with enhanced metrics."""
    print("\n" + "=" * 80)
    print("BENCHMARK SUMMARY")
    print("=" * 80)

    for scenario in ["1x1000", "10x700", "100x300"]:
        scenario_results = [r for r in results if r.get("scenario") == scenario]
        if not scenario_results:
            continue

        batch_size = scenario_results[0]["batch_size"]
        print(f"\n{'-' * 70}")
        print(
            f"SCENARIO: {scenario} (Batch Size: {batch_size}, Runs: {len(scenario_results)})"
        )
        print(f"{'-' * 70}")

        # Basic timing metrics
        avg_submission = sum(
            r.get("submission_time", 0) for r in scenario_results
        ) / len(scenario_results)
        avg_processing = sum(
            r.get("processing_time", 0) for r in scenario_results
        ) / len(scenario_results)
        avg_total = sum(r.get("total_time", 0) for r in scenario_results) / len(
            scenario_results
        )
        avg_throughput = sum(
            r["batch_size"] / r["total_time"]
            for r in scenario_results
            if r.get("total_time", 0) > 0
        ) / len(scenario_results)

        print("TIMING METRICS:")
        print(f"  Submission time: {avg_submission:.4f}s")
        print(f"  Processing time: {avg_processing:.4f}s")
        print(f"  Total latency:   {avg_total:.4f}s")
        print(f"  Throughput:      {avg_throughput:.2f} tasks/sec")

        # Task processing metrics (if available)
        if any("avg_task_time" in r for r in scenario_results):
            task_times = [r for r in scenario_results if "avg_task_time" in r]
            avg_task_time = sum(r.get("avg_task_time", 0) for r in task_times) / len(
                task_times
            )
            avg_p95 = (
                sum(
                    r.get("p95_task_time", 0)
                    for r in task_times
                    if "p95_task_time" in r
                )
                / len([r for r in task_times if "p95_task_time" in r])
                if any("p95_task_time" in r for r in task_times)
                else 0
            )

            print("\nTASK METRICS:")
            print(f"  Avg task time:   {avg_task_time:.4f}s")
            print(f"  P95 task time:   {avg_p95:.4f}s")

            # Calculate average of min and max across all runs
            if any("min_task_time" in r for r in scenario_results):
                avg_min = sum(
                    r.get("min_task_time", 0)
                    for r in task_times
                    if "min_task_time" in r
                ) / len([r for r in task_times if "min_task_time" in r])
                avg_max = sum(
                    r.get("max_task_time", 0)
                    for r in task_times
                    if "max_task_time" in r
                ) / len([r for r in task_times if "max_task_time" in r])
                print(f"  Min task time:   {avg_min:.4f}s")
                print(f"  Max task time:   {avg_max:.4f}s")

        # Download metrics (if available)
        if any("avg_download_time" in r for r in scenario_results):
            download_times = [r for r in scenario_results if "avg_download_time" in r]
            avg_download = sum(
                r.get("avg_download_time", 0) for r in download_times
            ) / len(download_times)

            print("\nDOWNLOAD METRICS:")
            print(f"  Avg download:    {avg_download:.4f}s")

        # System efficiency metrics (if available)
        if any("system_utilization" in r for r in scenario_results):
            efficiency_results = [
                r for r in scenario_results if "system_utilization" in r
            ]
            avg_utilization = sum(
                r.get("system_utilization", 0) for r in efficiency_results
            ) / len(efficiency_results)
            avg_efficiency = (
                sum(
                    r.get("parallelization_efficiency", 0)
                    for r in efficiency_results
                    if "parallelization_efficiency" in r
                )
                / len(
                    [r for r in efficiency_results if "parallelization_efficiency" in r]
                )
                if any("parallelization_efficiency" in r for r in efficiency_results)
                else 0
            )

            print("\nEFFICIENCY METRICS:")
            print(f"  System utilization:      {avg_utilization*100:.2f}%")
            print(f"  Parallelization eff.:    {avg_efficiency*100:.2f}%")

    print("\n" + "=" * 80)


def generate_detailed_report(results: List[Dict[str, Any]]) -> None:
    """Generate a detailed HTML report with metrics breakdown."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename = f"detailed_report_{timestamp}.html"

    # Simple HTML report template
    html_content = f"""
    <!DOCTYPE html>
    <html>
    <head>
        <title>ArmoniK Benchmark Report {timestamp}</title>
        <style>
            body {{ font-family: Arial, sans-serif; margin: 20px; }}
            h1 {{ color: #333; }}
            table {{ border-collapse: collapse; width: 100%; }}
            th, td {{ text-align: left; padding: 8px; }}
            tr:nth-child(even) {{ background-color: #f2f2f2; }}
            th {{ background-color: #4CAF50; color: white; }}
            .section {{ margin-bottom: 30px; }}
        </style>
    </head>
    <body>
        <h1>ArmoniK Benchmark Results - {timestamp}</h1>
    """

    # Add summary section for each scenario
    for scenario in ["1x1000", "10x700", "100x300"]:
        scenario_results = [r for r in results if r.get("scenario") == scenario]
        if not scenario_results:
            continue

        batch_size = scenario_results[0]["batch_size"]

        html_content += f"""
        <div class="section">
            <h2>Scenario: {scenario}</h2>
            <p>Batch Size: {batch_size}, Total Runs: {len(scenario_results)}</p>
            
            <h3>Performance Summary</h3>
            <table>
                <tr>
                    <th>Metric</th>
                    <th>Average</th>
                    <th>Min</th>
                    <th>Max</th>
                    <th>P95</th>
                </tr>
        """

        # Add rows for key metrics
        metrics = [
            ("Total Time (s)", "total_time"),
            ("Processing Time (s)", "processing_time"),
            ("Submission Time (s)", "submission_time"),
            ("Tasks per Second", "tasks_per_second"),
            ("Task Time (s)", "avg_task_time"),
            ("Download Time (s)", "avg_download_time"),
            (
                "System Utilization",
                "system_utilization",
                100,
            ),  # Multiply by 100 for percentage
            (
                "Parallelization Efficiency",
                "parallelization_efficiency",
                100,
            ),  # Multiply by 100 for percentage
        ]

        for label, key, *args in metrics:
            multiplier = args[0] if args else 1
            if any(key in r for r in scenario_results):
                valid_results = [r[key] for r in scenario_results if key in r]
                if valid_results:
                    avg_val = sum(valid_results) / len(valid_results) * multiplier
                    min_val = min(valid_results) * multiplier
                    max_val = max(valid_results) * multiplier

                    # Calculate P95
                    sorted_vals = sorted(valid_results)
                    p95_idx = int(len(sorted_vals) * 0.95)
                    p95_val = (
                        sorted_vals[p95_idx] * multiplier
                        if p95_idx < len(sorted_vals)
                        else sorted_vals[-1] * multiplier
                    )

                    html_content += f"""
                    <tr>
                        <td>{label}</td>
                        <td>{avg_val:.4f}</td>
                        <td>{min_val:.4f}</td>
                        <td>{max_val:.4f}</td>
                        <td>{p95_val:.4f}</td>
                    </tr>
                    """

        html_content += """
            </table>
        </div>
        """

    # Close HTML
    html_content += """
    </body>
    </html>
    """

    # Write HTML file
    with open(filename, "w") as f:
        f.write(html_content)

    logger.info("Detailed HTML report generated: %s", filename)

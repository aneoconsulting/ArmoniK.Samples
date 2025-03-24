import time
from typing import Any, Dict, List

import matplotlib.pyplot as plt
import numpy as np
import plotly.graph_objects as go
import plotly.io as pio
from utils import logger


def generate_latency_percentile_graph(results: List[Dict[str, Any]]) -> None:
    """Generate interactive CDF graphs for per-task, batch E2E, and batch completion latencies."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename_per_task_e2e = f"per_task_e2e_cdf_{timestamp}.html"
    filename_batch_e2e = f"batch_e2e_cdf_{timestamp}.html"
    filename_batch_completion = f"batch_completion_cdf_{timestamp}.html"

    fig_per_task_e2e = go.Figure()
    fig_batch_e2e = go.Figure()
    fig_batch_completion = go.Figure()

    colors = {
        "1x1000": "blue",
        "10x700": "red",
        "100x300": "green",
    }

    label_map = {
        "1x1000": "1-task batch",
        "10x700": "10-task batch",
        "100x300": "100-task batch",
    }

    for scenario in ["1x1000", "10x700", "100x300"]:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if not scenario_results:
            logger.warning("No results found for scenario %s", scenario)
            continue

        # 1. Per-task end-to-end latencies (from submission to download)
        all_per_task_e2e_times = []
        for result in scenario_results:
            if "per_task_stats" in result:
                for task_stat in result["per_task_stats"]:
                    if "e2e_latency" in task_stat:
                        all_per_task_e2e_times.append(task_stat["e2e_latency"])

        if all_per_task_e2e_times:
            all_per_task_e2e_times.sort()
            percentiles = np.linspace(0, 100, len(all_per_task_e2e_times)) / 100
            avg_e2e_time = sum(all_per_task_e2e_times) / len(all_per_task_e2e_times)
            display_name = label_map.get(scenario, scenario)

            fig_per_task_e2e.add_trace(
                go.Scatter(
                    x=all_per_task_e2e_times,
                    y=percentiles,
                    mode="lines",
                    name=f"{display_name} (avg: {avg_e2e_time:.3f}s)",
                    line=dict(color=colors.get(scenario, "black"), width=2),
                )
            )

        # 2. Batch end-to-end latencies (submission to all results collected)
        batch_e2e_times = [r.get("end_to_end_latency", 0) for r in scenario_results]

        if batch_e2e_times:
            batch_e2e_times.sort()
            percentiles = np.linspace(0, 100, len(batch_e2e_times)) / 100
            avg_batch_e2e = sum(batch_e2e_times) / len(batch_e2e_times)
            display_name = label_map.get(scenario, scenario)

            fig_batch_e2e.add_trace(
                go.Scatter(
                    x=batch_e2e_times,
                    y=percentiles,
                    mode="lines",
                    name=f"{display_name} (avg: {avg_batch_e2e:.3f}s)",
                    line=dict(color=colors.get(scenario, "black"), width=2),
                )
            )

        # 3. Full batch completion times (submission to last task download completed)
        batch_completion_times = [
            r.get("batch_completion_time", 0)
            for r in scenario_results
            if "batch_completion_time" in r
        ]

        if batch_completion_times:
            batch_completion_times.sort()
            percentiles = np.linspace(0, 100, len(batch_completion_times)) / 100
            avg_completion = sum(batch_completion_times) / len(batch_completion_times)
            display_name = label_map.get(scenario, scenario)

            fig_batch_completion.add_trace(
                go.Scatter(
                    x=batch_completion_times,
                    y=percentiles,
                    mode="lines",
                    name=f"{display_name} (avg: {avg_completion:.3f}s)",
                    line=dict(color=colors.get(scenario, "black"), width=2),
                )
            )

    # Configure layout for all figures
    x_label = "Latency (seconds)"
    y_label = "Percentile (CDF)"

    # Configure layout for per-task E2E figure
    fig_per_task_e2e.update_layout(
        title=dict(
            text="Per-Task End-to-End Latency Distribution (submission to download)",
            font=dict(size=20),
        ),
        xaxis=dict(
            title=dict(text=x_label, font=dict(size=16)),
            tickfont=dict(size=14),
            range=[0, 0.35],  # Set x-axis range to 0-0.35 seconds
            dtick=0.05,  # Set tick spacing to 0.05 seconds
        ),
        yaxis=dict(
            title=dict(text=y_label, font=dict(size=16)),
            range=[0, 1],
            tickfont=dict(size=14),
        ),
        legend=dict(yanchor="top", y=0.99, xanchor="right", x=0.99, font=dict(size=14)),
        height=600,
        width=900,
        template="plotly_white",
        margin=dict(l=80, r=80, t=100, b=80),
    )

    # Configure layout for batch E2E figure
    fig_batch_e2e.update_layout(
        title=dict(
            text="Batch End-to-End Latency Distribution (submission to all results collected)",
            font=dict(size=20),
        ),
        xaxis=dict(
            title=dict(text=x_label, font=dict(size=16)),
            tickfont=dict(size=14),
            range=[0, 0.35],  # Set x-axis range to 0-0.35 seconds
            dtick=0.05,  # Set tick spacing to 0.05 seconds
        ),
        yaxis=dict(
            title=dict(text=y_label, font=dict(size=16)),
            range=[0, 1],
            tickfont=dict(size=14),
        ),
        legend=dict(yanchor="top", y=0.99, xanchor="right", x=0.99, font=dict(size=14)),
        height=600,
        width=900,
        template="plotly_white",
        margin=dict(l=80, r=80, t=100, b=80),
    )

    # Configure layout for batch completion figure
    fig_batch_completion.update_layout(
        title=dict(
            text="Full Batch Completion Time Distribution (submission to last task download)",
            font=dict(size=20),
        ),
        xaxis=dict(
            title=dict(text=x_label, font=dict(size=16)),
            tickfont=dict(size=14),
            range=[0, 0.35],  # Set x-axis range to 0-0.35 seconds
            dtick=0.05,  # Set tick spacing to 0.05 seconds
        ),
        yaxis=dict(
            title=dict(text=y_label, font=dict(size=16)),
            range=[0, 1],
            tickfont=dict(size=14),
        ),
        legend=dict(yanchor="top", y=0.99, xanchor="right", x=0.99, font=dict(size=14)),
        height=600,
        width=900,
        template="plotly_white",
        margin=dict(l=80, r=80, t=100, b=80),
    )

    # Add grid lines and median line to all figures
    for fig in [fig_per_task_e2e, fig_batch_e2e, fig_batch_completion]:
        fig.update_xaxes(gridcolor="lightgray", gridwidth=0.5)
        fig.update_yaxes(gridcolor="lightgray", gridwidth=0.5)
        fig.add_shape(
            type="line",
            x0=0,
            x1=0.35,  # Match x-axis range
            y0=0.5,
            y1=0.5,
            line=dict(color="black", width=1, dash="dash"),
        )

    # Save the figures
    pio.write_html(fig_per_task_e2e, filename_per_task_e2e)
    pio.write_html(fig_batch_e2e, filename_batch_e2e)
    pio.write_html(fig_batch_completion, filename_batch_completion)

    logger.info(
        "Interactive per-task E2E latency graph saved to: %s", filename_per_task_e2e
    )
    logger.info("Interactive batch E2E latency graph saved to: %s", filename_batch_e2e)
    logger.info(
        "Interactive batch completion time graph saved to: %s",
        filename_batch_completion,
    )


def generate_matplotlib_latency_graph(results: List[Dict[str, Any]]) -> None:
    """Generate latency percentile graphs using matplotlib."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename_per_task_e2e = f"mpl_per_task_e2e_{timestamp}.png"
    filename_batch_e2e = f"mpl_batch_e2e_{timestamp}.png"
    filename_batch_completion = f"mpl_batch_completion_{timestamp}.png"  # New file

    colors = {
        "1x1000": "blue",
        "10x700": "red",
        "100x300": "green",
    }

    # Map for improved labels
    label_map = {
        "1x1000": "1-task batch",
        "10x700": "10-task batch",
        "100x300": "100-task batch",
    }

    # Generate per-task E2E CDF
    plt.figure(figsize=(10, 6))
    for scenario in ["1x1000", "10x700", "100x300"]:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if not scenario_results:
            continue

        # Collect all per-task E2E latencies
        latencies = []
        for result in scenario_results:
            if "per_task_stats" in result:
                for task_stat in result["per_task_stats"]:
                    if "e2e_latency" in task_stat:
                        latencies.append(task_stat["e2e_latency"])

        if latencies:
            latencies.sort()
            percentiles = np.linspace(0, 100, len(latencies)) / 100
            avg_latency = sum(latencies) / len(latencies)
            display_name = label_map.get(scenario, scenario)

            plt.plot(
                latencies,
                percentiles,
                "-",
                color=colors.get(scenario, "black"),
                linewidth=2,
                label=f"{display_name} (avg: {avg_latency:.3f}s)",
            )

    # Configure per-task E2E plot
    plt.title(
        "Per-Task End-to-End Latency Distribution", fontsize=16, fontweight="bold"
    )
    plt.xlabel("Latency (seconds)", fontsize=14)
    plt.ylabel("Percentile (CDF)", fontsize=14)
    plt.xlim(0, 0.35)  # Set x-axis range to 0-0.35 seconds
    plt.ylim(0, 1)
    plt.xticks(np.arange(0, 0.36, 0.05))  # Set tick spacing to 0.05 seconds
    plt.grid(True, linestyle="--", alpha=0.7)
    plt.axhline(y=0.5, color="black", linestyle="--", alpha=0.7)
    plt.legend(loc="lower right", frameon=True, framealpha=1, fontsize=12)
    plt.tight_layout()
    plt.savefig(filename_per_task_e2e, dpi=300)
    plt.close()

    # Generate batch E2E CDF
    plt.figure(figsize=(10, 6))
    for scenario in ["1x1000", "10x700", "100x300"]:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if not scenario_results:
            continue

        # Collect batch E2E latencies
        batch_e2e_times = [r.get("end_to_end_latency", 0) for r in scenario_results]

        if batch_e2e_times:
            batch_e2e_times.sort()
            percentiles = np.linspace(0, 100, len(batch_e2e_times)) / 100
            avg_latency = sum(batch_e2e_times) / len(batch_e2e_times)
            display_name = label_map.get(scenario, scenario)

            plt.plot(
                batch_e2e_times,
                percentiles,
                "-",
                color=colors.get(scenario, "black"),
                linewidth=2,
                label=f"{display_name} (avg: {avg_latency:.3f}s)",
            )

    # Configure batch E2E plot
    plt.title("Batch End-to-End Latency Distribution", fontsize=16, fontweight="bold")
    plt.xlabel("Latency (seconds)", fontsize=14)
    plt.ylabel("Percentile (CDF)", fontsize=14)
    plt.xlim(0, 0.35)  # Set x-axis range to 0-0.35 seconds
    plt.ylim(0, 1)
    plt.xticks(np.arange(0, 0.36, 0.05))  # Set tick spacing to 0.05 seconds
    plt.grid(True, linestyle="--", alpha=0.7)
    plt.axhline(y=0.5, color="black", linestyle="--", alpha=0.7)
    plt.legend(loc="lower right", frameon=True, framealpha=1, fontsize=12)
    plt.tight_layout()
    plt.savefig(filename_batch_e2e, dpi=300)
    plt.close()

    # Generate batch completion CDF
    plt.figure(figsize=(10, 6))
    for scenario in ["1x1000", "10x700", "100x300"]:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if not scenario_results:
            continue

        # Collect batch completion times
        completion_times = [
            r.get("batch_completion_time", 0)
            for r in scenario_results
            if "batch_completion_time" in r
        ]

        if completion_times:
            completion_times.sort()
            percentiles = np.linspace(0, 100, len(completion_times)) / 100
            avg_completion = sum(completion_times) / len(completion_times)
            display_name = label_map.get(scenario, scenario)

            plt.plot(
                completion_times,
                percentiles,
                "-",
                color=colors.get(scenario, "black"),
                linewidth=2,
                label=f"{display_name} (avg: {avg_completion:.3f}s)",
            )

    # Configure batch completion plot
    plt.title("Full Batch Completion Time Distribution", fontsize=16, fontweight="bold")
    plt.xlabel("Latency (seconds)", fontsize=14)
    plt.ylabel("Percentile (CDF)", fontsize=14)
    plt.xlim(0, 0.35)  # Set x-axis range to 0-0.35 seconds
    plt.ylim(0, 1)
    plt.xticks(np.arange(0, 0.36, 0.05))  # Set tick spacing to 0.05 seconds
    plt.grid(True, linestyle="--", alpha=0.7)
    plt.axhline(y=0.5, color="black", linestyle="--", alpha=0.7)
    plt.legend(loc="lower right", frameon=True, framealpha=1, fontsize=12)
    plt.tight_layout()
    plt.savefig(filename_batch_completion, dpi=300)
    plt.close()

    logger.info("Matplotlib per-task E2E graph saved to %s", filename_per_task_e2e)
    logger.info("Matplotlib batch E2E graph saved to %s", filename_batch_e2e)
    logger.info(
        "Matplotlib batch completion graph saved to %s", filename_batch_completion
    )

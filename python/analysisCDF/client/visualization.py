import time
from typing import Any, Dict, List

import matplotlib.pyplot as plt
import numpy as np
import plotly.graph_objects as go
import plotly.io as pio
from utils import logger


def generate_latency_percentile_graph(results: List[Dict[str, Any]]) -> None:
    """Generate an interactive graph showing the CDF of scheduling overhead."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename_e2e = f"latency_percentile_e2e_{timestamp}.html"
    filename_task = f"latency_percentile_task_{timestamp}.html"
    filename_normalized = f"latency_percentile_normalized_{timestamp}.html"

    # Create separate figures for different latency metrics
    fig_e2e = go.Figure()
    fig_task = go.Figure()
    fig_normalized = go.Figure()

    colors = {
        "1x1000": "blue",
        "10x700": "red",
        "100x300": "green",
    }

    for scenario in ["1x1000", "10x700", "100x300"]:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if not scenario_results:
            logger.warning("No results found for scenario %s", scenario)
            continue

        batch_size = scenario_results[0]["batch_size"]

        # End-to-end latency (total time per batch)
        e2e_latencies = sorted([r["total_time"] for r in scenario_results])
        e2e_percentiles = np.linspace(0, 100, len(e2e_latencies)) / 100
        avg_e2e_latency = sum(e2e_latencies) / len(e2e_latencies)

        # Task-level latency (avg_task_time - actual time each task took to process)
        task_latencies = sorted(
            [
                r.get("avg_task_time", 0)
                for r in scenario_results
                if "avg_task_time" in r
            ]
        )
        if task_latencies:
            task_percentiles = np.linspace(0, 100, len(task_latencies)) / 100
            avg_task_latency = sum(task_latencies) / len(task_latencies)
        else:
            task_latencies = [0]
            task_percentiles = [0]
            avg_task_latency = 0

        # Normalized latency (total_time/batch_size - fairer comparison across batch sizes)
        normalized_latencies = sorted(
            [r["total_time"] / r["batch_size"] for r in scenario_results]
        )
        normalized_percentiles = np.linspace(0, 100, len(normalized_latencies)) / 100
        avg_normalized_latency = sum(normalized_latencies) / len(normalized_latencies)

        # Add traces to each figure
        fig_e2e.add_trace(
            go.Scatter(
                x=e2e_latencies,
                y=e2e_percentiles,
                mode="lines",
                name=f"{scenario} (avg: {avg_e2e_latency:.3f}s)",
                line=dict(color=colors.get(scenario, "black"), width=2),
            )
        )

        if task_latencies[0] > 0:
            fig_task.add_trace(
                go.Scatter(
                    x=task_latencies,
                    y=task_percentiles,
                    mode="lines",
                    name=f"{scenario} (avg: {avg_task_latency:.3f}s)",
                    line=dict(color=colors.get(scenario, "black"), width=2),
                )
            )

        fig_normalized.add_trace(
            go.Scatter(
                x=normalized_latencies,
                y=normalized_percentiles,
                mode="lines",
                name=f"{scenario} (avg: {avg_normalized_latency:.3f}s)",
                line=dict(color=colors.get(scenario, "black"), width=2),
            )
        )

    # Configure layouts - make legends more prominent
    def _configure_layout(fig, title, graph_type="e2e"):
        # Set x-axis range based on graph type
        if graph_type == "e2e":
            x_range = [0, 3]
            x_dtick = 0.25
            ref_line_x1 = 3
        else:
            x_range = [0, 0.75]  # Shorter x-axis for task and normalized graphs
            x_dtick = 0.05
            ref_line_x1 = 0.75

        fig.update_layout(
            title=dict(text=title, font=dict(size=20)),
            xaxis=dict(title="Latency (seconds)", range=x_range, dtick=x_dtick),
            yaxis=dict(title="Percentile (CDF)", range=[0, 1]),
            legend=dict(
                yanchor="top",
                y=0.99,
                xanchor="right",
                x=0.99,
                font=dict(size=14),
                bordercolor="Black",
                borderwidth=1,
            ),
            grid=dict(rows=1, columns=1),
            height=600,
            width=900,
            template="plotly_white",
            margin=dict(l=80, r=80, t=100, b=80),
        )
        # Add horizontal grid lines
        fig.update_yaxes(gridcolor="lightgray", gridwidth=0.5)
        # Add a reference line at the median
        fig.add_shape(
            type="line",
            x0=0,
            x1=ref_line_x1,
            y0=0.5,
            y1=0.5,
            line=dict(color="black", width=1, dash="dash"),
        )

    _configure_layout(fig_e2e, "End-to-End Batch Latency Distribution", "e2e")
    _configure_layout(fig_task, "Individual Task Latency Distribution", "task")
    _configure_layout(
        fig_normalized,
        "Normalized Per-Task Latency Distribution (Total Time/Batch Size)",
        "normalized",
    )

    # Save all figures (only once)
    pio.write_html(fig_e2e, filename_e2e)
    pio.write_html(fig_task, filename_task)
    pio.write_html(fig_normalized, filename_normalized)

    logger.info("Interactive latency percentile graphs saved to:")
    logger.info("  - End-to-end batch latency: %s", filename_e2e)
    logger.info("  - Individual task latency: %s", filename_task)
    logger.info("  - Normalized per-task latency: %s", filename_normalized)


def generate_matplotlib_latency_graph(results: List[Dict[str, Any]]) -> None:
    """Generate latency percentile graphs using matplotlib."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")

    # Create filenames for different metrics
    filenames = {
        "e2e": f"mpl_latency_e2e_{timestamp}.png",
        "task": f"mpl_latency_task_{timestamp}.png",
        "normalized": f"mpl_latency_normalized_{timestamp}.png",
    }

    titles = {
        "e2e": "End-to-End Batch Latency Distribution",
        "task": "Individual Task Latency Distribution",
        "normalized": "Normalized Per-Task Latency Distribution",
    }

    # Set x-axis limits based on graph type
    x_limits = {
        "e2e": (0, 3),
        "task": (0, 0.75),
        "normalized": (0, 0.75),
    }

    # Set tick spacing based on graph type
    x_ticks = {
        "e2e": np.arange(0, 3, 0.25),
        "task": np.arange(0, 0.75, 0.05),
        "normalized": np.arange(0, 0.75, 0.05),
    }

    colors = {
        "1x1000": "blue",
        "10x700": "red",
        "100x300": "green",
    }

    # Create separate plots for each metric
    for metric_type in ["e2e", "task", "normalized"]:
        plt.figure(figsize=(10, 6))

        for scenario in ["1x1000", "10x700", "100x300"]:
            scenario_results = [r for r in results if r["scenario"] == scenario]
            if not scenario_results:
                logger.warning("No results found for scenario %s", scenario)
                continue

            # Get appropriate latency metric based on type
            if metric_type == "e2e":
                latencies = sorted([r["total_time"] for r in scenario_results])
            elif metric_type == "task":
                latencies = sorted(
                    [
                        r.get("avg_task_time", 0)
                        for r in scenario_results
                        if "avg_task_time" in r
                    ]
                )
            else:  # normalized
                latencies = sorted(
                    [r["total_time"] / r["batch_size"] for r in scenario_results]
                )

            if latencies:
                percentiles = np.linspace(0, 100, len(latencies)) / 100
                avg_latency = sum(latencies) / len(latencies)

                plt.plot(
                    latencies,
                    percentiles,
                    "-",
                    color=colors.get(scenario, "black"),
                    linewidth=2,
                    label=f"{scenario} (avg: {avg_latency:.3f}s)",
                )

        # Set plot styling with optimized limits
        plt.title(titles[metric_type], fontsize=16, fontweight="bold")
        plt.xlabel("Latency (seconds)", fontsize=14)
        plt.ylabel("Percentile (CDF)", fontsize=14)
        plt.xlim(*x_limits[metric_type])
        plt.ylim(0, 1)
        plt.xticks(x_ticks[metric_type])
        plt.grid(True, linestyle="--", alpha=0.7)

        # Add reference line at median
        plt.axhline(y=0.5, color="black", linestyle="--", alpha=0.7)

        # Add legend with improved styling
        plt.legend(loc="lower right", frameon=True, framealpha=1, fontsize=12)
        plt.tight_layout()

        # Save figure
        plt.savefig(filenames[metric_type], dpi=300)
        plt.close()

        logger.info(
            "Matplotlib %s graph saved to %s",
            titles[metric_type],
            filenames[metric_type],
        )


def generate_throughput_graph(results: List[Dict[str, Any]]) -> None:
    """Generate an interactive graph showing the throughput (tasks per second)."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename = f"throughput_{timestamp}.html"

    fig = go.Figure()

    scenarios = ["1x1000", "10x700", "100x300"]
    colors = {
        "1x1000": "blue",
        "10x700": "red",
        "100x300": "green",
    }

    # Calculate all throughputs first to determine appropriate x-axis range
    all_throughputs = []
    for scenario in scenarios:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if scenario_results:
            throughputs = [
                r["batch_size"] / r["total_time"]
                for r in scenario_results
                if r["total_time"] > 0
            ]
            all_throughputs.extend(throughputs)

    # Dynamically set bin range based on actual data
    if all_throughputs:
        max_throughput = max(all_throughputs)
        # Round up to next integer and add a small buffer
        max_bin = min(100, int(max_throughput * 1.1) + 1)
    else:
        max_bin = 10

    # Create bins with appropriate range
    bin_edges = np.linspace(0, max_bin, min(max_bin * 10 + 1, 101))
    bin_width = bin_edges[1] - bin_edges[0]

    for scenario in scenarios:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if not scenario_results:
            continue

        throughputs = [
            r["batch_size"] / r["total_time"]
            for r in scenario_results
            if r["total_time"] > 0
        ]

        avg_throughput = sum(throughputs) / len(throughputs)

        hist, _ = np.histogram(throughputs, bins=bin_edges)
        bin_centers = 0.5 * (bin_edges[:-1] + bin_edges[1:])

        fig.add_trace(
            go.Bar(
                x=bin_centers,
                y=hist,
                name=f"{scenario} (avg: {avg_throughput:.2f} tasks/s)",
                marker_color=colors.get(scenario, "gray"),
                opacity=0.7,
                width=bin_width * 0.9,
            )
        )

    # Configure axes with more prominence
    fig.update_layout(
        title={"text": "Task Processing Throughput Distribution", "font": {"size": 24}},
        xaxis=dict(
            title={"text": "Throughput (tasks per second)", "font": {"size": 18}},
            tickfont={"size": 14},
        ),
        yaxis=dict(
            title={"text": "Frequency", "font": {"size": 18}}, tickfont={"size": 14}
        ),
        barmode="overlay",
        legend=dict(yanchor="top", y=0.99, xanchor="right", x=0.01, font={"size": 14}),
        height=600,
        width=900,
    )

    # Enable better interactivity features
    fig.update_layout(
        hovermode="x unified", hoverlabel=dict(bgcolor="white", font_size=14)
    )

    pio.write_html(fig, filename)
    logger.info("Interactive throughput graph saved to %s", filename)


def generate_matplotlib_throughput_graph(results: List[Dict[str, Any]]) -> None:
    """Generate a throughput distribution graph using matplotlib."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename = f"mpl_throughput_{timestamp}.png"

    plt.figure(figsize=(10, 6))

    scenarios = ["1x1000", "10x700", "100x300"]
    colors = {
        "1x1000": "blue",
        "10x700": "red",
        "100x300": "green",
    }

    # Calculate all throughputs first to determine appropriate x-axis range
    all_throughputs = []
    for scenario in scenarios:
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if scenario_results:
            throughputs = [
                r["batch_size"] / r["total_time"]
                for r in scenario_results
                if r["total_time"] > 0
            ]
            all_throughputs.extend(throughputs)

    # Dynamically set bin range based on actual data
    if all_throughputs:
        max_throughput = max(all_throughputs)
        # Round up to next integer and add a small buffer
        max_bin = min(100, int(max_throughput * 1.1) + 1)
    else:
        max_bin = 10

    # Create bins with appropriate range
    bin_edges = np.linspace(0, max_bin, min(max_bin * 10 + 1, 101))

    for i, scenario in enumerate(scenarios):
        scenario_results = [r for r in results if r["scenario"] == scenario]
        if not scenario_results:
            continue

        throughputs = [
            r["batch_size"] / r["total_time"]
            for r in scenario_results
            if r["total_time"] > 0
        ]

        avg_throughput = sum(throughputs) / len(throughputs)

        hist, _ = np.histogram(throughputs, bins=bin_edges)
        bin_centers = 0.5 * (bin_edges[:-1] + bin_edges[1:])

        plt.bar(
            bin_centers,
            hist,
            width=bin_edges[1] - bin_edges[0],
            color=colors.get(scenario, f"C{i}"),
            alpha=0.7,
            label=f"{scenario} (avg: {avg_throughput:.2f} tasks/s)",
        )

    plt.title("Task Processing Throughput Distribution", fontsize=14)
    plt.xlabel("Throughput (tasks per second)")
    plt.ylabel("Frequency")
    plt.legend(loc="upper right")
    plt.grid(axis="y", linestyle="--", alpha=0.7)
    plt.tight_layout()
    plt.savefig(filename, dpi=300)
    plt.close()
    logger.info("Matplotlib throughput graph saved to %s", filename)

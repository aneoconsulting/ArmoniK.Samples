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
    filename = f"latency_percentile_{timestamp}.html"

    fig = go.Figure()

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

        latencies = sorted([r["total_time"] for r in scenario_results])
        percentiles = np.linspace(0, 100, len(latencies)) / 100
        avg_latency = sum(latencies) / len(latencies)

        fig.add_trace(
            go.Scatter(
                x=latencies,
                y=percentiles,
                mode="lines",
                name=f"{scenario} (avg: {avg_latency:.3f}s)",
                line=dict(color=colors.get(scenario, "black"), width=2),
            )
        )

    # Configure x-axis to show only first 4 seconds with 0.5s intervals
    fig.update_layout(
        title="End-to-End Latency Distribution",
        xaxis=dict(title="End-to-end latency (seconds)", range=[0, 7], dtick=0.25),
        yaxis=dict(title="Percentile (CDF)", range=[0, 1]),
        legend=dict(yanchor="top", y=0.99, xanchor="left", x=0.01),
        grid=dict(rows=1, columns=1),
    )

    # Save as interactive HTML
    pio.write_html(fig, filename)
    logger.info("Interactive latency percentile graph saved to %s", filename)


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
        legend=dict(yanchor="top", y=0.99, xanchor="left", x=0.01, font={"size": 14}),
        height=600,
        width=900,
    )

    # Enable better interactivity features
    fig.update_layout(
        hovermode="x unified", hoverlabel=dict(bgcolor="white", font_size=14)
    )

    pio.write_html(fig, filename)
    logger.info("Interactive throughput graph saved to %s", filename)


def generate_matplotlib_latency_graph(results: List[Dict[str, Any]]) -> None:
    """Generate a latency percentile graph using matplotlib."""
    timestamp = time.strftime("%Y%m%d_%H%M%S")
    filename = f"mpl_latency_percentile_{timestamp}.png"

    plt.figure(figsize=(10, 6))

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

        latencies = sorted([r["total_time"] for r in scenario_results])
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

    plt.title("End-to-End Latency Distribution", fontsize=14)
    plt.xlabel("End-to-end latency (seconds)")
    plt.ylabel("Percentile (CDF)")
    plt.xlim(0, 7)
    plt.ylim(0, 1)
    plt.xticks(np.arange(0, 7.5, 0.5))
    plt.grid(True, linestyle="--", alpha=0.7)
    plt.legend(loc="lower right")
    plt.tight_layout()

    plt.savefig(filename, dpi=300)
    plt.close()
    logger.info("Matplotlib latency percentile graph saved to %s", filename)


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

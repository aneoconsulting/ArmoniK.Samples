"""
ArmoniK Benchmark Suite

A toolkit for benchmarking ArmoniK task processing capabilities
with zero-work tasks to measure scheduling overhead.
"""

__version__ = "1.0.0"

# Import key components for easy access
from .benchmark import run_batch, run_benchmarks

# Expose main entry point
from .main import main
from .reporting import print_summary, save_results_to_csv
from .visualization import (
    generate_latency_percentile_graph,
    generate_matplotlib_latency_graph,
    generate_matplotlib_throughput_graph,
    generate_throughput_graph,
)

__all__ = [
    "run_batch",
    "run_benchmarks",
    "generate_latency_percentile_graph",
    "generate_throughput_graph",
    "generate_matplotlib_latency_graph",
    "generate_matplotlib_throughput_graph",
    "save_results_to_csv",
    "print_summary",
    "main",
]

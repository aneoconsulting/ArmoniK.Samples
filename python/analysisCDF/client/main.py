import argparse
import sys
import time
from typing import List

from benchmark import run_benchmarks
from reporting import print_summary
from utils import logger


def main(args: List[str]) -> None:
    """Parses command-line arguments and runs the benchmarking scenarios."""
    parser = argparse.ArgumentParser(
        description="Benchmark for scheduling overhead using zero-work tasks."
    )
    parser.add_argument(
        "--endpoint",
        type=str,
        default="localhost:7001",
        help="Endpoint for the connection to ArmoniK control plane.",
    )
    parser.add_argument(
        "--partition",
        type=str,
        default="default",
        help="Name of the partition to which tasks are submitted.",
    )
    parser.add_argument(
        "--scenarios",
        type=str,
        default="1x1000,10x700,100x300",
        help="Comma-separated list of scenarios to run (format: batchsize x iterations)",
    )
    parsed_args = parser.parse_args(args)

    start_time = time.time()
    logger.info("Starting benchmark at %s", time.strftime("%Y-%m-%d %H:%M:%S"))

    results = run_benchmarks(parsed_args.endpoint, parsed_args.partition)

    print_summary(results)

    total_duration = time.time() - start_time
    print("\n" + "=" * 50)
    print(f"Benchmark completed in {total_duration:.1f} seconds")
    print("=" * 50)


if __name__ == "__main__":
    main(sys.argv[1:])

#!/usr/bin/env python3

import argparse
import logging
import sys
import time
from dataclasses import dataclass
from datetime import timedelta
from pathlib import Path
from typing import Optional

import grpc
from armonik.client import ArmoniKEvents, ArmoniKResults, ArmoniKSessions, ArmoniKTasks
from armonik.common import TaskDefinition, TaskOptions

# Add the common directory to the system path
common_path = Path(__file__).resolve().parent.parent / "common"
sys.path.append(str(common_path))

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(name)s: %(message)s",
)
logger = logging.getLogger("CommandLineClient")


@dataclass
class CommandResult:
    """Container for command execution results"""

    success: bool
    output: str
    execution_time: float
    task_id: Optional[str] = None
    error_message: Optional[str] = None


class ArmoniKCommandClient:
    """Client for submitting commands to ArmoniK and processing results"""

    def __init__(
        self,
        endpoint: str = "localhost:5001",
        partition: str = "default",
        timeout: int = 300,
        verbose: bool = False,
    ):
        self.endpoint = endpoint
        self.partition = partition
        self.timeout = timeout
        self.verbose = verbose

        self.channel = None
        self.tasks_client = None
        self.results_client = None
        self.sessions_client = None
        self.events_client = None

        # Initialize connections
        self._connect()

    def _connect(self) -> None:
        """Establish connection to ArmoniK services"""
        try:
            self.channel = grpc.insecure_channel(self.endpoint)
            self.tasks_client = ArmoniKTasks(self.channel)
            self.results_client = ArmoniKResults(self.channel)
            self.sessions_client = ArmoniKSessions(self.channel)
            self.events_client = ArmoniKEvents(self.channel)

            if self.verbose:
                logger.info("Connected to ArmoniK at %s", self.endpoint)
        except Exception as e:
            logger.error("Failed to connect to ArmoniK: %s", str(e))
            raise

    def _create_session(self) -> str:
        """Create a new ArmoniK session"""
        task_options = TaskOptions(
            max_duration=timedelta(hours=1),
            max_retries=2,
            priority=1,
            partition_id=self.partition,
        )

        session_id = self.sessions_client.create_session(
            task_options, partition_ids=[self.partition]
        )

        if self.verbose:
            logger.info("Created session: %s", session_id)

        return session_id

    def run_commands(self, commands: str) -> CommandResult:
        """
        Submit the commands to ArmoniK and wait for results.

        Args:
            commands: Command string to execute (multiple commands separated by newlines)

        Returns:
            CommandResult object containing execution results
        """
        if self.verbose:
            logger.info("Preparing to execute commands:\n%s", commands)

        start_time = time.time()

        try:
            session_id = self._create_session()

            output_result = self.results_client.create_results_metadata(
                result_names=["payload", "result"], session_id=session_id
            )
            payload_id = output_result["payload"].result_id
            result_id = output_result["result"].result_id

            payload_data = commands.encode("utf-8")
            try:
                self.results_client.upload_result_data(
                    payload_id, session_id, payload_data
                )
                if self.verbose:
                    logger.info("Uploaded command payload with ID: %s", payload_id)
            except Exception as e:
                return CommandResult(
                    success=False,
                    output="",
                    execution_time=time.time() - start_time,
                    error_message=f"Error uploading payload: {str(e)}",
                )

            task_definition = TaskDefinition(
                payload_id=payload_id,
                expected_output_ids=[result_id],
            )

            task_id = self.tasks_client.submit_tasks(session_id, [task_definition])[0]

            if self.verbose:
                logger.info("Task submitted with ID: %s", task_id)
                logger.info("Waiting for results (timeout: %ss)...", self.timeout)

            t_wait_start = time.time()

            try:
                self.events_client.wait_for_result_availability(
                    result_ids=[result_id],
                    session_id=session_id,
                )

                if self.verbose:
                    wait_time = time.time() - t_wait_start
                    logger.info("Results available in %.2f seconds", wait_time)

                result_data = self.results_client.download_result_data(
                    result_id, session_id
                ).decode("utf-8")

                execution_time = time.time() - start_time

                return CommandResult(
                    success=True,
                    output=result_data,
                    execution_time=execution_time,
                    task_id=task_id,
                )

            except Exception as e:
                return CommandResult(
                    success=False,
                    output="",
                    execution_time=time.time() - start_time,
                    task_id=task_id,
                    error_message=f"Error waiting for or downloading results: {str(e)}",
                )

        except Exception as e:
            return CommandResult(
                success=False,
                output="",
                execution_time=time.time() - start_time,
                error_message=f"Error: {str(e)}",
            )

    def close(self) -> None:
        """Close the client connections"""
        if self.channel:
            self.channel.close()


def main():
    """Main entry point for the command line client"""
    parser = argparse.ArgumentParser(
        description="ArmoniK Command Line Client",
        formatter_class=argparse.ArgumentDefaultsHelpFormatter,
    )

    parser.add_argument(
        "--endpoint",
        type=str,
        default="localhost:5001",
        help="gRPC endpoint for ArmoniK connection",
    )
    parser.add_argument(
        "--partition",
        type=str,
        default="default",
        help="Partition ID to use for task execution",
    )
    parser.add_argument(
        "--timeout",
        type=int,
        default=300,
        help="Maximum time to wait for results (in seconds)",
    )
    parser.add_argument(
        "--verbose", action="store_true", help="Print detailed information"
    )
    parser.add_argument("--file", type=str, help="File containing commands to execute")
    parser.add_argument(
        "--command", type=str, help="Command string to execute (use quotes)"
    )
    parser.add_argument(
        "--output", type=str, help="File to write command output (default: stdout)"
    )

    args = parser.parse_args()

    if args.verbose:
        logging.getLogger().setLevel(logging.DEBUG)

    if args.file:
        try:
            with open(args.file, "r", encoding="utf-8") as f:
                commands = f.read()
        except Exception as e:
            logger.error("Error reading command file: %s", str(e))
            return 1
    elif args.command:
        commands = args.command
    else:
        print("Enter commands (Ctrl+D to finish):")
        commands = sys.stdin.read()

        if not commands.strip():
            logger.error("Error: No commands provided")
            return 1

    client = ArmoniKCommandClient(
        endpoint=args.endpoint,
        partition=args.partition,
        timeout=args.timeout,
        verbose=args.verbose,
    )

    try:
        result = client.run_commands(commands)

        if result.success:
            if args.output:
                with open(args.output, "w", encoding="utf-8") as f:
                    f.write(result.output)
                print(f"Command output saved to {args.output}")
            else:
                print(result.output, end="")
                if args.verbose:
                    print(
                        f"Command execution completed in {result.execution_time:.2f}s",
                        file=sys.stderr,
                    )
        else:
            logger.error("Command execution failed: %s", result.error_message)

        return 0 if result.success else 1

    finally:
        client.close()


if __name__ == "__main__":
    sys.exit(main())

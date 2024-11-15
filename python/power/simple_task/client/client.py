import argparse
import logging
import sys
from datetime import timedelta
from typing import List

import grpc
import grpc.aio
from armonik.client import ArmoniKEvents, ArmoniKResults, ArmoniKSessions, ArmoniKTasks
from armonik.common import TaskDefinition, TaskOptions

# Configure logging
logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)


def run(endpoint: str, partition: str, base: int, exponent: int) -> None:
    """
    Connects to the ArmoniK control plane via a gRPC channel and performs a series of tasks.

    Args:
        endpoint: The endpoint for the connection to ArmoniK control plane.
        partition: The name of the partition to which tasks are submitted.
        base: the base of the power expression
        exponent: the exponent of the power expression

    Example:
        run("172.24.55.197:5001", "default", 2, 3)
    """
    # Create gRPC channel to connect with ArmoniK control plane
    with grpc.insecure_channel(endpoint) as channel:
        task_client = ArmoniKTasks(channel)
        result_client = ArmoniKResults(channel)
        session_client = ArmoniKSessions(channel)
        events_client = ArmoniKEvents(channel)

        # Default task options that will be used by each task if not overwritten when submitting tasks
        task_options = TaskOptions(
            max_duration=timedelta(hours=1),  # Duration of 1 hour
            max_retries=2,
            priority=1,
            partition_id=partition,
            options={"base": str(base), "exponent": str(exponent)},
        )

        # Request for session creation with default task options and allowed partitions for the session
        try:
            session_id = session_client.create_session(
                task_options, partition_ids=[partition]
            )
            logger.info("Created session with ID: %s", session_id)

            logger.info("sessionId: %s", session_id)
        except Exception as e:
            logger.error("An error occurred while creating the session: %s", e)
            raise ValueError("Task failed: session creation")

        results = result_client.create_results_metadata(
            result_names=["payload", "result"], session_id=session_id
        )

        payload_id = results["payload"].result_id
        result_id = results["result"].result_id

        payload = f"{base},{exponent}".encode()
        try:
            result_client.upload_result_data(payload_id, session_id, payload)
        except Exception as e:
            logger.error("An error occurred while uploading the payload: %s", e)
            raise ValueError("Task failed: payload upload")

        task_definition = TaskDefinition(
            payload_id=payload_id,
            expected_output_ids=[result_id],
        )

        task_client.submit_tasks(session_id, [task_definition])

        try:
            events_client.wait_for_result_availability(
                result_ids=[result_id], session_id=session_id
            )
        except Exception as e:
            logger.error(f"An error occurred: {e}")
        try:
            result_data = result_client.download_result_data(result_id, session_id)
            result = result_data.decode()

            expected = base**exponent
            if result:
                if result == str(expected):
                    logger.info(
                        "Task completed successfully: %d^%d = %s",
                        base,
                        exponent,
                        result,
                    )
                else:
                    logger.error(
                        "Arithmetic error: %d^%d = %d, expected %d",
                        base,
                        exponent,
                        result,
                        expected,
                    )
            else:
                raise ValueError("Task failed: result is None")
        except Exception as e:
            logger.error(f"An error occurred: {e}")

        logger.info("Deleting session")

        session_client.close_session(session_id)
        session_client.purge_session(session_id)
        session_client.delete_session(session_id)


def main(args: List[str]) -> None:
    """
    Parses command-line arguments and runs the Power demo for ArmoniK.

    Args:
        args: Command-line arguments.

    Example:
        python client.py --endpoint 172.30.209.227:5001 --partition default --base 2 --exponent 3
    """
    parser = argparse.ArgumentParser(
        description="Power demo for ArmoniK.\nIt sends a task to ArmoniK in the given partition. The task receives x and y as input numbers and, for the result that will be returned by the task, compute the x power y and the resultID to the input. Then, the client retrieves and prints the result of the task.\nArmoniK endpoint location is provided through --endpoint."
    )
    parser.add_argument(
        "--endpoint",
        type=str,
        default="172.24.55.197:5001",
        help="Endpoint for the connection to ArmoniK control plane.",
    )
    parser.add_argument(
        "--partition",
        type=str,
        default="default",
        help="Name of the partition to which submit tasks.",
    )
    parser.add_argument(
        "--base",
        type=int,
        default=1,
        help="Base value for the power computation.",
    )
    parser.add_argument(
        "--exponent",
        type=int,
        default=1,
        help="Exponent value for the power computation.",
    )
    args = parser.parse_args(args)
    run(args.endpoint, args.partition, args.base, args.exponent)


if __name__ == "__main__":
    main(sys.argv[1:])

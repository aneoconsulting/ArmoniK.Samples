import argparse
import logging
from typing import List
import grpc
import grpc.aio
from datetime import timedelta
import sys
from armonik.client import ArmoniKResults, ArmoniKSessions, ArmoniKTasks, ArmoniKEvents
from armonik.common import TaskDefinition, TaskOptions
from pathlib import Path

# Configure logging
logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

# Add the common directory to the system path
common_path = Path(__file__).resolve().parent.parent / "common"
sys.path.append(str(common_path))

from common import NameIdDict


def run(endpoint: str, partition: str) -> None:
    """
    Connects to the ArmoniK control plane via a gRPC channel and performs a series of tasks.

    Args:
        endpoint: The endpoint for the connection to ArmoniK control plane.
        partition: The name of the partition to which tasks are submitted.

    Example:
        run("172.24.55.197:5001", "default")
    """
    # Create gRPC channel to connect with ArmoniK control plane
    with grpc.insecure_channel(endpoint) as channel:
        # Create client for task submission
        task_client = ArmoniKTasks(channel)

        # Create client for result creation
        result_client = ArmoniKResults(channel)

        # Create client for session creation
        sessions_client = ArmoniKSessions(channel)

        # Create client for events listening
        events_client = ArmoniKEvents(channel)

        # Default task options that will be used by each task if not overwritten when submitting tasks
        task_options = TaskOptions(
            max_duration=timedelta(hours=1),  # Duration of 1 hour
            max_retries=2,
            priority=1,
            partition_id=partition,
        )

        # Request for session creation with default task options and allowed partitions for the session
        session_id = sessions_client.create_session(
            task_options, partition_ids=[partition]
        )
        logger.info(f"sessionId: {session_id}")

        # Create the result metadata and keep the id for task submission
        results = result_client.create_results_metadata(
            result_names=["input", "output", "payload"], session_id=session_id
        )

        # Get the results ids
        input_id = results["input"].result_id
        output_id = results["output"].result_id
        payload_id = results["payload"].result_id

        # Create the metadata (a result) and upload data at the same time
        result_client.upload_result_data(
            result_id=input_id,
            session_id=session_id,
            result_data=b"Hello",
        )
        logger.info("data uploaded")

        # Creating a NameIdDict instance
        name_id_mapping = NameIdDict({"input": input_id, "output": output_id})

        # Serializing the instance
        serialized_name_id_mapping = name_id_mapping.serialize()

        result_client.upload_result_data(
            result_id=payload_id,
            session_id=session_id,
            result_data=serialized_name_id_mapping,
        )
        logger.info("payload uploaded")

        # Submit task with payload and result ids
        task_client.submit_tasks(
            session_id=session_id,
            tasks=[
                TaskDefinition(
                    data_dependencies=[input_id],
                    expected_output_ids=[output_id],
                    payload_id=payload_id,
                )
            ],
        )
        logger.info("tasks submitted")

        # Wait for task end and result availability
        try:
            events_client.wait_for_result_availability(
                result_ids=[output_id], session_id=session_id
            )
        except Exception as e:
            logger.error(f"An error occurred: {e}")

        # Download result
        try:
            serialized_result = result_client.download_result_data(
                output_id, session_id
            )
            final_result = serialized_result.decode()
            logger.info(f"resultId: {output_id}, data: {final_result}")
        except Exception as e:
            logger.error(f"An error occurred: {e.details()}")

    logger.info("End Connection!")


def main(args: List[str]) -> None:
    """
    Parses command-line arguments and runs the Hello World demo for ArmoniK.

    Args:
        args: Command-line arguments.

    Example:
        python client.py --endpoint 172.24.55.197:5001 --partition default
    """
    parser = argparse.ArgumentParser(
        description="Hello World demo for ArmoniK.\nIt sends a task to ArmoniK in the given partition. The task receives 'Hello' as input string and, for the result that will be returned by the task, append the word 'World' and the resultID to the input. Then, the client retrieves and prints the result of the task.\nArmoniK endpoint location is provided through --endpoint."
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
    parsed_args = parser.parse_args(args)
    run(parsed_args.endpoint, parsed_args.partition)


if __name__ == "__main__":
    main(sys.argv[1:])

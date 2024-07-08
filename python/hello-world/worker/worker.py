import logging
import os
import sys
import grpc

from armonik.worker import ArmoniKWorker, TaskHandler, ClefLogger
from armonik.common import Output
from pathlib import Path

# Add the common directory to the system path
common_path = Path(__file__).resolve().parent.parent / "common"
sys.path.append(str(common_path))


from common import NameIdDict

ClefLogger.setup_logging(logging.INFO)


# Task processing
def processor(task_handler: TaskHandler) -> Output:
    """
    Processes a task by appending 'world!' to the input string and sending the result.

    Args:
        task_handler: The handler for the current task.

    Returns:
        Output: The result of the task processing.
    """
    logger = ClefLogger.getLogger("ArmoniKWorker")
    logger.info("Handeling the Task")
    payload = task_handler.payload

    name_id_mapping = NameIdDict.deserialize(payload).data

    encoded_data = task_handler.data_dependencies[name_id_mapping["input"]]

    # We convert the binary data from the handler back to the string sent by the client
    input = encoded_data.decode()
    output = input + " world!"

    result_id = task_handler.expected_results.pop(0)

    task_handler.send_results({result_id: output.encode()})

    return Output()


def main():
    """
    Initializes and starts the ArmoniK worker.

    This function creates defines the communication endpoints
    for the agent and worker, and starts the worker. The worker connects to the agent
    and begins processing tasks using the specified processor function.

    Environment Variables:
        ComputePlane__WorkerChannel__SocketType (str): The socket type for worker communication ('unixdomainsocket' or 'tcp').
        ComputePlane__WorkerChannel__Address (str): The address for the worker endpoint.
        ComputePlane__AgentChannel__SocketType (str): The socket type for agent communication ('unixdomainsocket' or 'tcp').
        ComputePlane__AgentChannel__Address (str): The address for the agent endpoint.

    Example:
        python worker.py
    """
    # Create Seq compatible logger
    logger = ClefLogger.getLogger("ArmoniKWorker")

    # Define agent-worker communication endpoints
    worker_scheme = (
        "unix://"
        if os.getenv("ComputePlane__WorkerChannel__SocketType", "unixdomainsocket")
        == "unixdomainsocket"
        else "http://"
    )
    agent_scheme = (
        "unix://"
        if os.getenv("ComputePlane__AgentChannel__SocketType", "unixdomainsocket")
        == "unixdomainsocket"
        else "http://"
    )
    worker_endpoint = worker_scheme + os.getenv(
        "ComputePlane__WorkerChannel__Address", "/cache/armonik_worker.sock"
    )
    agent_endpoint = agent_scheme + os.getenv(
        "ComputePlane__AgentChannel__Address", "/cache/armonik_agent.sock"
    )

    # Start worker
    logger.info("Started new worker!")
    # Use options to fix Unix socket connection on localhost (cf: <GitHub>)
    with grpc.insecure_channel(
        agent_endpoint, options=(("grpc.default_authority", "localhost"),)
    ) as agent_channel:
        worker = ArmoniKWorker(agent_channel, processor, logger=logger)
        logger.info("Worker Connected")
        worker.start(worker_endpoint)


if __name__ == "__main__":
    main()

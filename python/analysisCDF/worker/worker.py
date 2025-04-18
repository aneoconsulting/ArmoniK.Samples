import logging
import os
import sys
from pathlib import Path

import grpc
from armonik.common import Output
from armonik.worker import ArmoniKWorker, ClefLogger, TaskHandler

# Add the common directory to the system path
common_path = Path(__file__).resolve().parent.parent / "common"
sys.path.append(str(common_path))


ClefLogger.setup_logging(logging.INFO)


# Task processing
def processor(task_handler: TaskHandler) -> Output:
    """
    Processes a task by doing nothing.

    Args:
        task_handler: The handler for the current task.

    Returns:
        Output: The result of the task processing.
    """
    logger = ClefLogger.getLogger("ArmoniKWorker")
    logger.info("Handling the Task")

    payload = task_handler.payload
    logger.info(f"Received payload: {payload}")
    result = ""
    # Return the result as Output
    task_handler.send_results({task_handler.expected_results[0]: result.encode()})
    return Output()


def main():
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
    logger.info("Worker Started")
    # Use options to fix Unix socket connection on localhost (cf: <GitHub>)
    with grpc.insecure_channel(
        agent_endpoint, options=(("grpc.default_authority", "localhost"),)
    ) as agent_channel:
        worker = ArmoniKWorker(agent_channel, processor, logger=logger)
        logger.info("Worker Connected")
        worker.start(worker_endpoint)


if __name__ == "__main__":
    main()

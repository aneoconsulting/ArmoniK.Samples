import logging
import os
import subprocess
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
    Processes a task by executing a command script from the payload and returning only stdout.

    Args:
        task_handler: The handler for the current task.

    Returns:
        Output: The result of the task processing.
    """
    logger = ClefLogger.getLogger("ArmoniKWorker")
    logger.info("Handling the Task")

    # Get the payload and decode it to string
    payload = task_handler.payload
    logger.info(f"Received payload: {payload}")

    try:
        # Decode the payload to get the complete script
        command_script = payload.decode("utf-8")
        logger.info(f"Executing script: {command_script}")

        try:
            # Execute the entire script as a single command and capture the output
            result = subprocess.run(
                command_script,
                shell=True,
                capture_output=True,
                text=True,
                timeout=300,
                check=False,
            )

            # Only use the stdout output
            final_output = result.stdout

            # Log information for debugging but don't include in the output
            logger.info(f"Script return code: {result.returncode}")
            if result.stderr:
                logger.info(f"Script stderr: {result.stderr}")

        except subprocess.TimeoutExpired:
            logger.error("Script execution timed out")
            final_output = ""

        except Exception as e:
            logger.error(f"Error executing script: {str(e)}")
            final_output = ""

        # Send only the stdout output back
        task_handler.send_results(
            {task_handler.expected_results[0]: final_output.encode()}
        )
        return Output()

    except Exception as e:
        logger.error(f"Error processing payload: {str(e)}")
        task_handler.send_results({task_handler.expected_results[0]: b""})
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

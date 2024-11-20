import logging
import os

import grpc
from armonik.common import Output, TaskDefinition, TaskOptions
from armonik.worker import ArmoniKWorker, ClefLogger, TaskHandler

# Configure logging
ClefLogger.setup_logging(logging.INFO)
logger = ClefLogger.getLogger("ArmoniKWorker")


def processor(task_handler) -> Output:
    logger.info("Handling the Task")
    # Parse the payload
    payload_parts = task_handler.payload.decode().split(",")
    base = int(payload_parts[0])
    exponent = int(payload_parts[1])
    # Initialize product, default to 1 if not provided
    product = int(payload_parts[2]) if len(payload_parts) > 2 else 1
    negative_exponent = payload_parts[3] == "True" if len(payload_parts) > 3 else False

    logger.info(f"Received payload: {payload_parts}")
    logger.info(f"Base: {base}, Exponent: {exponent}, Product: {product}")

    # Handle negative exponents
    if exponent < 0:
        exponent = -exponent
        negative_exponent = True
        if base == 0:
            error_message = "Undefined result: 0 raised to a negative exponent."
            logger.error(error_message)
            task_handler.send_results(
                {task_handler.expected_results[0]: error_message.encode()}
            )
            return Output()

    # Base case: exponent is zero
    if len(payload_parts) < 3:
        if exponent == 0:
            result = 1
            logger.info(
                f"Computed {base}^{'-' if negative_exponent else ''}0 = {result}"
            )
            task_handler.send_results(
                {task_handler.expected_results[0]: str(result).encode()}
            )
            return Output()
        elif exponent > 0 and base == 0:
            result = 0
            logger.info(f"Computed {base}^{exponent} = {result}")
            task_handler.send_results(
                {task_handler.expected_results[0]: str(result).encode()}
            )
            return Output()
        elif base == 1:
            result = 1
            logger.info(f"Computed {base}^{exponent} = {result}")
            task_handler.send_results(
                {task_handler.expected_results[0]: str(result).encode()}
            )
            return Output()

    if exponent == 0:
        result = product
        if negative_exponent:
            result = 1 / result
        logger.info(f"Computed {base}^{'-' if negative_exponent else ''}0 = {result}")
        task_handler.send_results(
            {task_handler.expected_results[0]: str(result).encode()}
        )
        return Output()

    # Recursive case: create a new subtask
    new_exponent = exponent - 1
    new_product = product * base

    # Use the same expected_results so the final result is sent back correctly
    new_payload = task_handler.create_results(
        {"payload": f"{base},{new_exponent},{new_product},{negative_exponent}".encode()}
    )
    subtask = TaskDefinition(
        new_payload["payload"].result_id, task_handler.expected_results
    )

    logger.info(
        f"Submitting subtask with exponent {new_exponent} and product {new_product}"
    )
    task_handler.submit_tasks([subtask])
    return Output()


def main():
    """Main entry point to start the worker."""
    logger.info("Worker Started")
    # Define communication endpoints
    scheme = (
        "unix://"
        if os.getenv("ComputePlane__WorkerChannel__SocketType", "unixdomainsocket")
        == "unixdomainsocket"
        else "http://"
    )
    worker_endpoint = scheme + os.getenv(
        "ComputePlane__WorkerChannel__Address", "/cache/armonik_worker.sock"
    )
    agent_endpoint = scheme + os.getenv(
        "ComputePlane__AgentChannel__Address", "/cache/armonik_agent.sock"
    )

    logger.info(f"Worker endpoint: {worker_endpoint}")
    logger.info(f"Agent endpoint: {agent_endpoint}")

    with grpc.insecure_channel(
        agent_endpoint, options=(("grpc.default_authority", "localhost"),)
    ) as agent_channel:
        worker = ArmoniKWorker(agent_channel, processor, logger=logger)
        logger.info("Worker Connected")
        worker.start(worker_endpoint)


if __name__ == "__main__":
    main()

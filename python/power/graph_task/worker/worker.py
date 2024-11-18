import logging
import os

import grpc
from armonik.common import Output, TaskDefinition, TaskOptions
from armonik.worker import ArmoniKWorker, ClefLogger, TaskHandler

# Configure logging
ClefLogger.setup_logging(logging.INFO)
logger = ClefLogger.getLogger("ArmoniKWorker")


def processor(task_handler: TaskHandler) -> Output:
    """
    Processes a task to compute the power of a base number raised to an exponent,
    using recursive subtasking to handle large exponents.
    """
    logger.info("Handling the Task")
    payload = task_handler.payload
    payload = payload.decode().split(",")

    logger.info(f"Received payload: {payload}")

    if payload and len(payload) == 2:
        # Computation task: extract base and exponent from payload
        base = int(payload[0])
        exponent = int(payload[1])
        logger.info(f"Base: {base}, Exponent: {exponent}")

        # Handle negative exponent
        negative_exponent = False
        if exponent < 0:
            negative_exponent = True
            exponent = -exponent
            logger.info(
                "Negative exponent detected, will compute reciprocal at the end"
            )

        if exponent == 0:
            result = 1
            logger.info(
                f"Computed {base}^{'-' if negative_exponent else ''}{exponent} = {result}"
            )
            task_handler.send_results(
                {task_handler.expected_results[0]: str(result).encode()}
            )

        if base == 0:
            if exponent > 0:
                result = 0
                logger.info(f"Computed {base}^{exponent} = {result}")
                task_handler.send_results(
                    {task_handler.expected_results[0]: str(result).encode()}
                )
            else:
                error_message = "Undefined result: 0 raised to a negative exponent."
                logger.error(error_message)
                task_handler.send_results(
                    {task_handler.expected_results[0]: error_message.encode()}
                )

        elif exponent == 1 and base != 0:
            result = base
            if negative_exponent:
                result = 1 / result
            logger.info(
                f"Computed {base}^{'-' if negative_exponent else ''}{exponent} = {result}"
            )
            task_handler.send_results(
                {task_handler.expected_results[0]: str(result).encode()}
            )
        elif exponent == 2:
            result = base * base
            if negative_exponent:
                result = 1 / result
            logger.info(
                f"Computed {base}^{'-' if negative_exponent else ''}{exponent} = {result}"
            )
            task_handler.send_results(
                {task_handler.expected_results[0]: str(result).encode()}
            )
        else:
            parallel_tasks = None
            odd = exponent % 2 != 0
            if exponent % 2 == 0:
                parallel_tasks = exponent // 2
            else:
                parallel_tasks = (exponent - 1) // 2

            logger.info(f"Parallel tasks: {parallel_tasks}")

            subtasks = []
            new_results = task_handler.create_results_metadata(
                [f"sub_result_{i}" for i in range(parallel_tasks)]
            )
            for i in range(parallel_tasks):
                new_payload = task_handler.create_results(
                    {
                        "payload": f"{base},{2}".encode(),
                        "aggregation_payload": f"{base},{odd},{negative_exponent}".encode(),
                    }
                )

                subtask = TaskDefinition(
                    new_payload["payload"].result_id,
                    [new_results[f"sub_result_{i}"].result_id],
                )
                subtasks.append(subtask)

            sub_results_ids = [
                new_results[f"sub_result_{i}"].result_id for i in range(parallel_tasks)
            ]
            aggregate = TaskDefinition(
                new_payload["aggregation_payload"].result_id,
                task_handler.expected_results,
                data_dependencies=sub_results_ids,
            )
            subtasks.append(aggregate)
            task_handler.submit_tasks(subtasks)
    else:
        logger.info("Processing subtask result")
        keys = list(task_handler.data_dependencies.keys())
        sub_results = []
        for key in keys:
            sub_results.append(int(task_handler.data_dependencies[key].decode()))

        result = 1
        for sub_result in sub_results:
            result *= sub_result

        logger.info(f"Computed result: {result}")

        options = task_handler.payload.decode().split(",")
        base = int(options[0])
        odd = options[1] == "True"
        negative_exponent = options[2] == "True"

        # If exponent was odd, multiply by base once more
        if odd:
            result *= base
            logger.info(f"Multiplied by base due to odd exponent: {result}")

        # Handle negative exponent
        if negative_exponent:
            result = 1 / result
            logger.info(f"Computed reciprocal due to negative exponent: {result}")

        # Send the result
        logger.info(f"Computed result: {result}")
        task_handler.send_results(
            {task_handler.expected_results[0]: str(result).encode()}
        )
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

import logging
import os

import grpc
from armonik.common import Output, TaskDefinition
from armonik.worker import ArmoniKWorker, ClefLogger, TaskHandler

ClefLogger.setup_logging(logging.INFO)


def processor(task_handler: TaskHandler) -> Output:
    # Get inputs
    split_threshold = task_handler.task_options.options.get("split", None)
    if split_threshold is None:
        return Output("Threshold is not specified")
    else:
        split_threshold = int(split_threshold)

    payload = task_handler.payload

    if len(payload) > 0:
        # Data needs to be computed
        number_of_values = len(payload) // 4
        if number_of_values > split_threshold:
            # Above Compute threshold : Split into 2 subtasks
            pivot_byte_index = (number_of_values // 2) * 4
            left_payload, right_payload = (
                payload[:pivot_byte_index],
                payload[pivot_byte_index:],
            )
            # Create new results
            new_results = task_handler.create_results_metadata(
                ["left_result", "right_result"]
            )
            # Create new payloads
            new_payloads = task_handler.create_results(
                {
                    "left_payload": left_payload,
                    "right_payload": right_payload,
                    "aggregation_payload": b"",
                }
            )
            # Create subtask definitions
            left_task = TaskDefinition(
                new_payloads["left_payload"].result_id,
                [new_results["left_result"].result_id],
            )
            right_task = TaskDefinition(
                new_payloads["right_payload"].result_id,
                [new_results["right_result"].result_id],
            )
            aggregate = TaskDefinition(
                new_payloads["aggregation_payload"].result_id,
                task_handler.expected_results,  # The result is delegated to this aggregation task
                data_dependencies=[
                    new_results["left_result"].result_id,
                    new_results["right_result"].result_id,
                ],  # The task depends on the subtasks
            )

            # Submit tasks
            task_handler.submit_tasks([left_task, right_task, aggregate])

            # No result to be submitted

        else:
            # Compute the sum
            values = [
                int.from_bytes(payload[i : i + 4], "little")
                for i in range(0, len(payload), 4)
            ]
            result = sum(values)

            # Send the result
            task_handler.send_results(
                {task_handler.expected_results[0]: result.to_bytes(8, "little")}
            )
    else:
        # Aggregation of results
        result = sum(
            int.from_bytes(v, "little") for v in task_handler.data_dependencies.values()
        )

        # Send the result
        task_handler.send_results(
            {task_handler.expected_results[0]: result.to_bytes(8, "little")}
        )

    # Done
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

import logging
import os
import numpy as np
from numba import cuda
from armonik.worker import ArmoniKWorker, TaskHandler, ClefLogger
from armonik.common import Output
import grpc
import time
from common import NameIdDict, NumpyArraySerializer

ClefLogger.setup_logging(logging.INFO)


# Task processing
def processor(task_handler: TaskHandler) -> Output:
    """
    Processes a task by summing the two vectors and sending the result.

    Args:
        task_handler: The handler for the current task.

    Returns:
        Output: The result of the task processing.
    """
    logger = ClefLogger.getLogger("ArmoniKWorker")
    logger.info("Handling the Task")

    if not cuda.is_available():
        raise RuntimeError(
            "CUDA is not available. Please check your GPU and driver installation."
        )

    logger.info(
        "CUDA is available",
        extra={"context": {"Device name": cuda.get_current_device().name}},
    )
    payload = task_handler.payload

    task_info = NameIdDict.deserialize(payload).data

    encoded_array1 = task_handler.data_dependencies[task_info["array1"]]
    encoded_array2 = task_handler.data_dependencies[task_info["array2"]]

    # Initialize data on the host (CPU)
    a_host = NumpyArraySerializer.deserialize(encoded_array1).array
    b_host = NumpyArraySerializer.deserialize(encoded_array2).array
    N = len(a_host)

    # Transfer data to the device (GPU)
    a_device = cuda.to_device(a_host)
    b_device = cuda.to_device(b_host)
    c_device = cuda.device_array(N, dtype=np.float32)

    # Define CUDA kernel
    @cuda.jit
    def vector_add(a, b, c):
        idx = cuda.grid(1)
        if idx < c.size:
            c[idx] = a[idx] + b[idx]

    # Define number of threads and blocks
    threads_per_block = task_info.get("threads_per_block", 1024)
    blocks_per_grid = task_info.get(
        "blocks_per_grid", (N + (threads_per_block - 1)) // threads_per_block
    )

    # Measure time for GPU execution
    start = time.time()
    # Launch kernel
    vector_add[blocks_per_grid, threads_per_block](a_device, b_device, c_device)
    cuda.synchronize()  # Wait for the kernel to finish
    end = time.time()

    # Copy the result back to the host (CPU)
    c_host = c_device.copy_to_host()

    # Print and verify results
    logger.info(
        "GPU execution time in seconds", extra={"context": {"time": end - start}}
    )

    result_id = task_handler.expected_results[0]
    task_handler.send_results({result_id: NumpyArraySerializer(c_host).serialize()})

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

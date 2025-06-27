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


def gpu_processor(task_handler: TaskHandler) -> Output:
    """GPU processor for dependent tasks that use root task output."""
    logger = ClefLogger.getLogger("GPUWorker")
    logger.info("Handling GPU Task")

    if not cuda.is_available():
        raise RuntimeError("CUDA is not available. Please check your GPU and driver installation.")

    logger.info("CUDA is available", extra={"context": {"Device name": cuda.get_current_device().name}})
    
    payload = task_handler.payload
    task_info = NameIdDict.deserialize(payload).data
    
    task_id = task_info.get("task_id", "unknown")
    logger.info(f"Processing dependent GPU task: {task_id}")

    # Get input data (dependent tasks only)
    array1_id = task_info["array1"]
    array2_id = task_info["array2"]
    root_data_id = task_info["root_data"]
    
    # Deserialize input arrays
    a_host = NumpyArraySerializer.deserialize(task_handler.data_dependencies[array1_id]).array
    b_host = NumpyArraySerializer.deserialize(task_handler.data_dependencies[array2_id]).array
    root_data = NumpyArraySerializer.deserialize(task_handler.data_dependencies[root_data_id]).array
    
    N = len(a_host)
    root_mean = np.mean(root_data).astype(np.float32)

    # Transfer data to GPU
    a_device = cuda.to_device(a_host)
    b_device = cuda.to_device(b_host)
    c_device = cuda.device_array(N, dtype=np.float32)

    # Define CUDA kernel that uses root data
    @cuda.jit
    def vector_add_with_root(a, b, c, root_mean):
        idx = cuda.grid(1)
        if idx < c.size:
            c[idx] = a[idx] + b[idx] + root_mean

    # Get GPU parameters
    threads_per_block = task_info.get("threads_per_block", 1024)
    blocks_per_grid = task_info.get("blocks_per_grid", (N + (threads_per_block - 1)) // threads_per_block)

    # Execute on GPU
    start = time.time()
    vector_add_with_root[blocks_per_grid, threads_per_block](a_device, b_device, c_device, root_mean)
    cuda.synchronize()
    end = time.time()

    # Copy result back to host
    c_host = c_device.copy_to_host()

    logger.info(
        f"GPU task {task_id} used root data mean: {root_mean}, execution time: {end - start:.4f} seconds",
        extra={
            "context": {
                "array_size": N,
                "threads_per_block": threads_per_block,
                "blocks_per_grid": blocks_per_grid,
                "root_mean_used": float(root_mean)
            }
        }
    )

    # Send result
    result_id = task_handler.expected_results[0]
    task_handler.send_results({result_id: NumpyArraySerializer(c_host).serialize()})

    return Output()


def main():
    """Initialize and start the GPU worker."""
    logger = ClefLogger.getLogger("GPUWorker")

    # Define communication endpoints
    worker_scheme = "unix://" if os.getenv("ComputePlane__WorkerChannel__SocketType", "unixdomainsocket") == "unixdomainsocket" else "http://"
    agent_scheme = "unix://" if os.getenv("ComputePlane__AgentChannel__SocketType", "unixdomainsocket") == "unixdomainsocket" else "http://"
    
    worker_endpoint = worker_scheme + os.getenv("ComputePlane__WorkerChannel__Address", "/cache/armonik_worker.sock")
    agent_endpoint = agent_scheme + os.getenv("ComputePlane__AgentChannel__Address", "/cache/armonik_agent.sock")

    logger.info("Started GPU worker for dependency workflow!")
    
    with grpc.insecure_channel(agent_endpoint, options=(("grpc.default_authority", "localhost"),)) as agent_channel:
        worker = ArmoniKWorker(agent_channel, gpu_processor, logger=logger)
        logger.info("GPU Worker Connected")
        worker.start(worker_endpoint)


if __name__ == "__main__":
    main()

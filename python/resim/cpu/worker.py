import logging
import os
import numpy as np
from armonik.worker import ArmoniKWorker, TaskHandler, ClefLogger
from armonik.common import Output
import grpc
import time
from common import NameIdDict, NumpyArraySerializer

ClefLogger.setup_logging(logging.INFO)


def cpu_processor(task_handler: TaskHandler) -> Output:
    """CPU processor handling both root and dependent tasks."""
    logger = ClefLogger.getLogger("CPUWorker")
    logger.info("Handling CPU Task")
    
    payload = task_handler.payload
    task_info = NameIdDict.deserialize(payload).data
    
    task_type = task_info.get("task_type", "dependent")
    task_id = task_info.get("task_id", "unknown")
    
    logger.info(f"Processing {task_type} CPU task: {task_id}")
    
    if task_type == "root":
        # Root task: generate base data
        input_data = np.array(task_info["input_data"], dtype=np.float32)
        
        # Process root data (example: square the values)
        start = time.time()
        root_result = np.square(input_data)
        end = time.time()
        
        logger.info(f"Root task generated data with shape: {root_result.shape}")
        
        # Send result
        output_id = task_info["output"]
        task_handler.send_results({output_id: NumpyArraySerializer(root_result).serialize()})
        
    elif task_type == "dependent":
        # Dependent task: process arrays using root task output
        array1_id = task_info["array1"]
        array2_id = task_info["array2"]
        root_data_id = task_info["root_data"]
        
        # Get input data
        a_host = NumpyArraySerializer.deserialize(task_handler.data_dependencies[array1_id]).array
        b_host = NumpyArraySerializer.deserialize(task_handler.data_dependencies[array2_id]).array
        root_data = NumpyArraySerializer.deserialize(task_handler.data_dependencies[root_data_id]).array
        
        # Use root data to modify the computation (example: add mean of root data)
        start = time.time()
        root_mean = np.mean(root_data)
        c_host = a_host + b_host + root_mean
        end = time.time()
        
        logger.info(f"Dependent CPU task used root data mean: {root_mean}")
        
        # Send result
        result_id = task_handler.expected_results[0]
        task_handler.send_results({result_id: NumpyArraySerializer(c_host).serialize()})
    
    logger.info(f"CPU task {task_id} execution time: {end - start:.4f} seconds")
    return Output()


def main():
    """Initialize and start the CPU worker."""
    logger = ClefLogger.getLogger("CPUWorker")
    
    # Define communication endpoints
    worker_scheme = "unix://" if os.getenv("ComputePlane__WorkerChannel__SocketType", "unixdomainsocket") == "unixdomainsocket" else "http://"
    agent_scheme = "unix://" if os.getenv("ComputePlane__AgentChannel__SocketType", "unixdomainsocket") == "unixdomainsocket" else "http://"
    
    worker_endpoint = worker_scheme + os.getenv("ComputePlane__WorkerChannel__Address", "/cache/armonik_worker.sock")
    agent_endpoint = agent_scheme + os.getenv("ComputePlane__AgentChannel__Address", "/cache/armonik_agent.sock")
    
    logger.info("Started CPU worker for dependency workflow!")
    
    with grpc.insecure_channel(agent_endpoint, options=(("grpc.default_authority", "localhost"),)) as agent_channel:
        worker = ArmoniKWorker(agent_channel, cpu_processor, logger=logger)
        logger.info("CPU Worker Connected")
        worker.start(worker_endpoint)


if __name__ == "__main__":
    main()

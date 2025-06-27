import argparse
import logging
from typing import List, Dict
import grpc
from datetime import timedelta
import numpy as np
import asyncio
import time
from armonik.client import ArmoniKResults, ArmoniKSessions, ArmoniKTasks, ArmoniKEvents
from armonik.common import TaskDefinition, TaskOptions
from common import NameIdDict, NumpyArraySerializer

logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s")
logger = logging.getLogger(__name__)


def get_dependency_job_configurations() -> Dict[str, Dict]:
    """Define configurations for root CPU job and dependent jobs (3 CPU + 4 GPU)."""
    return {
        "root_cpu_job": {"partition": "cpu", "task_type": "root", "job_type": "cpu", "size": 50000, "seed": 100},
        "cpu_dep_001": {"partition": "cpu", "task_type": "dependent", "job_type": "cpu", "size": 10000, "seed": 42},
        "cpu_dep_002": {"partition": "cpu", "task_type": "dependent", "job_type": "cpu", "size": 15000, "seed": 43},
        "cpu_dep_003": {"partition": "cpu", "task_type": "dependent", "job_type": "cpu", "size": 20000, "seed": 44},
        "gpu_dep_001": {"partition": "gpu", "task_type": "dependent", "job_type": "gpu", "size": 1000000, "seed": 47, "threads_per_block": 1024},
        "gpu_dep_002": {"partition": "gpu", "task_type": "dependent", "job_type": "gpu", "size": 2000000, "seed": 48, "threads_per_block": 512},
        "gpu_dep_003": {"partition": "gpu", "task_type": "dependent", "job_type": "gpu", "size": 1500000, "seed": 49, "threads_per_block": 1024},
        "gpu_dep_004": {"partition": "gpu", "task_type": "dependent", "job_type": "gpu", "size": 3000000, "seed": 50, "threads_per_block": 256}
    }


def print_input_data(job_id: str, config: Dict, input_arrays: Dict = None, root_data: np.ndarray = None):
    """Print detailed input data for a job."""
    logger.info(f"📥 INPUT DATA for {job_id}: {config['job_type'].upper()} on {config['partition']}, size: {config['size']:,}")
    if config['task_type'] == 'root' and root_data is not None:
        logger.info(f"  Root data: sum={np.sum(root_data):.3f}, mean={np.mean(root_data):.3f}")
    elif input_arrays:
        array1, array2 = input_arrays.get('array1'), input_arrays.get('array2')
        if array1 is not None and array2 is not None:
            logger.info(f"  Array1: sum={np.sum(array1):.3f}, Array2: sum={np.sum(array2):.3f}")


def print_output_data(job_id: str, config: Dict, output_data: np.ndarray, execution_time: float = None):
    """Print detailed output data for a job."""
    logger.info(f"📤 OUTPUT DATA for {job_id}: shape={output_data.shape}, sum={np.sum(output_data):.3f}, mean={np.mean(output_data):.3f}")
    if execution_time:
        throughput = output_data.size / execution_time if execution_time > 0 else 0
        logger.info(f"  Execution: {execution_time:.3f}s, throughput: {throughput:,.0f} elements/s")


async def check_result_availability_async(result_client, output_id: str, session_id: str):
    """Asynchronously check if a result is available for download."""
    try:
        loop = asyncio.get_event_loop()
        await loop.run_in_executor(None, result_client.download_result_data, output_id, session_id)
        return True
    except Exception:
        return False


async def download_result_async(result_client, output_id: str, session_id: str, job_id: str, config: Dict):
    """Asynchronously download and process a single result."""
    try:
        logger.info(f"🔄 Starting async download for {job_id}...")
        start_download = time.time()
        
        loop = asyncio.get_event_loop()
        serialized_result = await loop.run_in_executor(None, result_client.download_result_data, output_id, session_id)
        final_result = NumpyArraySerializer.deserialize(serialized_result).array
        
        download_time = time.time() - start_download
        logger.info(f"⬇️ Downloaded {job_id} in {download_time:.3f} seconds")
        
        print_output_data(job_id, config, final_result)
        
        return {
            'job_id': job_id, 'config': config, 'result': final_result,
            'success': True, 'error': None, 'download_time': download_time
        }
    except Exception as e:
        logger.error(f"❌ Error downloading result for {job_id}: {e}")
        return {
            'job_id': job_id, 'config': config, 'result': None,
            'success': False, 'error': str(e), 'download_time': None
        }


async def monitor_results_async(result_client, session_id: str, all_output_ids: List[str], 
                               job_configs: Dict, dep_results: Dict, root_output_id: str):
    """Asynchronously monitor and download results as they become available."""
    logger.info("Starting asynchronous result monitoring...")
    
    # Create mapping of output_id to job info
    output_to_job = {root_output_id: {'job_id': 'root_cpu_job', 'config': job_configs['root_cpu_job']}}
    
    dependent_jobs = {k: v for k, v in job_configs.items() if v["task_type"] == "dependent"}
    for job_id, config in dependent_jobs.items():
        output_id = dep_results[f"{job_id}_output"].result_id
        output_to_job[output_id] = {'job_id': job_id, 'config': config}
    
    completed_jobs = []
    pending_outputs = set(all_output_ids)
    check_interval = 2.0
    
    logger.info(f"Monitoring {len(all_output_ids)} results asynchronously...")
    
    while pending_outputs:
        # Check which results are now available (asynchronously)
        availability_tasks = [(output_id, check_result_availability_async(result_client, output_id, session_id)) 
                             for output_id in pending_outputs]
        
        newly_available = []
        for output_id, task in availability_tasks:
            try:
                is_available = await task
                if is_available:
                    newly_available.append(output_id)
            except Exception as e:
                logger.debug(f"Availability check failed for {output_id}: {e}")
        
        # Remove newly available outputs from pending
        for output_id in newly_available:
            pending_outputs.discard(output_id)
        
        # Download newly available results asynchronously
        if newly_available:
            logger.info(f"Found {len(newly_available)} newly available results")
            
            download_tasks = []
            for output_id in newly_available:
                job_info = output_to_job[output_id]
                task = download_result_async(result_client, output_id, session_id, 
                                           job_info['job_id'], job_info['config'])
                download_tasks.append(task)
            
            # Wait for all downloads to complete
            download_results = await asyncio.gather(*download_tasks, return_exceptions=True)
            
            # Process download results
            for result in download_results:
                if isinstance(result, Exception):
                    logger.error(f"❌ Download task failed: {result}")
                else:
                    completed_jobs.append(result)
                    if result['success']:
                        logger.info(f"✅ Successfully processed {result['job_id']}")
                    else:
                        logger.error(f"❌ Failed to process {result['job_id']}: {result['error']}")
        
        # If there are still pending results, wait before next check
        if pending_outputs:
            logger.info(f"⏳ Waiting for {len(pending_outputs)} more results... (checking again in {check_interval}s)")
            await asyncio.sleep(check_interval)
    
    logger.info(f"🎉 All {len(completed_jobs)} results processed asynchronously!")
    return completed_jobs


def run_dependency_jobs_batch_async(endpoint: str) -> None:
    """Submit ALL jobs (1 root + 7 dependent) with async result monitoring."""
    partitions = ["cpu", "gpu"]
    job_configs = get_dependency_job_configurations()
    
    logger.info("🚀 Starting ArmoniK Dependency Workflow with Async Results")
    
    with grpc.insecure_channel(endpoint) as channel:
        task_client = ArmoniKTasks(channel)
        result_client = ArmoniKResults(channel)
        sessions_client = ArmoniKSessions(channel)

        # Create session
        session_id = sessions_client.create_session(
            TaskOptions(max_duration=timedelta(hours=2), max_retries=2, priority=1, partition_id="cpu"),
            partition_ids=partitions
        )
        logger.info(f"📋 Session created: {session_id}")

        # Create result metadata
        root_results = result_client.create_results_metadata(["root_output", "root_payload"], session_id)
        root_output_id, root_payload_id = root_results["root_output"].result_id, root_results["root_payload"].result_id
        
        dependent_jobs = {k: v for k, v in job_configs.items() if v["task_type"] == "dependent"}
        all_result_names = []
        for job_id in dependent_jobs.keys():
            all_result_names.extend([f"{job_id}_array1", f"{job_id}_array2", f"{job_id}_output", f"{job_id}_payload"])
        
        dep_results = result_client.create_results_metadata(all_result_names, session_id)

        # Upload root job data
        root_config = job_configs["root_cpu_job"]
        np.random.seed(root_config["seed"])
        root_data = np.random.rand(root_config["size"]).astype(np.float32)
        
        print_input_data("root_cpu_job", root_config, root_data=root_data)
        
        root_payload_data = {
            "task_id": "root_cpu_job", "task_type": "root", "job_type": "cpu",
            "output": root_output_id, "size": root_config["size"], "input_data": root_data.tolist()
        }
        
        result_client.upload_result_data(root_payload_id, session_id, NameIdDict(root_payload_data).serialize())
        
        # Upload dependent job data
        all_tasks = []
        all_output_ids = [root_output_id]
        
        # Root task
        all_tasks.append(TaskDefinition(
            data_dependencies=[], expected_output_ids=[root_output_id], payload_id=root_payload_id,
            options=TaskOptions(max_duration=timedelta(hours=1), max_retries=2, priority=1, partition_id="cpu")
        ))
        
        # Dependent tasks
        for job_id, config in dependent_jobs.items():
            array1_id = dep_results[f"{job_id}_array1"].result_id
            array2_id = dep_results[f"{job_id}_array2"].result_id
            output_id = dep_results[f"{job_id}_output"].result_id
            payload_id = dep_results[f"{job_id}_payload"].result_id
            
            all_output_ids.append(output_id)
            
            # Generate and upload input arrays
            np.random.seed(config["seed"])
            a_host = np.random.rand(config["size"]).astype(np.float32)
            b_host = np.random.rand(config["size"]).astype(np.float32)
            
            print_input_data(job_id, config, {"array1": a_host, "array2": b_host})
            
            result_client.upload_result_data(array1_id, session_id, NumpyArraySerializer(a_host).serialize())
            result_client.upload_result_data(array2_id, session_id, NumpyArraySerializer(b_host).serialize())
            
            # Create payload
            dep_payload_data = {
                "task_id": job_id, "task_type": "dependent", "job_type": config["job_type"],
                "array1": array1_id, "array2": array2_id, "root_data": root_output_id, "size": config["size"]
            }
            
            if config["job_type"] == "gpu":
                dep_payload_data["threads_per_block"] = config["threads_per_block"]
                dep_payload_data["blocks_per_grid"] = (config["size"] + config["threads_per_block"] - 1) // config["threads_per_block"]
            
            result_client.upload_result_data(payload_id, session_id, NameIdDict(dep_payload_data).serialize())
            
            # Create task
            all_tasks.append(TaskDefinition(
                data_dependencies=[array1_id, array2_id, root_output_id],
                expected_output_ids=[output_id], payload_id=payload_id,
                options=TaskOptions(
                    max_duration=timedelta(hours=2 if config["job_type"] == "gpu" else 1),
                    max_retries=2, priority=2 if config["job_type"] == "gpu" else 1,
                    partition_id=config["partition"]
                )
            ))

        # Submit all tasks
        logger.info(f"🚀 Submitting {len(all_tasks)} tasks...")
        submission_start = time.time()
        task_client.submit_tasks(session_id, all_tasks)
        submission_time = time.time() - submission_start
        
        logger.info(f"✅ All tasks submitted in {submission_time:.3f} seconds!")

        # Start asynchronous result monitoring
        logger.info("🔍 Starting asynchronous result monitoring...")
        monitoring_start = time.time()
        
        # Run async monitoring
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        try:
            completed_results = loop.run_until_complete(
                monitor_results_async(result_client, session_id, all_output_ids, job_configs, dep_results, root_output_id)
            )
        finally:
            loop.close()
        
        total_time = time.time() - monitoring_start

        # Final summary
        successful_jobs = [r for r in completed_results if r['success']]
        failed_jobs = [r for r in completed_results if not r['success']]
        
        logger.info("🎉 EXECUTION SUMMARY")
        logger.info(f"  ✅ Successful: {len(successful_jobs)}/{len(completed_results)}")
        logger.info(f"  ❌ Failed: {len(failed_jobs)}")
        logger.info(f"  ⏱️ Total time: {total_time:.3f}s, submission: {submission_time:.3f}s")
        
        if successful_jobs:
            for job in successful_jobs:
                result_size = job['result'].size if job['result'] is not None else 0
                download_time = job.get('download_time', 0)
                logger.info(f"    - {job['job_id']}: {result_size:,} elements, {download_time:.3f}s")

    logger.info("🎉 Async workflow completed!")


def main() -> None:
    parser = argparse.ArgumentParser(description="ArmoniK Dependency Workflow with Async Results")
    parser.add_argument("--endpoint", type=str, default="127.0.0.1:5001", help="ArmoniK endpoint")
    args = parser.parse_args()
    run_dependency_jobs_batch_async(args.endpoint)


if __name__ == "__main__":
    main()

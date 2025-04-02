import argparse
import logging
from datetime import timedelta
from typing import List
import sys
import grpc
import grpc.aio
import json
from io import BytesIO
from armonik.client import ArmoniKTasks, ArmoniKResults, ArmoniKSessions, ArmoniKEvents
from armonik.common import TaskOptions, TaskDefinition
from pathlib import Path

logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger(__name__)

# common_path = Path(__file__).resolve().parent.parent / "common"
# sys.path.append(str(common_path))

# from common import NameIdDict

def run(endpoint: str, partition: str , size: int, tile: int) -> None:

    with grpc.insecure_channel(endpoint) as channel:
        # Initialize ArmoniK's side
        task_client = ArmoniKTasks(channel)

        result_client = ArmoniKResults(channel)

        session_client = ArmoniKSessions(channel)

        events_client = ArmoniKEvents(channel)

        N = (size//tile)+1 if size%tile else size//tile

        default_task_options = TaskOptions(
            max_duration=timedelta(seconds=300),
            priority=1,
            max_retries=5,
            partition_id=partition,
            options={"size": str(size), "tile": str(tile)}
        )

        session_id = session_client.create_session(
            default_task_options=default_task_options,
            partition_ids=[partition]
        )
        print(f"Session {session_id} has been created")
        
        if size > 1000 or size <= tile:
            raise Exception("Error size")

        # Compute the number of subtask needed to fill the array
        N = (size//tile)+1 if size%tile else size//tile

        # Assign a name for each task, including the init and final
        result_tasks_names = ["t" + str(i) for i in range(N+2)]
        payload_tasks_names = ["p" + str(i) for i in range (N+2)]

        result_names = ["tasks_dict"] + result_tasks_names + payload_tasks_names

        results = result_client.create_results_metadata(
            result_names, session_id=session_id
        )
        
        # Create a list of tasks, where a task is [[name, id], [payload_name, payload_id]] 
        # and a dict for filling the list during last task
        tasks_list = []
        tasks_dict = {}
        for i in range(N+2):
            task_name = result_tasks_names[i]
            payload_name = payload_tasks_names[i]
            tasks_list.append([[task_name, results[task_name].result_id], [payload_name, results[payload_name].result_id]])

        for i in range(1,N+1):
            tasks_dict[tasks_list[i][0][0]] = tasks_list[i][0][1]

        tasks_dict_encoded = json.dumps(tasks_dict).encode("utf-8")

        # Upload payloads and fill the tasks definition list
        tasks_def = []
        for i in range(N+2):
            data_dep = []
            if i == 0:
                result_data = b""
            elif i == N+1:
                last_dep = tasks_list[1:-1]
                for task in last_dep:
                    data_dep.append(task[0][1])
                result_data = tasks_dict_encoded
            else:
                if i == N and size%tile:
                    result_data = (tasks_list[i][0][0] + '-').encode("utf-8")
                else:
                    result_data =tasks_list[i][0][0].encode("utf-8")
                data_dep = [tasks_list[0][0][1]]
            
            result_client.upload_result_data(
                result_id=tasks_list[i][1][1],
                session_id=session_id,
                result_data=result_data
            )

            tasks_def.append(TaskDefinition(
                data_dependencies=data_dep,
                expected_output_ids=[tasks_list[i][0][1]],
                payload_id=tasks_list[i][1][1],
            ))
        logger.info("Payloads uploaded")

        task_client.submit_tasks(
            session_id=session_id,
            tasks=tasks_def,
        )
        logger.info("Tasks submitted")

        try:
            events_client.wait_for_result_availability(
                result_ids=tasks_list[N+1][0][1], session_id=session_id
            )
        except Exception as e:
            logger.error(f"An error occurred: {e}")

        try:
            serialized_result = result_client.download_result_data(
                tasks_list[N+1][0][1], session_id
            )
            final_result = json.loads(serialized_result.decode("utf-8"))
            logger.info(f"resultId: {tasks_list[N+1][0][1]}, data: {final_result}")
        except Exception as e:
            logger.error(f"An error occurred: {e.details()}")

        # session_client.close_session(session_id)

        # session_client.purge_session(session_id)
        # session_client.delete_session(session_id)

    logger.info("End Connection!")

def main() -> None:
    """
    Parses command-line arguments and runs the Hello World demo for ArmoniK.

    Args:
        args: Command-line arguments.

    Example:
        python client.py --endpoint 172.24.55.197:5001 --partition default
    """
    parser = argparse.ArgumentParser(
        description="Simple tasks, the first one randomly choose a seed and the next one produce a output sting depending on the result of the last task\n"
    )
    connection_args = parser.add_argument_group(
        title="Connection", 
        description="Connection arguments"
    )
    connection_args.add_argument(
        "--endpoint",
        type=str,
        default="172.24.55.197:5001",
        help="Endpoint for the connection to ArmoniK control plane.",
    )
    armonik_args = parser.add_argument_group(
        title="ArmoniK", 
        description="ArmoniK arguments"
    )
    armonik_args.add_argument(
        "--partition",
        type=str,
        default="default",
        help="Name of the partition to which submit tasks.",
    )
    parser.add_argument(
        "-s",
        "--size",
        help="Define the size of the array",
        type=int,
        default=3,
    )
    parser.add_argument(
        "-t",
        "--tile",
        help="Define the size of the tiles",
        type=int,
        default=1,
    )
    parsed_args = parser.parse_args()
    run(parsed_args.endpoint, parsed_args.partition, parsed_args.size, parsed_args.tile)

if __name__ == "__main__":
    main()
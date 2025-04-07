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

def run(endpoint: str, partition: str , list: int, segment: int) -> None:

    with grpc.insecure_channel(endpoint) as channel:
        # Initialize ArmoniK's side
        task_client = ArmoniKTasks(channel)

        result_client = ArmoniKResults(channel)

        session_client = ArmoniKSessions(channel)

        events_client = ArmoniKEvents(channel)

        default_task_options = TaskOptions(
            max_duration=timedelta(seconds=300),
            priority=1,
            max_retries=5,
            partition_id=partition,
            options={"list": str(list), "segment": str(segment)}
        )

        session_id = session_client.create_session(
            default_task_options=default_task_options,
            partition_ids=[partition]
        )
        print(f"Session {session_id} has been created")
        
        if list > 1000 or list < segment:
            raise Exception("Error size")

        # Compute the number of subtask needed to fill the final list
        N = (list//segment)+1 if list%segment else list//segment

        # Assign a name for each task, including the init and final
        result_tasks_names = ["t" + str(i) for i in range(N+2)]
        payload_tasks_names = ["p" + str(i) for i in range (N+2)]

        result_names = ["subtask_dict"] + result_tasks_names + payload_tasks_names

        results = result_client.create_results_metadata(
            result_names, session_id=session_id
        )
        
        # Create a list of dictionnaries and a dict for filling the list during last task
        tasks_list = []
        subtask_dict = {}
        for i in range(N+2):
            tasks_list.append({
                "result name" : result_tasks_names[i], 
                "result id" : results[result_tasks_names[i]].result_id, 
                "payload name" : payload_tasks_names[i], 
                "payload id" : results[payload_tasks_names[i]].result_id})

        for i in range(1,N+1):
            subtask_dict[tasks_list[i]["result name"]] = tasks_list[i]["result id"]

        subtask_dict_encoded = json.dumps(subtask_dict).encode("utf-8")

        # Upload payloads and fill the tasks definition list
        tasks_def = []
        for i in range(N+2):
            data_dep = []
            if i == 0:
                result_data = b""
            elif i == N+1:
                last_dep = tasks_list[1:-1]
                for task in last_dep:
                    data_dep.append(task["result id"])
                result_data = subtask_dict_encoded
            else:
                # Adding a mark to catch if the last one need a different length
                if i == N and list%segment:
                    result_data = (tasks_list[i]["result name"] + '-').encode("utf-8")
                else:
                    result_data =tasks_list[i]["result name"].encode("utf-8")
                data_dep = [tasks_list[0]["result id"]]
            
            result_client.upload_result_data(
                result_id=tasks_list[i]["payload id"],
                session_id=session_id,
                result_data=result_data
            )

            tasks_def.append(TaskDefinition(
                data_dependencies=data_dep,
                expected_output_ids=[tasks_list[i]["result id"]],
                payload_id=tasks_list[i]["payload id"],
            ))
        logger.info("Payloads uploaded")

        task_client.submit_tasks(
            session_id=session_id,
            tasks=tasks_def,
        )
        logger.info("Tasks submitted")

        try:
            events_client.wait_for_result_availability(
                result_ids=tasks_list[N+1]["result id"], session_id=session_id
            )
        except Exception as e:
            logger.error(f"An error occurred: {e}")

        try:
            serialized_result = result_client.download_result_data(
                tasks_list[N+1]["result id"], session_id
            )
            final_result = json.loads(serialized_result.decode("utf-8"))
            logger.info(f"resultId: {tasks_list[N+1]["result id"]}, data: {final_result}")
        except Exception as e:
            logger.error(f"An error occurred: {e.details()}")

        session_client.close_session(session_id)

    logger.info("End Connection!")

def main() -> None:
    """
    Parses command-line arguments and runs the Hello World demo for ArmoniK.

    Args:
        args: Command-line arguments.

    Example:
        python client.py --endpoint <ip>:<port> --partition default
    """
    parser = argparse.ArgumentParser(
        description="Simple tasks to build a list made up of segments created asynchronously."
    )
    connection_args = parser.add_argument_group(
        title="Connection", 
        description="Connection arguments"
    )
    connection_args.add_argument(
        "--endpoint",
        type=str,
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
        "-l",
        "--list",
        help="Define the length of the list",
        type=int,
        default=3,
    )
    parser.add_argument(
        "-s",
        "--segment",
        help="Define the length of the segment",
        type=int,
        default=1,
    )
    parsed_args = parser.parse_args()
    run(parsed_args.endpoint, parsed_args.partition, parsed_args.list, parsed_args.segment)

if __name__ == "__main__":
    main()
import logging
import sys
import random
import json
from armonik.worker import TaskHandler, ClefLogger
# This import should be fixed in future versions of the API
from armonik.worker.worker import armonik_worker
from armonik.common import Output
from pathlib import Path

ClefLogger.setup_logging(logging.INFO)
logger = ClefLogger.getLogger("ArmoniKWorker")

@armonik_worker()
def processor(task_handler: TaskHandler) -> Output:
    logger = ClefLogger.getLogger("ArmoniKWorker")
    logger.info("Handeling the Task")
    payload = task_handler.payload
    list = task_handler.task_options.options.get("list", None)
    segment = task_handler.task_options.options.get("segment", None)

    if list is None or segment is None:
            raise Exception("List or segment length are not specified")
    else:
        list = int(list)
        segment = int(segment)

    if (len(payload) == 0):
        # Init task
        taks_init_id = task_handler.expected_results.pop(0)

        init = b""

        task_handler.send_results({taks_init_id: init})

    elif (payload.decode("utf-8"))[0] == "t":
        # Sublist tasks
        string = payload.decode("utf-8")

        if string[-1] == "-":
            N = list%segment
            string = string[:-1]
        else:
            N = segment
        num = string[1:]
        sublist = []

        for i in range(N):
            sublist.append(num)

        sublist_id = task_handler.expected_results.pop(0)
        sublist_encoded = json.dumps(sublist).encode("utf-8")
        task_handler.send_results({sublist_id: sublist_encoded})

    elif(len(payload) > 3):
        # Output task
        tasks_dict = json.loads(payload.decode("utf-8"))
        output_list = []
        for i in range(1, len(tasks_dict) + 1):
            sublist = task_handler.data_dependencies[tasks_dict["t" + str(i)]]
            sublist = json.loads(sublist.decode("utf-8"))

            for num in sublist:
                output_list.append(int(num))

        output_id = task_handler.expected_results.pop(0)
        task_handler.send_results({output_id: json.dumps(output_list).encode("utf-8")})
    return Output()

if __name__ == "__main__":
    processor.run()
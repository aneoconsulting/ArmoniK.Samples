import logging
import sys

from armonik.worker import TaskHandler, ClefLogger
# This import should be fixed in future versions of the API
from armonik.worker.worker import armonik_worker
from armonik.common import Output
from pathlib import Path

# Add the common directory to the system path
common_path = Path(__file__).resolve().parent.parent / "common"
sys.path.append(str(common_path))

from common import NameIdDict


# Create Seq compatible logger
ClefLogger.setup_logging(logging.INFO)
logger = ClefLogger.getLogger("ArmoniKWorker")


@armonik_worker()
def processor(task_handler: TaskHandler) -> Output:
    """
    Processes a task by appending 'world!' to the input string and sending the result.

    Args:
        task_handler: The handler for the current task.

    Returns:
        Output: The result of the task processing.
    """
    logger = ClefLogger.getLogger("ArmoniKWorker")
    logger.info("Handeling the Task")
    payload = task_handler.payload

    name_id_mapping = NameIdDict.deserialize(payload).data

    encoded_data = task_handler.data_dependencies[name_id_mapping["input"]]

    # We convert the binary data from the handler back to the string sent by the client
    input = encoded_data.decode()
    output = input + " world!"

    result_id = task_handler.expected_results.pop(0)

    task_handler.send_results({result_id: output.encode()})

    return Output()


if __name__ == "__main__":
    processor.run()

"""ArmoniK worker entry point: ``python -m quantlib_worker``."""

from __future__ import annotations

import json
import logging
import os
from urllib.parse import urlparse

import QuantLib as ql
from armonik.common import Output
from armonik.worker import ClefLogger, TaskHandler
from armonik.worker.worker import ArmoniKWorkerWrapper

from .batch import InvalidPayload, process_payload


def _endpoints(channel: str, default_socket: str) -> tuple[str, str]:
    """Return (server bind address, client URI) for a ComputePlane channel.

    ArmoniK.Core passes a bare socket path in unixdomainsocket mode and a full URI
    (``http://host:port``) in tcp mode; the SDK wrapper would prefix the latter twice.
    """
    socket_type = os.getenv(f"ComputePlane__{channel}__SocketType", "unixdomainsocket").lower()
    address = os.getenv(f"ComputePlane__{channel}__Address", default_socket)
    if socket_type == "unixdomainsocket":
        path = urlparse(address).path if "://" in address else address
        return f"unix://{path}", f"unix://{path}"
    uri = address if "://" in address else f"http://{address}"
    return f"0.0.0.0:{urlparse(uri).port}", uri


CONVENTION_VERSION = "v1"
INPUT_NAME = "requests"
OUTPUT_NAME = "results"


def _convention_io(task_handler: TaskHandler) -> tuple[bytes, str]:
    """Return (pricing payload, output id) of a task following the ArmoniK SDK convention v1.

    The task options carry ``ConventionVersion=v1`` and the task payload maps names to blob ids,
    ``{"inputs": {"requests": <id>}, "outputs": {"results": <id>}}``: the pricing payload is the
    ``requests`` data dependency and the results go to the ``results`` expected output.
    The C++ SDK does not name outputs (``"outputs": {}``): the single expected output is then used.
    """
    version = task_handler.task_options.options.get("ConventionVersion")
    if version != CONVENTION_VERSION:
        raise InvalidPayload(f"unsupported ConventionVersion {version!r}, expected {CONVENTION_VERSION!r}")
    try:
        refs = json.loads(task_handler.payload)
        input_id = refs["inputs"][INPUT_NAME]
        if not refs["outputs"] and len(task_handler.expected_results) == 1:
            output_id = task_handler.expected_results[0]
        else:
            output_id = refs["outputs"][OUTPUT_NAME]
    except (ValueError, TypeError, KeyError) as e:
        raise InvalidPayload(
            f'expected {{"inputs": {{"{INPUT_NAME}": <blob id>}}, "outputs": {{"{OUTPUT_NAME}": <blob id>}}}}: {e!r}'
        ) from e
    if input_id not in task_handler.data_dependencies:
        raise InvalidPayload(f"input '{INPUT_NAME}' ({input_id}) is not a data dependency of the task")
    if output_id not in task_handler.expected_results:
        raise InvalidPayload(f"output '{OUTPUT_NAME}' ({output_id}) is not an expected output of the task")
    return task_handler.data_dependencies[input_id], output_id


def process(task_handler: TaskHandler) -> Output:
    logger = logging.getLogger("QuantLibWorker")
    try:
        payload, output_id = _convention_io(task_handler)
        result = process_payload(payload)
    except InvalidPayload as e:
        logger.warning(f"Task {task_handler.task_id} has an invalid payload: {e}")
        return Output(f"invalid payload: {e}")
    task_handler.send_results({output_id: result})
    return Output()


def main() -> None:
    ClefLogger.setup_logging(getattr(logging, os.getenv("LOG_LEVEL", "INFO").upper(), logging.INFO))
    logger = ClefLogger.getLogger("QuantLibWorker")
    worker_bind, _ = _endpoints("WorkerChannel", "/cache/armonik_worker.sock")
    _, agent_uri = _endpoints("AgentChannel", "/cache/armonik_agent.sock")
    logger.info(f"QuantLib {ql.__version__} worker listening on {worker_bind}, agent at {agent_uri}")
    ArmoniKWorkerWrapper(
        processor=process, logger=logger, worker_endpoint=worker_bind, agent_endpoint=agent_uri
    ).run()


if __name__ == "__main__":
    main()

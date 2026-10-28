"""ArmoniK worker entry point: ``python -m quantlib_worker``."""

from __future__ import annotations

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


def process(task_handler: TaskHandler) -> Output:
    logger = logging.getLogger("QuantLibWorker")
    if len(task_handler.expected_results) != 1:
        return Output(f"expected exactly one output, got {len(task_handler.expected_results)}")
    try:
        result = process_payload(task_handler.payload)
    except InvalidPayload as e:
        logger.warning(f"Task {task_handler.task_id} has an invalid payload: {e}")
        return Output(f"invalid payload: {e}")
    task_handler.send_results({task_handler.expected_results[0]: result})
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

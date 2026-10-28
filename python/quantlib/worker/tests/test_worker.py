import json

import pytest

from quantlib_worker.__main__ import _endpoints, process


@pytest.mark.parametrize(
    "socket_type,address,expected",
    [
        (None, None, ("unix:///cache/armonik_worker.sock", "unix:///cache/armonik_worker.sock")),
        ("unixdomainsocket", "/c/w.sock", ("unix:///c/w.sock", "unix:///c/w.sock")),
        ("tcp", "http://worker0:10667", ("0.0.0.0:10667", "http://worker0:10667")),
        ("tcp", "worker0:10667", ("0.0.0.0:10667", "http://worker0:10667")),
    ],
)
def test_endpoints(monkeypatch, socket_type, address, expected):
    for name, value in (("SocketType", socket_type), ("Address", address)):
        if value is None:
            monkeypatch.delenv(f"ComputePlane__WorkerChannel__{name}", raising=False)
        else:
            monkeypatch.setenv(f"ComputePlane__WorkerChannel__{name}", value)
    assert _endpoints("WorkerChannel", "/cache/armonik_worker.sock") == expected


class FakeTaskHandler:
    def __init__(self, payload, expected_results=("out",)):
        self.payload = payload
        self.task_id = "task"
        self.expected_results = list(expected_results)
        self.sent = {}

    def send_results(self, results):
        self.sent.update(results)


def test_process_sends_result():
    payload = {
        "version": 1,
        "evaluationDate": "2026-10-22",
        "requests": [{"id": "x", "instrument": {"type": "Nope"}}],
    }
    handler = FakeTaskHandler(json.dumps(payload).encode())
    assert process(handler).success
    assert json.loads(handler.sent["out"])["results"][0]["id"] == "x"


def test_process_invalid_payload_is_task_error():
    handler = FakeTaskHandler(b"oops")
    output = process(handler)
    assert not output.success and output.error.startswith("invalid payload:")
    assert handler.sent == {}


def test_process_requires_single_output():
    assert not process(FakeTaskHandler(b"{}", expected_results=())).success

import json
from types import SimpleNamespace

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


REFS = {"inputs": {"requests": "in"}, "outputs": {"results": "out"}}


class FakeTaskHandler:
    def __init__(self, data, refs=REFS, options=None, expected_results=("out",)):
        self.payload = json.dumps(refs).encode() if isinstance(refs, dict) else refs
        self.data_dependencies = {"in": data}
        self.task_options = SimpleNamespace(options={"ConventionVersion": "v1"} if options is None else options)
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


def test_process_unnamed_single_output():
    payload = {"version": 1, "evaluationDate": "2026-10-22", "requests": [{"id": "x", "instrument": {"type": "Nope"}}]}
    handler = FakeTaskHandler(json.dumps(payload).encode(), refs={"inputs": {"requests": "in"}, "outputs": {}})
    assert process(handler).success
    assert json.loads(handler.sent["out"])["results"][0]["id"] == "x"


@pytest.mark.parametrize(
    "kwargs,message",
    [
        ({}, "invalid payload: "),
        ({"options": {}}, "unsupported ConventionVersion None"),
        ({"options": {"ConventionVersion": "v2"}}, "unsupported ConventionVersion 'v2'"),
        ({"refs": b"oops"}, "expected {"),
        ({"refs": {"inputs": {"other": "in"}, "outputs": {"results": "out"}}}, "expected {"),
        ({"refs": {"inputs": {"requests": "missing"}, "outputs": {"results": "out"}}}, "is not a data dependency"),
        ({"refs": {"inputs": {"requests": "in"}, "outputs": {}}, "expected_results": ("a", "b")}, "expected {"),
        ({"expected_results": ()}, "is not an expected output"),
    ],
)
def test_process_invalid_task_is_task_error(kwargs, message):
    handler = FakeTaskHandler(b"oops", **kwargs)
    output = process(handler)
    assert not output.success and output.error.startswith("invalid payload:") and message in output.error
    assert handler.sent == {}

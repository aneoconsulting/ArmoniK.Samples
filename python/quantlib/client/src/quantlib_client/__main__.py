"""Submit QuantLib pricing requests to ArmoniK: ``python -m quantlib_client requests.json``.

The input file is either a full payload (``{"evaluationDate": ..., "requests": [...]}``), a list of
requests, or a single request. Requests are split into tasks of ``--batch-size`` requests each; the
results are reassembled in input order.
"""

from __future__ import annotations

import argparse
import datetime as dt
import json
import logging
import os
import sys
import time
from typing import Any, Dict, List

from armonik.client import ArmoniKEvents, ArmoniKResults, ArmoniKSessions, ArmoniKTasks
from armonik.common import ResultStatus, TaskDefinition, TaskOptions
from armonik.common.channel import create_channel

logger = logging.getLogger("quantlib_client")


def parse_args(argv: List[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("input", help="JSON file with the pricing requests, '-' for stdin")
    parser.add_argument(
        "--endpoint",
        default=os.getenv("GrpcClient__Endpoint", "http://localhost:5001"),
        help="ArmoniK control plane endpoint (default: $GrpcClient__Endpoint or %(default)s)",
    )
    parser.add_argument("--partition", default=os.getenv("PARTITION", ""), help="partition id (default: default one)")
    parser.add_argument("--batch-size", type=int, default=10, help="pricing requests per task (default: %(default)s)")
    parser.add_argument("--repeat", type=int, default=1, help="submit the requests N times, for load tests")
    parser.add_argument("--evaluation-date", type=dt.date.fromisoformat, help="when the input has none (default: today)")
    parser.add_argument("--max-retries", type=int, default=2, help="task retries on worker failure (default: %(default)s)")
    parser.add_argument("--max-duration", type=float, default=300, help="task timeout in seconds (default: %(default)s)")
    parser.add_argument("--output", "-o", help="write the results JSON there instead of stdout")
    parser.add_argument("--keep-session", action="store_true", help="do not close the session at the end")
    args = parser.parse_args(argv)
    if args.batch_size < 1 or args.repeat < 1:
        parser.error("--batch-size and --repeat must be >= 1")
    return args


def load_requests(args: argparse.Namespace) -> tuple[str, List[Any]]:
    with sys.stdin if args.input == "-" else open(args.input, encoding="utf-8") as f:
        document = json.load(f)
    if isinstance(document, dict) and "requests" in document:
        evaluation_date = args.evaluation_date or document.get("evaluationDate")
        requests = document["requests"]
    else:
        evaluation_date = args.evaluation_date
        requests = document if isinstance(document, list) else [document]
    evaluation_date = str(evaluation_date or dt.date.today().isoformat())

    if args.repeat > 1:
        requests = [
            {**r, "id": f"{r.get('id')}#{k}"} if isinstance(r, dict) else r
            for k in range(args.repeat)
            for r in requests
        ]
    return evaluation_date, requests


def wait_for_outputs(events: ArmoniKEvents, results: ArmoniKResults, session_id: str, output_ids: List[str]) -> None:
    """Wait until every output is completed or aborted (the SDK stops at the first aborted one)."""
    pending = set(output_ids)
    while pending:
        try:
            events.wait_for_result_availability(list(pending), session_id, parallelism=4)
            return
        except RuntimeError:
            pending = {r for r in pending if results.get_result(r).status not in (ResultStatus.COMPLETED, ResultStatus.ABORTED)}


def main(argv: List[str] | None = None) -> int:
    logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s", stream=sys.stderr)
    args = parse_args(argv)
    evaluation_date, requests = load_requests(args)
    batches = [requests[i : i + args.batch_size] for i in range(0, len(requests), args.batch_size)]

    start = time.perf_counter()
    with create_channel(args.endpoint) as channel:
        sessions, tasks = ArmoniKSessions(channel), ArmoniKTasks(channel)
        results, events = ArmoniKResults(channel), ArmoniKEvents(channel)

        options = TaskOptions(
            max_duration=dt.timedelta(seconds=args.max_duration),
            priority=1,
            max_retries=args.max_retries,
            partition_id=args.partition or None,
        )
        session_id = sessions.create_session(options, partition_ids=[args.partition] if args.partition else None)
        logger.info(f"Session {session_id}: {len(requests)} requests in {len(batches)} tasks")

        payloads = {
            f"payload-{i}": json.dumps({"version": 1, "evaluationDate": evaluation_date, "requests": b}).encode()
            for i, b in enumerate(batches)
        }
        payload_ids = {name: r.result_id for name, r in results.create_results(payloads, session_id, batch_size=100).items()}
        output_names = [f"output-{i}" for i in range(len(batches))]
        output_ids = [results.create_results_metadata(output_names, session_id)[n].result_id for n in output_names]

        submitted = tasks.submit_tasks(
            session_id,
            [
                TaskDefinition(payload_id=payload_ids[f"payload-{i}"], expected_output_ids=[output_ids[i]])
                for i in range(len(batches))
            ],
        )
        logger.info(f"Submitted {len(submitted)} tasks in {time.perf_counter() - start:.2f}s")

        wait_for_outputs(events, results, session_id, output_ids)

        merged: List[Dict[str, Any]] = []
        failed_tasks: List[Dict[str, Any]] = []
        quantlib_version = None
        for batch, output_id in zip(batches, output_ids):
            result = results.get_result(output_id)
            if result.status == ResultStatus.COMPLETED:
                document = json.loads(results.download_result_data(output_id, session_id))
                quantlib_version = document.get("quantlibVersion")
                merged.extend(document["results"])
            else:
                task = tasks.get_task(result.owner_task_id)
                error = (task.output.error if task.output else None) or task.status_message or "task failed"
                failed_tasks.append({"taskId": task.id, "error": error, "requestIds": [r.get("id") for r in batch if isinstance(r, dict)]})
                merged.extend({"id": r.get("id") if isinstance(r, dict) else None, "error": f"task failed: {error}"} for r in batch)

        if not args.keep_session:
            sessions.close_session(session_id)

    wall = time.perf_counter() - start
    errors = sum(1 for r in merged if "error" in r)
    output = {
        "version": 1,
        "sessionId": session_id,
        "evaluationDate": evaluation_date,
        "quantlibVersion": quantlib_version,
        "results": merged,
        "failedTasks": failed_tasks,
    }
    text = json.dumps(output, indent=2)
    if args.output:
        with open(args.output, "w", encoding="utf-8") as f:
            f.write(text + "\n")
    else:
        print(text)

    pricing_ms = sum(r.get("elapsedMs", 0) for r in merged)
    logger.info(
        f"{len(merged)} results, {errors} errors, {len(failed_tasks)} failed tasks in {wall:.2f}s "
        f"({len(merged) / wall:.1f} req/s, pricing time {pricing_ms / 1000:.2f}s)"
    )
    return 1 if failed_tasks or errors else 0


if __name__ == "__main__":
    sys.exit(main())

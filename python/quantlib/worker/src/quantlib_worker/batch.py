"""Payload (JSON bytes) -> result (JSON bytes), independent from ArmoniK."""

from __future__ import annotations

import json
import math
import time
from typing import Any, Dict, List

import QuantLib as ql
from pydantic import ValidationError

from .conventions import to_ql_date
from .pricers import price
from .schema import SCHEMA_VERSION, PricingBatch, parse_request


class InvalidPayload(ValueError):
    """The payload as a whole cannot be processed: the task must fail."""


def _format_validation_error(error: ValidationError) -> str:
    return "; ".join(
        f"{'.'.join(str(p) for p in e['loc']) or '<root>'}: {e['msg']}" for e in error.errors(include_url=False)
    )


def _finite(value: Any) -> Any:
    """Replace NaN/inf (not representable in JSON) by null."""
    if isinstance(value, float) and not math.isfinite(value):
        return None
    if isinstance(value, dict):
        return {k: _finite(v) for k, v in value.items()}
    if isinstance(value, list):
        return [_finite(v) for v in value]
    return value


def _price_one(raw: Any, batch: PricingBatch) -> Dict[str, Any]:
    request_id = raw.get("id") if isinstance(raw, dict) else None
    start = time.perf_counter()
    try:
        if not isinstance(raw, dict):
            raise ValueError("each request must be a JSON object")
        req = parse_request(raw)
        evaluation = to_ql_date(req.evaluation_date or batch.evaluation_date)
        ql.Settings.instance().evaluationDate = evaluation
        out: Dict[str, Any] = {"id": req.id, "type": req.instrument.type, **price(req, evaluation)}
    except ValidationError as e:
        out = {"id": request_id, "error": f"invalid request: {_format_validation_error(e)}"}
    except (ValueError, RuntimeError) as e:  # QuantLib errors surface as RuntimeError
        out = {"id": request_id, "error": str(e)}
    except Exception as e:  # noqa: BLE001 - one request must never fail the whole batch
        out = {"id": request_id, "error": f"{type(e).__name__}: {e}"}
    out["elapsedMs"] = (time.perf_counter() - start) * 1000
    return out


def process_payload(payload: bytes) -> bytes:
    try:
        batch = PricingBatch.model_validate_json(payload)
    except ValidationError as e:
        raise InvalidPayload(_format_validation_error(e)) from e

    results: List[Dict[str, Any]] = [_price_one(raw, batch) for raw in batch.requests]
    document = {
        "version": SCHEMA_VERSION,
        "evaluationDate": batch.evaluation_date.isoformat(),
        "quantlibVersion": ql.__version__,
        "results": results,
    }
    return json.dumps(_finite(document), allow_nan=False).encode()

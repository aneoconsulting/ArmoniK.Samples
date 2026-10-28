from __future__ import annotations

from typing import Any, Dict

import QuantLib as ql

from ..schema import BondRequest, OptionRequest, PricingRequest, SwapRequest
from .bond import price_bond
from .options import price_option
from .swap import price_swap


def price(req: PricingRequest, evaluation: ql.Date) -> Dict[str, Any]:
    """Price one request. ``evaluation`` must already be set as QuantLib's global evaluation date."""
    if isinstance(req, OptionRequest):
        return price_option(req, evaluation)
    if isinstance(req, BondRequest):
        return price_bond(req, evaluation)
    if isinstance(req, SwapRequest):
        return price_swap(req, evaluation)
    raise TypeError(f"unsupported request {type(req).__name__}")


__all__ = ["price"]

"""Fixed-for-floating vanilla interest rate swaps."""

from __future__ import annotations

from typing import Any, Dict

import QuantLib as ql

from ..conventions import BUSINESS_DAY_CONVENTIONS, FREQUENCIES, calendar, day_counter, to_iso, to_ql_date
from ..market import build_curve
from ..schema import SwapRequest


def price_swap(req: SwapRequest, evaluation: ql.Date) -> Dict[str, Any]:
    instrument, market = req.instrument, req.market
    cal = calendar(instrument.calendar)
    convention = BUSINESS_DAY_CONVENTIONS[instrument.business_day_convention]

    discount = ql.YieldTermStructureHandle(build_curve(market.discount_curve, evaluation))
    forecast = (
        ql.YieldTermStructureHandle(build_curve(market.forecast_curve, evaluation))
        if market.forecast_curve is not None
        else discount
    )
    index = ql.Euribor(ql.Period(instrument.float_index.tenor), forecast)

    if instrument.start_date is not None:
        start = to_ql_date(instrument.start_date)
    else:
        start = cal.advance(cal.adjust(evaluation), instrument.settlement_days, ql.Days)
    if index.fixingDate(start) < evaluation:
        raise ValueError("swaps whose first fixing is before the evaluation date need past fixings (not supported)")
    end = cal.advance(start, ql.Period(instrument.tenor), convention)

    fixed_schedule = ql.Schedule(
        start,
        end,
        ql.Period(FREQUENCIES[instrument.fixed_frequency]),
        cal,
        convention,
        convention,
        ql.DateGeneration.Forward,
        False,
    )
    float_schedule = ql.Schedule(
        start, end, index.tenor(), cal, convention, convention, ql.DateGeneration.Forward, False
    )
    swap = ql.VanillaSwap(
        ql.Swap.Payer if instrument.side == "Payer" else ql.Swap.Receiver,
        instrument.notional,
        fixed_schedule,
        instrument.fixed_rate,
        day_counter(instrument.fixed_day_counter),
        float_schedule,
        index,
        instrument.float_spread,
        index.dayCounter(),
    )
    swap.setPricingEngine(ql.DiscountingSwapEngine(discount))

    return {
        "npv": swap.NPV(),
        "fairRate": swap.fairRate(),
        "fairSpread": swap.fairSpread(),
        "fixedLegNpv": swap.fixedLegNPV(),
        "floatingLegNpv": swap.floatingLegNPV(),
        "fixedLegBps": swap.fixedLegBPS(),
        "floatingLegBps": swap.floatingLegBPS(),
        "startDate": to_iso(start),
        "maturityDate": to_iso(swap.maturityDate()),
    }

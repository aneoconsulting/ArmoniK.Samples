"""Construction of QuantLib term structures from the JSON market description."""

from __future__ import annotations

import datetime as dt

import QuantLib as ql

from .conventions import COMPOUNDINGS, FREQUENCIES, day_counter, to_ql_date
from .schema import Curve, FlatCurve


def build_curve(spec: Curve, evaluation: ql.Date) -> ql.YieldTermStructure:
    dc = day_counter(spec.day_counter)
    compounding = COMPOUNDINGS[spec.compounding]
    frequency = FREQUENCIES[spec.frequency]

    if isinstance(spec, FlatCurve):
        curve = ql.FlatForward(evaluation, spec.flat_rate, dc, compounding, frequency)
    else:
        points = sorted(
            (to_ql_date(pillar) if isinstance(pillar, dt.date) else evaluation + ql.Period(pillar), rate)
            for pillar, rate in spec.zero_points
        )
        dates = [d for d, _ in points]
        if dates[0] <= evaluation:
            raise ValueError("zeroPoints must all be after the evaluation date")
        if len(set(dates)) != len(dates):
            raise ValueError("zeroPoints contain the same date twice")
        # QuantLib needs the curve to start at its reference date: extend the first rate flat.
        dates = [evaluation] + dates
        rates = [points[0][1]] + [r for _, r in points]
        curve = ql.ZeroCurve(dates, rates, dc, ql.NullCalendar(), ql.Linear(), compounding, frequency)

    curve.enableExtrapolation()
    return curve


class BumpableCurve:
    """Curve with a parallel zero spread quote, so rates can be bumped for finite-difference risks."""

    def __init__(self, spec: Curve, evaluation: ql.Date):
        self.base = ql.YieldTermStructureHandle(build_curve(spec, evaluation))
        self.spread = ql.SimpleQuote(0.0)
        spreaded = ql.ZeroSpreadedTermStructure(self.base, ql.QuoteHandle(self.spread))
        spreaded.enableExtrapolation()
        self.handle = ql.YieldTermStructureHandle(spreaded)

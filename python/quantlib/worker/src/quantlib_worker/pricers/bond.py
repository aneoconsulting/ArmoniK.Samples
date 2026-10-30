"""Fixed rate bullet bonds."""

from __future__ import annotations

from typing import Any, Dict

import QuantLib as ql

from ..conventions import BUSINESS_DAY_CONVENTIONS, FREQUENCIES, calendar, day_counter, to_iso, to_ql_date
from ..market import build_curve
from ..schema import BondRequest


def price_bond(req: BondRequest, evaluation: ql.Date) -> Dict[str, Any]:
    instrument, market = req.instrument, req.market
    issue, maturity = to_ql_date(instrument.issue_date), to_ql_date(instrument.maturity)
    if maturity <= issue:
        raise ValueError("bond maturity must be after its issue date")

    cal = calendar(instrument.calendar)
    convention = BUSINESS_DAY_CONVENTIONS[instrument.business_day_convention]
    frequency = FREQUENCIES[instrument.frequency]
    dc = day_counter(instrument.day_counter)

    schedule = ql.Schedule(
        issue, maturity, ql.Period(frequency), cal, convention, convention, ql.DateGeneration.Backward, False
    )
    bond = ql.FixedRateBond(
        instrument.settlement_days,
        instrument.face_amount,
        schedule,
        [instrument.coupon],
        dc,
        convention,
        instrument.redemption,
        issue,
    )
    settlement = bond.settlementDate()
    if settlement >= maturity:
        raise ValueError(f"bond has matured at settlement date {to_iso(settlement)}")

    out: Dict[str, Any] = {"settlementDate": to_iso(settlement), "accruedAmount": bond.accruedAmount()}

    if market.discount_curve is not None:
        curve = ql.YieldTermStructureHandle(build_curve(market.discount_curve, evaluation))
        bond.setPricingEngine(ql.DiscountingBondEngine(curve))
        out["npv"] = bond.NPV()
        out["curveCleanPrice"] = bond.cleanPrice()

    if market.clean_price is not None:
        clean, out["priceSource"] = market.clean_price, "market"
    else:
        clean, out["priceSource"] = out["curveCleanPrice"], "curve"

    price = ql.BondPrice(clean, ql.BondPrice.Clean)
    ytm = ql.BondFunctions.bondYield(bond, price, dc, ql.Compounded, frequency, settlement)
    rate = ql.InterestRate(ytm, dc, ql.Compounded, frequency)

    out.update(
        {
            "cleanPrice": clean,
            "dirtyPrice": clean + out["accruedAmount"],
            "yield": ytm,
            "modifiedDuration": ql.BondFunctions.duration(bond, rate, ql.Duration.Modified, settlement),
            "macaulayDuration": ql.BondFunctions.duration(bond, rate, ql.Duration.Macaulay, settlement),
            "convexity": ql.BondFunctions.convexity(bond, rate, settlement),
            "basisPointValue": ql.BondFunctions.basisPointValue(bond, rate, settlement),
        }
    )
    return out

"""European and American vanilla options under Black-Scholes-Merton."""

from __future__ import annotations

from typing import Any, Callable, Dict, Optional

import QuantLib as ql

from ..conventions import calendar, day_counter, to_ql_date
from ..market import BumpableCurve
from ..schema import (
    AnalyticEuropeanEngine,
    BaroneAdesiWhaleyEngine,
    BinomialEngine,
    FiniteDifferenceEngine,
    MonteCarloEngine,
    OptionEngine,
    OptionRequest,
)

DEFAULT_ENGINES: Dict[str, OptionEngine] = {
    "EuropeanOption": AnalyticEuropeanEngine(type="AnalyticEuropean"),
    "AmericanOption": FiniteDifferenceEngine(type="FiniteDifference"),
}

# Relative spot bump and absolute vol / rate bumps used when the engine does not provide a greek.
SPOT_BUMP = 1e-3
VOL_BUMP = 1e-4
RATE_BUMP = 1e-4


def _make_engine(spec: OptionEngine, process: ql.GeneralizedBlackScholesProcess) -> ql.PricingEngine:
    if isinstance(spec, AnalyticEuropeanEngine):
        return ql.AnalyticEuropeanEngine(process)
    if isinstance(spec, BinomialEngine):
        return ql.BinomialVanillaEngine(process, spec.tree, spec.steps)
    if isinstance(spec, FiniteDifferenceEngine):
        return ql.FdBlackScholesVanillaEngine(process, spec.time_steps, spec.grid_points)
    if isinstance(spec, MonteCarloEngine):
        return ql.MCEuropeanEngine(
            process,
            "pseudorandom",
            timeSteps=spec.time_steps,
            antitheticVariate=spec.antithetic,
            requiredSamples=spec.samples,
            seed=spec.seed,
        )
    if isinstance(spec, BaroneAdesiWhaleyEngine):
        return ql.BaroneAdesiWhaleyApproximationEngine(process)
    raise ValueError(f"unsupported engine {spec.type}")


def _central(quote: ql.SimpleQuote, h: float, npv: Callable[[], float]) -> tuple[float, float]:
    """Return (npv(x+h), npv(x-h)) and restore the quote."""
    x = quote.value()
    try:
        quote.setValue(x + h)
        up = npv()
        quote.setValue(x - h)
        down = npv()
    finally:
        quote.setValue(x)
    return up, down


def price_option(req: OptionRequest, evaluation: ql.Date) -> Dict[str, Any]:
    instrument, market = req.instrument, req.market
    maturity = to_ql_date(instrument.maturity)
    if maturity <= evaluation:
        raise ValueError("option maturity must be after the evaluation date")

    cal = calendar(market.calendar)
    dc = day_counter(market.day_counter)

    spot = ql.SimpleQuote(market.spot)
    vol = ql.SimpleQuote(market.volatility)
    rates = BumpableCurve(market.discount_curve, evaluation)
    dividends = ql.YieldTermStructureHandle(ql.FlatForward(evaluation, market.dividend_yield, dc))
    vol_ts = ql.BlackVolTermStructureHandle(ql.BlackConstantVol(evaluation, cal, ql.QuoteHandle(vol), dc))
    process = ql.BlackScholesMertonProcess(ql.QuoteHandle(spot), dividends, rates.handle, vol_ts)

    option_type = ql.Option.Call if instrument.option_type == "Call" else ql.Option.Put
    if instrument.type == "EuropeanOption":
        exercise = ql.EuropeanExercise(maturity)
    else:
        exercise = ql.AmericanExercise(evaluation, maturity)
    option = ql.VanillaOption(ql.PlainVanillaPayoff(option_type, instrument.strike), exercise)

    engine_spec = req.engine or DEFAULT_ENGINES[instrument.type]
    option.setPricingEngine(_make_engine(engine_spec, process))

    out: Dict[str, Any] = {"engine": engine_spec.type, "npv": option.NPV()}
    if isinstance(engine_spec, MonteCarloEngine):
        out["errorEstimate"] = option.errorEstimate()

    bumped = []

    def native(name: str) -> Optional[float]:
        try:
            return getattr(option, name)()
        except RuntimeError:  # "<greek> not provided" by this engine
            return None

    def greek(name: str) -> Optional[float]:
        value = native(name)
        if value is not None:
            return value
        if name in ("delta", "gamma"):
            h = SPOT_BUMP * spot.value()
            up, down = _central(spot, h, option.NPV)
            value = (up - down) / (2 * h) if name == "delta" else (up - 2 * out["npv"] + down) / (h * h)
        elif name == "vega":
            up, down = _central(vol, VOL_BUMP, option.NPV)
            value = (up - down) / (2 * VOL_BUMP)
        elif name == "rho":
            up, down = _central(rates.spread, RATE_BUMP, option.NPV)
            value = (up - down) / (2 * RATE_BUMP)
        else:  # theta needs a time shift that frozen market data cannot provide
            return None
        bumped.append(name)
        return value

    greeks = {name: greek(name) for name in req.outputs if name != "npv"}
    if greeks:
        out["greeks"] = greeks
    if bumped:
        out["bumpedGreeks"] = bumped
    if "npv" not in req.outputs:
        del out["npv"]
    return out

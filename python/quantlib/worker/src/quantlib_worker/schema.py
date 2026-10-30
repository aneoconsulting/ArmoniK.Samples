"""JSON contract of the QuantLib worker.

A task payload is a :class:`PricingBatch`. Each element of ``requests`` is validated
independently (see :func:`parse_request`) so a single invalid request does not fail the
whole batch.
"""

from __future__ import annotations

import datetime as dt
import json
from typing import Annotated, Any, Dict, List, Literal, Optional, Tuple, Type, Union

from pydantic import BaseModel, ConfigDict, Field, PositiveFloat, PositiveInt, StringConstraints, model_validator
from pydantic.alias_generators import to_camel

from .conventions import BUSINESS_DAY_CONVENTIONS, CALENDARS, COMPOUNDINGS, DAY_COUNTERS, FREQUENCIES

SCHEMA_VERSION = 1

DayCounterName = Literal[tuple(DAY_COUNTERS)]  # type: ignore[valid-type]
CalendarName = Literal[tuple(CALENDARS)]  # type: ignore[valid-type]
FrequencyName = Literal[tuple(FREQUENCIES)]  # type: ignore[valid-type]
CompoundingName = Literal[tuple(COMPOUNDINGS)]  # type: ignore[valid-type]
BusinessDayConventionName = Literal[tuple(BUSINESS_DAY_CONVENTIONS)]  # type: ignore[valid-type]
Tenor = Annotated[str, StringConstraints(pattern=r"^\d+[DWMY]$")]


class Model(BaseModel):
    model_config = ConfigDict(alias_generator=to_camel, populate_by_name=True, extra="forbid", frozen=True)


# --- Curves --------------------------------------------------------------------------


class FlatCurve(Model):
    flat_rate: float
    compounding: CompoundingName = "Continuous"
    frequency: FrequencyName = "Annual"
    day_counter: DayCounterName = "Actual365Fixed"


class ZeroCurve(Model):
    """Zero rates interpolated linearly. Each point is ``[tenor or ISO date, rate]``."""

    zero_points: List[Tuple[Union[dt.date, Tenor], float]] = Field(min_length=1)
    compounding: CompoundingName = "Continuous"
    frequency: FrequencyName = "Annual"
    day_counter: DayCounterName = "Actual365Fixed"


Curve = Union[FlatCurve, ZeroCurve]


# --- Options -------------------------------------------------------------------------


class AnalyticEuropeanEngine(Model):
    type: Literal["AnalyticEuropean"]


class BinomialEngine(Model):
    type: Literal["Binomial"]
    tree: Literal["crr", "jr", "eqp", "trigeorgis", "tian", "lr", "joshi4"] = "crr"
    steps: PositiveInt = 500


class FiniteDifferenceEngine(Model):
    type: Literal["FiniteDifference"]
    time_steps: PositiveInt = 200
    grid_points: PositiveInt = 200


class MonteCarloEngine(Model):
    type: Literal["MonteCarlo"]
    samples: PositiveInt = 100_000
    time_steps: PositiveInt = 1
    seed: int = 42
    antithetic: bool = True


class BaroneAdesiWhaleyEngine(Model):
    type: Literal["BaroneAdesiWhaley"]


OptionEngine = Union[
    AnalyticEuropeanEngine, BinomialEngine, FiniteDifferenceEngine, MonteCarloEngine, BaroneAdesiWhaleyEngine
]

EUROPEAN_ENGINES = ("AnalyticEuropean", "Binomial", "FiniteDifference", "MonteCarlo")
AMERICAN_ENGINES = ("Binomial", "FiniteDifference", "BaroneAdesiWhaley")

OptionOutput = Literal["npv", "delta", "gamma", "vega", "theta", "rho"]


class VanillaOption(Model):
    type: Literal["EuropeanOption", "AmericanOption"]
    option_type: Literal["Call", "Put"]
    strike: PositiveFloat
    maturity: dt.date


class EquityMarket(Model):
    spot: PositiveFloat
    volatility: PositiveFloat
    dividend_yield: float = 0.0
    discount_curve: Curve
    day_counter: DayCounterName = "Actual365Fixed"
    calendar: CalendarName = "TARGET"


class OptionRequest(Model):
    id: str
    evaluation_date: Optional[dt.date] = None
    instrument: VanillaOption
    market: EquityMarket
    engine: Optional[OptionEngine] = Field(default=None, discriminator="type")
    outputs: List[OptionOutput] = ["npv", "delta", "gamma", "vega", "theta", "rho"]

    @model_validator(mode="after")
    def _check_engine(self) -> "OptionRequest":
        if self.engine is None:
            return self
        allowed = EUROPEAN_ENGINES if self.instrument.type == "EuropeanOption" else AMERICAN_ENGINES
        if self.engine.type not in allowed:
            raise ValueError(f"engine {self.engine.type} is not supported for {self.instrument.type}; use one of {allowed}")
        return self


# --- Bonds ---------------------------------------------------------------------------


class FixedRateBond(Model):
    type: Literal["FixedRateBond"]
    face_amount: PositiveFloat = 100.0
    coupon: float
    issue_date: dt.date
    maturity: dt.date
    frequency: FrequencyName = "Annual"
    day_counter: DayCounterName = "ActualActualISMA"
    calendar: CalendarName = "TARGET"
    business_day_convention: BusinessDayConventionName = "Unadjusted"
    settlement_days: int = Field(default=2, ge=0)
    redemption: PositiveFloat = 100.0


class BondMarket(Model):
    """``discountCurve`` prices the bond; ``cleanPrice`` (quoted, per 100) drives yield and risk measures.

    At least one of them is required. When only the curve is given, the yield is implied from the curve price.
    """

    discount_curve: Optional[Curve] = None
    clean_price: Optional[PositiveFloat] = None

    @model_validator(mode="after")
    def _check_any(self) -> "BondMarket":
        if self.discount_curve is None and self.clean_price is None:
            raise ValueError("market needs a discountCurve, a cleanPrice, or both")
        return self


class BondRequest(Model):
    id: str
    evaluation_date: Optional[dt.date] = None
    instrument: FixedRateBond
    market: BondMarket


# --- Swaps ---------------------------------------------------------------------------


class IborIndex(Model):
    name: Literal["Euribor"] = "Euribor"
    tenor: Tenor


class VanillaSwap(Model):
    type: Literal["VanillaSwap"]
    side: Literal["Payer", "Receiver"] = Field(description="Payer pays the fixed leg, Receiver receives it")
    notional: PositiveFloat
    fixed_rate: float
    tenor: Tenor
    start_date: Optional[dt.date] = Field(
        default=None, description="Defaults to spot: evaluation date + settlementDays business days"
    )
    settlement_days: int = Field(default=2, ge=0)
    fixed_frequency: FrequencyName = "Annual"
    fixed_day_counter: DayCounterName = "Thirty360BondBasis"
    float_index: IborIndex = IborIndex(tenor="6M")
    float_spread: float = 0.0
    calendar: CalendarName = "TARGET"
    business_day_convention: BusinessDayConventionName = "ModifiedFollowing"


class SwapMarket(Model):
    discount_curve: Curve
    forecast_curve: Optional[Curve] = Field(default=None, description="Defaults to discountCurve (single curve)")


class SwapRequest(Model):
    id: str
    evaluation_date: Optional[dt.date] = None
    instrument: VanillaSwap
    market: SwapMarket


PricingRequest = Union[OptionRequest, BondRequest, SwapRequest]

REQUEST_MODELS: Dict[str, Type[Model]] = {
    "EuropeanOption": OptionRequest,
    "AmericanOption": OptionRequest,
    "FixedRateBond": BondRequest,
    "VanillaSwap": SwapRequest,
}


# --- Batch ---------------------------------------------------------------------------


class PricingBatch(Model):
    """Task payload. ``requests`` stays raw here and is validated request by request."""

    version: Literal[1] = 1
    evaluation_date: dt.date
    requests: List[Any] = Field(min_length=1)


def parse_request(raw: Dict[str, Any]) -> PricingRequest:
    instrument = raw.get("instrument")
    instrument_type = instrument.get("type") if isinstance(instrument, dict) else None
    model = REQUEST_MODELS.get(instrument_type)  # type: ignore[arg-type]
    if model is None:
        raise ValueError(f"unknown instrument.type {instrument_type!r}; expected one of {sorted(REQUEST_MODELS)}")
    return model.model_validate(raw)  # type: ignore[return-value]


def json_schema() -> Dict[str, Any]:
    """JSON Schema of a full payload, with ``requests`` items typed for documentation and client-side validation."""
    schema = PricingBatch.model_json_schema(by_alias=True)
    items = {"anyOf": []}
    for model in (OptionRequest, BondRequest, SwapRequest):
        sub = model.model_json_schema(by_alias=True, ref_template="#/$defs/{model}")
        schema.setdefault("$defs", {}).update(sub.pop("$defs", {}))
        schema["$defs"][model.__name__] = sub
        items["anyOf"].append({"$ref": f"#/$defs/{model.__name__}"})
    schema["properties"]["requests"]["items"] = items
    schema["$schema"] = "https://json-schema.org/draft/2020-12/schema"
    schema["title"] = "ArmoniK QuantLib pricing batch"
    return schema


if __name__ == "__main__":
    print(json.dumps(json_schema(), indent=2))

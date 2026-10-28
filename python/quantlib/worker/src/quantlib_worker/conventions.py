"""Mapping of the JSON convention names to QuantLib objects."""

from __future__ import annotations

import datetime as dt
from typing import Callable, Dict

import QuantLib as ql

DAY_COUNTERS: Dict[str, Callable[[], ql.DayCounter]] = {
    "Actual360": ql.Actual360,
    "Actual365Fixed": ql.Actual365Fixed,
    "ActualActualISDA": lambda: ql.ActualActual(ql.ActualActual.ISDA),
    "ActualActualISMA": lambda: ql.ActualActual(ql.ActualActual.ISMA),
    "ActualActualBond": lambda: ql.ActualActual(ql.ActualActual.Bond),
    "Thirty360BondBasis": lambda: ql.Thirty360(ql.Thirty360.BondBasis),
    "Thirty360European": lambda: ql.Thirty360(ql.Thirty360.European),
    "Business252": ql.Business252,
}

CALENDARS: Dict[str, Callable[[], ql.Calendar]] = {
    "TARGET": ql.TARGET,
    "NullCalendar": ql.NullCalendar,
    "France": ql.France,
    "Germany": ql.Germany,
    "UnitedKingdom": ql.UnitedKingdom,
    "UnitedStates": lambda: ql.UnitedStates(ql.UnitedStates.GovernmentBond),
    "UnitedStatesNYSE": lambda: ql.UnitedStates(ql.UnitedStates.NYSE),
    "Japan": ql.Japan,
}

FREQUENCIES: Dict[str, int] = {
    "Annual": ql.Annual,
    "Semiannual": ql.Semiannual,
    "Quarterly": ql.Quarterly,
    "Monthly": ql.Monthly,
}

COMPOUNDINGS: Dict[str, int] = {
    "Simple": ql.Simple,
    "Compounded": ql.Compounded,
    "Continuous": ql.Continuous,
}

BUSINESS_DAY_CONVENTIONS: Dict[str, int] = {
    "Following": ql.Following,
    "ModifiedFollowing": ql.ModifiedFollowing,
    "Preceding": ql.Preceding,
    "Unadjusted": ql.Unadjusted,
}


def day_counter(name: str) -> ql.DayCounter:
    return DAY_COUNTERS[name]()


def calendar(name: str) -> ql.Calendar:
    return CALENDARS[name]()


def to_ql_date(d: dt.date) -> ql.Date:
    return ql.Date(d.day, d.month, d.year)


def to_iso(d: ql.Date) -> str:
    return dt.date(d.year(), d.month(), d.dayOfMonth()).isoformat()

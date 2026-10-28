import json

import pytest

from quantlib_worker.batch import InvalidPayload, process_payload

EVAL = "2026-10-22"
FLAT_5 = {"flatRate": 0.05}


def run(*requests, evaluation_date=EVAL):
    payload = {"version": 1, "evaluationDate": evaluation_date, "requests": list(requests)}
    return json.loads(process_payload(json.dumps(payload).encode()))["results"]


def run_one(request):
    (result,) = run(request)
    assert "error" not in result, result["error"]
    return result


def option(kind="EuropeanOption", option_type="Call", engine=None, spot=100.0, strike=100.0, **market):
    request = {
        "id": f"{kind}-{option_type}",
        "instrument": {"type": kind, "optionType": option_type, "strike": strike, "maturity": "2027-10-22"},
        "market": {"spot": spot, "volatility": 0.2, "discountCurve": FLAT_5, **market},
    }
    if engine:
        request["engine"] = engine
    return request


def bond(market, **instrument):
    return {
        "id": "bond",
        "instrument": {
            "type": "FixedRateBond",
            "coupon": 0.04,
            "issueDate": EVAL,
            "maturity": "2031-10-22",
            "settlementDays": 0,
            **instrument,
        },
        "market": market,
    }


def swap(side="Payer", fixed_rate=0.035, **instrument):
    return {
        "id": f"swap-{side}",
        "instrument": {
            "type": "VanillaSwap",
            "side": side,
            "notional": 1_000_000,
            "fixedRate": fixed_rate,
            "tenor": "5Y",
            **instrument,
        },
        "market": {"discountCurve": {"flatRate": 0.03}, "forecastCurve": {"flatRate": 0.032}},
    }


# --- Options -------------------------------------------------------------------------


def test_black_scholes_reference_call():
    # S=100, K=100, r=5%, sigma=20%, T=1y: textbook value 10.4506
    result = run_one(option())
    assert result["engine"] == "AnalyticEuropean"
    assert result["npv"] == pytest.approx(10.4506, abs=1e-4)
    assert result["greeks"]["delta"] == pytest.approx(0.6368, abs=1e-4)
    assert "bumpedGreeks" not in result


def test_put_call_parity():
    call, put = run(option(option_type="Call"), option(option_type="Put"))
    # C - P = S - K e^{-rT} (T = 365/365 with Actual365Fixed)
    assert call["npv"] - put["npv"] == pytest.approx(100 - 100 * 0.951229424500714, abs=1e-8)


@pytest.mark.parametrize(
    "engine,tol",
    [
        ({"type": "Binomial", "steps": 1000}, 5e-3),
        ({"type": "FiniteDifference", "timeSteps": 400, "gridPoints": 400}, 5e-3),
        ({"type": "MonteCarlo", "samples": 200_000}, 5e-2),
    ],
)
def test_numerical_engines_converge_to_analytic(engine, tol):
    analytic = run_one(option())["npv"]
    assert run_one(option(engine=engine))["npv"] == pytest.approx(analytic, abs=tol)


def test_bumped_greeks_match_analytic():
    analytic = run_one(option())["greeks"]
    result = run_one(option(engine={"type": "Binomial", "steps": 1000}))
    assert set(result["bumpedGreeks"]) == {"vega", "rho"}
    assert result["greeks"]["vega"] == pytest.approx(analytic["vega"], rel=1e-2)
    assert result["greeks"]["rho"] == pytest.approx(analytic["rho"], rel=1e-2)


def test_monte_carlo_has_error_estimate_and_no_theta():
    result = run_one(option(engine={"type": "MonteCarlo", "samples": 10_000}))
    assert result["errorEstimate"] > 0
    assert result["greeks"]["theta"] is None


def test_american_put_worth_at_least_european():
    european = run_one(option(option_type="Put"))["npv"]
    for engine in ({"type": "Binomial"}, {"type": "FiniteDifference"}, {"type": "BaroneAdesiWhaley"}):
        assert run_one(option("AmericanOption", "Put", engine))["npv"] > european + 0.1


def test_american_call_without_dividends_equals_european():
    european = run_one(option())["npv"]
    assert run_one(option("AmericanOption"))["npv"] == pytest.approx(european, abs=1e-2)


def test_outputs_selection():
    request = option()
    request["outputs"] = ["delta"]
    result = run_one(request)
    assert "npv" not in result
    assert result["greeks"] == {"delta": pytest.approx(0.6368, abs=1e-4)}


def test_engine_not_allowed_for_instrument():
    (result,) = run(option("AmericanOption", engine={"type": "AnalyticEuropean"}))
    assert "not supported for AmericanOption" in result["error"]


def test_expired_option_is_an_error():
    (result,) = run(option(), evaluation_date="2028-01-01")
    assert "maturity must be after" in result["error"]


def test_single_point_zero_curve_equals_flat_curve():
    flat = run_one(option())["npv"]
    zero = run_one(option(discountCurve={"zeroPoints": [["1Y", 0.05]]}))["npv"]
    assert zero == pytest.approx(flat, abs=1e-12)


# --- Bonds ---------------------------------------------------------------------------


def test_bond_prices_at_par_when_coupon_equals_flat_yield():
    curve = {"flatRate": 0.04, "compounding": "Compounded", "frequency": "Annual", "dayCounter": "ActualActualISMA"}
    result = run_one(bond({"discountCurve": curve}))
    assert result["priceSource"] == "curve"
    assert result["cleanPrice"] == pytest.approx(100.0, abs=1e-6)
    assert result["yield"] == pytest.approx(0.04, abs=1e-8)
    assert result["accruedAmount"] == pytest.approx(0.0)
    assert 0 < result["modifiedDuration"] < result["macaulayDuration"] < 5


def test_bond_market_price_drives_yield():
    from_curve = run_one(bond({"discountCurve": {"flatRate": 0.03}}))
    from_market = run_one(bond({"cleanPrice": from_curve["cleanPrice"]}))
    assert from_market["priceSource"] == "market"
    assert "npv" not in from_market
    assert from_market["yield"] == pytest.approx(from_curve["yield"], abs=1e-8)


def test_bond_requires_curve_or_price():
    (result,) = run(bond({}))
    assert "discountCurve, a cleanPrice, or both" in result["error"]


def test_matured_bond_is_an_error():
    (result,) = run(bond({"cleanPrice": 100}), evaluation_date="2032-01-01")
    assert "matured" in result["error"]


# --- Swaps ---------------------------------------------------------------------------


def test_swap_at_fair_rate_has_zero_npv():
    fair_rate = run_one(swap())["fairRate"]
    assert run_one(swap(fixed_rate=fair_rate))["npv"] == pytest.approx(0.0, abs=1e-6)


def test_payer_and_receiver_are_opposite():
    payer, receiver = run(swap("Payer"), swap("Receiver"))
    assert payer["npv"] == pytest.approx(-receiver["npv"], abs=1e-8)
    assert payer["npv"] < 0  # pays 3.5% fixed against a ~3.25% forward


def test_swap_with_past_start_is_an_error():
    (result,) = run(swap(startDate="2026-01-02"))
    assert "past fixings" in result["error"]


# --- Batch ---------------------------------------------------------------------------


def test_errors_are_isolated_per_request():
    results = run(option(), {"id": "bad", "instrument": {"type": "Nope"}}, "not an object", swap())
    assert [r["id"] for r in results] == ["EuropeanOption-Call", "bad", None, "swap-Payer"]
    assert "error" not in results[0] and "error" not in results[3]
    assert "unknown instrument.type 'Nope'" in results[1]["error"]
    assert "JSON object" in results[2]["error"]


def test_invalid_field_reports_its_path():
    request = option()
    request["market"]["volatility"] = -1
    (result,) = run(request)
    assert result["error"].startswith("invalid request: market.volatility:")


def test_per_request_evaluation_date_overrides_batch():
    request = option()
    request["evaluationDate"] = "2027-04-22"  # half a year left
    assert run_one(request)["npv"] < run_one(option())["npv"]


@pytest.mark.parametrize(
    "payload",
    [b"not json", b"{}", b'{"evaluationDate": "2026-10-22", "requests": []}', b'{"version": 2, "evaluationDate": "2026-10-22", "requests": [{}]}'],
)
def test_invalid_payload_fails_the_task(payload):
    with pytest.raises(InvalidPayload):
        process_payload(payload)

# QuantLib pricing

An ArmoniK worker that prices vanilla options, fixed rate bonds and interest rate swaps with
[QuantLib](https://github.com/lballabio/quantlib), and a Python client that submits them. Requests and results are plain
JSON and tasks follow the ArmoniK SDK convention, so any ArmoniK client can use the worker: C# clients live in
[csharp/native/QuantLib](../../csharp/native/QuantLib) (gRPC API) and [csharp/sdk/QuantLib](../../csharp/sdk/QuantLib)
(C# SDK), a C++ one in [cpp/QuantLib](../../cpp/QuantLib) (C++ SDK).

```
worker/    Python worker (QuantLib 1.44 wheels + armonik SDK), unit tests, Dockerfile
client/    Python client: JSON file in, JSON results out
samples/   Example payloads
schema/    JSON Schema of a task payload (generated: python -m quantlib_worker.schema)
build.sh   Image builds: native, foreign architecture, multi-arch
```

## Build

All dependencies ship as manylinux wheels for `x86_64` and `aarch64`: nothing is compiled and the same Dockerfile serves
both architectures (`pip install --only-binary=:all:` fails fast if a wheel is ever missing).

```bash
./build.sh                                      # worker + client, local architecture
./build.sh worker --test                        # also run the unit tests during the build (target "test")
./build.sh worker --test --platform linux/amd64 # x86 image on an ARM host (or the reverse), needs QEMU binfmt
./build.sh all --platform linux/amd64,linux/arm64 --push --registry docker.io/<me>   # multi-arch manifest
```

Images: `armonik_quantlib_worker:<tag>` and `armonik_quantlib_client:<tag>` (default tag `0.2.1`).

Foreign architectures are emulated with QEMU through `binfmt_misc`. To register it (once per boot):
`docker run --privileged --rm tonistiigi/binfmt --install amd64` (or `arm64`).

## Deploy

With the [ArmoniK infrastructure](https://aneoconsulting.github.io/ArmoniK/), create a partition (e.g. `quantlib`)
using the worker image and pass `--partition quantlib` to the clients.

With a local [ArmoniK.Core](https://github.com/aneoconsulting/ArmoniK.Core) deployment, build the images here, then only
pass the worker image (tasks then go to the default partition):

```bash
cd <path to>/ArmoniK.Core
WORKER_IMAGE=armonik_quantlib_worker:0.2.1 just tag=<core tag> build-core deploy   # or just `deploy`
```

Do **not** use `just build-deploy`: its `buildWorker` step rebuilds `WORKER_IMAGE` from the default HtcMock Dockerfile
and overwrites the QuantLib image.

The worker supports both `socket_type=unixdomainsocket` (default) and `socket_type=tcp`, and runs as UID 1654 like the
polling agent so they can share the unix sockets.

## Run the client

```bash
docker run --rm --net armonik_network -v $PWD/samples:/data/samples:ro \
  -e GrpcClient__Endpoint=http://armonik.control.submitter:1080 \
  armonik_quantlib_client:0.2.1 samples/mixed_batch.json --batch-size 3

# load test: 100 copies of each request, 10 requests per task, results to a file
docker run --rm --net armonik_network -v $PWD/samples:/data/samples:ro -v $PWD:/out \
  -e GrpcClient__Endpoint=http://armonik.control.submitter:1080 \
  armonik_quantlib_client:0.2.1 samples/mixed_batch.json --repeat 100 --batch-size 10 -o /out/results.json
```

The input is a full payload, a list of requests or a single request. Options: `--batch-size`, `--repeat`,
`--partition`, `--evaluation-date`, `--max-retries`, `--max-duration`, `--output`, `--keep-session`. The merged
results go to stdout (or `--output`), a summary to stderr. The exit code is 1 when any request or task failed.

## JSON contract

### Task layout (ArmoniK SDK convention v1)

The worker follows the convention shared by the ArmoniK C# and C++ SDKs, so their clients can submit to it:

- the task options contain `ConventionVersion=v1` (the session default options are enough);
- the task has one input blob named `requests`, declared as a data dependency, holding the pricing payload below;
- the task has one output blob named `results`, receiving the task result below; when the payload names no output
  (`"outputs": {}`, as the C++ SDK does), the single expected output of the task is used;
- the task payload only maps these names to blob ids: `{"inputs": {"requests": "<id>"}, "outputs": {"results": "<id>"}}`.

### Pricing payload

One task prices a batch of requests, possibly of different kinds:

```json
{
  "version": 1,
  "evaluationDate": "2026-10-22",
  "requests": [
    {
      "id": "eu-call-atm",
      "instrument": { "type": "EuropeanOption", "optionType": "Call", "strike": 100, "maturity": "2027-10-22" },
      "market": { "spot": 100, "volatility": 0.2, "dividendYield": 0.0, "discountCurve": { "flatRate": 0.05 } },
      "engine": { "type": "AnalyticEuropean" }
    }
  ]
}
```

Field names are camelCase and unknown fields are rejected (typos do not go unnoticed). A request may override
`evaluationDate`. See `samples/` for every instrument and `schema/pricing-request.schema.json` for the full schema.

| `instrument.type` | Market | Engines (`engine.type`) | Default engine |
|---|---|---|---|
| `EuropeanOption` | `spot`, `volatility`, `dividendYield`, `discountCurve` | `AnalyticEuropean`, `Binomial`, `FiniteDifference`, `MonteCarlo` | `AnalyticEuropean` |
| `AmericanOption` | same | `Binomial`, `FiniteDifference`, `BaroneAdesiWhaley` | `FiniteDifference` |
| `FixedRateBond` | `discountCurve` and/or `cleanPrice` | discounting | |
| `VanillaSwap` | `discountCurve`, optional `forecastCurve` | discounting | |

Curves are either flat, `{"flatRate": 0.03}`, or zero rates interpolated linearly,
`{"zeroPoints": [["1Y", 0.030], ["2030-01-15", 0.035]]}` (tenor `\d+[DWMY]` or ISO date). Both accept
`compounding` (`Continuous` by default, `Compounded`, `Simple`), `frequency` and `dayCounter` (`Actual365Fixed`).

Conventions: day counters `Actual360`, `Actual365Fixed`, `ActualActualISDA`, `ActualActualISMA`, `ActualActualBond`,
`Thirty360BondBasis`, `Thirty360European`, `Business252`; calendars `TARGET`, `NullCalendar`, `France`, `Germany`,
`UnitedKingdom`, `UnitedStates`, `UnitedStatesNYSE`, `Japan`; frequencies `Annual`, `Semiannual`, `Quarterly`,
`Monthly`; business day conventions `Following`, `ModifiedFollowing`, `Preceding`, `Unadjusted`. Swap floating
legs use `{"name": "Euribor", "tenor": "6M"}`.

### Task result

```json
{
  "version": 1,
  "evaluationDate": "2026-10-22",
  "quantlibVersion": "1.44",
  "results": [
    { "id": "eu-call-atm", "type": "EuropeanOption", "engine": "AnalyticEuropean", "npv": 10.4506,
      "greeks": { "delta": 0.6368, "gamma": 0.0188, "vega": 37.52, "theta": -6.41, "rho": 53.23 }, "elapsedMs": 0.4 },
    { "id": "bad", "error": "invalid request: instrument.strike: Input should be greater than 0", "elapsedMs": 0.05 }
  ]
}
```

`results` follows the order of `requests`. Outputs:

- **Options**: `npv`, `greeks` (QuantLib units: vega and rho per 1.00 of vol/rate, theta per year). `outputs` selects
  a subset of `npv`, `delta`, `gamma`, `vega`, `theta`, `rho`. Greeks the engine does not provide are computed by
  central bump and reprice and listed in `bumpedGreeks`; `theta` is `null` when the engine does not provide it.
  Monte Carlo adds `errorEstimate`.
- **FixedRateBond** (prices per 100 of face): `cleanPrice`, `dirtyPrice`, `accruedAmount`, `yield` (compounded at
  the coupon frequency), `modifiedDuration`, `macaulayDuration`, `convexity`, `basisPointValue`, `settlementDate`,
  `priceSource` (`market` when `cleanPrice` is given, else `curve`); with a curve also `npv` and `curveCleanPrice`.
- **VanillaSwap**: `npv`, `fairRate`, `fairSpread`, `fixedLegNpv`, `floatingLegNpv`, `fixedLegBps`,
  `floatingLegBps`, `startDate`, `maturityDate`. Swaps whose first fixing is in the past are rejected.

### Errors

- An invalid or unpriceable **request** yields `{"id", "error"}` in `results`; the rest of the batch is priced.
- An invalid **task** (missing `ConventionVersion=v1`, task payload not following the convention, pricing payload not
  JSON, wrong `version`, no `requests`...) fails with `invalid payload: ...` as its output error. ArmoniK does not retry tasks failed by the worker.

## Submitting from another client

With the C# SDK, give each task an input named `requests` with the pricing payload and an output named `results`: the
SDK handles the rest ([csharp/sdk/QuantLib](../../csharp/sdk/QuantLib)). With the C++ SDK, give each task the input
`requests` and set `ConventionVersion=v1` in the task options, which `Submit` does not do without a dynamic library
([cpp/QuantLib](../../cpp/QuantLib)).

With the gRPC API ([csharp/native/QuantLib](../../csharp/native/QuantLib) is the C# version), per batch:

1. create a session with `ConventionVersion=v1` in its default task options (`Sessions.CreateSession`);
2. create the input with the pricing payload (`Results.CreateResults`) and one empty output (`Results.CreateResultsMetaData`);
3. create the task payload referencing them, `{"inputs": {"requests": <input id>}, "outputs": {"results": <output id>}}`
   (`Results.CreateResults`);
4. submit a task with that payload, the output as expected output and the input as data dependency (`Tasks.SubmitTasks`);
5. wait for the output (`Events.GetEvents`, or poll `Results.GetResult` until `COMPLETED`/`ABORTED`) and download it
   (`Results.DownloadResultData`); on `ABORTED`, the reason is the owner task's `output.error`.

`client/src/quantlib_client/__main__.py` is a complete example in about 180 lines.

## Development

```bash
docker build --target test worker/          # unit tests (pytest) in the image
docker run --rm --entrypoint python armonik_quantlib_worker:0.2.1 -m quantlib_worker.schema \
  > schema/pricing-request.schema.json      # regenerate the schema after a contract change
```

`worker/requirements.txt` and `client/requirements.txt` pin the full dependency set used in the images; direct
dependencies are declared in each `pyproject.toml`.

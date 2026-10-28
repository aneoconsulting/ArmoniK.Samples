# QuantLib pricing client

A C# client for the [QuantLib pricing worker](../../../python/quantlib) (a Python worker): it reads pricing requests
(options, bonds, swaps) from a JSON file, sends them to ArmoniK in tasks of several requests, and prints the results as
JSON in request order. The worker and the JSON contract are described in the worker's README.

## What it does

1. Reads the input file: a full payload (`{"evaluationDate": ..., "requests": [...]}`), a list of requests or a single
   request.
2. Creates a session and splits the requests in batches of `--batch-size`.
3. Per batch, uploads the JSON payload, reserves an output and submits one task.
4. Waits for every output (completed or aborted), downloads the results and reassembles them in request order; the
   requests of a failed task get the task error.

## Project structure

```
QuantLib/
└── Client/
    └── Program.cs   # Input parsing, batching, task submission, result retrieval
```

## Key concepts

- Uploading a JSON payload per task with `ResultsClient.CreateResults`
- Submitting many tasks in a single `TasksClient.SubmitTasks` call
- Waiting for results with `EventsClient.WaitForResultsAsync`, and handling aborted results through the owner task's
  `Output.Error`
- Calling a worker written in another language: the client only depends on the JSON contract

## Running

Deploy the worker first (see [python/quantlib](../../../python/quantlib)), then:

```bash
dotnet run --project Client -- ../../../python/quantlib/samples/mixed_batch.json \
  --endpoint http://localhost:5001 --batch-size 3
```

Or with Docker, on the network of a local ArmoniK.Core deployment:

```bash
docker build -t armonik_quantlib_client_csharp -f QuantLib/Client/Dockerfile .   # from csharp/native
docker run --rm --net armonik_network -v $PWD/../../python/quantlib/samples:/data/samples:ro \
  armonik_quantlib_client_csharp samples/mixed_batch.json --endpoint http://armonik.control.submitter:1080
```

| Option | Default | Description |
|---|---|---|
| `input` | | JSON file with the pricing requests |
| `--endpoint` | `http://localhost:5001` | ArmoniK control plane URL |
| `--partition` | (default partition) | Partition running the QuantLib worker |
| `--batch-size` | `10` | Pricing requests per task |
| `--repeat` | `1` | Submit the requests N times, for load tests |
| `--evaluation-date` | today | Used when the input has none |
| `--max-retries` | `2` | Task retries on worker failure |
| `--output` | stdout | File receiving the results JSON |

The exit code is 1 when any request or task failed.

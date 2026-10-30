# QuantLib pricing with the C# SDK

A C# client for the [QuantLib pricing worker](../../../python/quantlib), a Python worker, written with the ArmoniK C#
SDK (`ArmoniK.Extensions.CSharp.Client`). It reads pricing requests (options, bonds, swaps) from a JSON file, sends them
in tasks of several requests and prints the results as JSON in request order.

[csharp/native/QuantLib](../../native/QuantLib) does the same with the gRPC API: compare both to see what the SDK
handles for you.

## How it works

The SDK and the worker share the ArmoniK SDK convention v1: each task declares named inputs and outputs, and the SDK
uploads the inputs, creates the outputs and a task payload mapping the names to blob ids. The worker expects:

- an input named `requests`: the JSON document of a batch of pricing requests;
- an output named `results`: the JSON results of the batch.

```csharp
new TaskDefinition().WithTaskOptions(taskConfiguration)
                    .WithInput("requests", InputBlobDefinition.FromString("requests-0", json))
                    .WithOutput("results", OutputBlobDefinition.CreateOutput("results-0").WithCallback(callback));
```

The callback receives the content of each output, or the reason why it was aborted. The worker and the JSON contract are
described in the [worker's README](../../../python/quantlib/README.md).

## Running

Deploy the worker in a partition (see [python/quantlib](../../../python/quantlib)), then, with the
[.NET 10 SDK](https://dotnet.microsoft.com/download):

```bash
cd csharp/sdk
dotnet run --project QuantLib -- \
  --Input ../../python/quantlib/samples/mixed_batch.json \
  --Grpc:EndPoint http://localhost:5001 \
  --Partition quantlib \
  --BatchSize 3
```

| Key | Description | Default |
|-----|-------------|---------|
| `Input` | JSON file with the pricing requests, `{"evaluationDate": ..., "requests": [...]}` | *(required)* |
| `Grpc:EndPoint` | ArmoniK control-plane endpoint (`Grpc__EndPoint` as environment variable) | `http://localhost:5001` |
| `Partition` | Partition running the QuantLib worker | `quantlib` |
| `BatchSize` | Pricing requests per task | `10` |

Values come from `appsettings.json`, environment variables, then command-line flags (highest priority). The results go
to stdout, the logs to stderr. The exit code is 1 when any request or task failed.

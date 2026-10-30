# QuantLib pricing with the C++ SDK

A C++ client for the [QuantLib pricing worker](../../python/quantlib), a Python worker, written with the ArmoniK C++
SDK. It reads pricing requests (options, bonds, swaps) from a JSON file, sends them in tasks of several requests and
prints the results as JSON in request order. The same client exists in C# with the
[C# SDK](../../csharp/sdk/QuantLib) and the [gRPC API](../../csharp/native/QuantLib).

## How it works

The SDK and the worker share the ArmoniK SDK convention v1: each task declares named inputs, and the SDK uploads them
and creates a task payload mapping the names to blob ids, plus one output per task. The worker expects an input named
`requests`, the JSON document of a batch of pricing requests, and writes the results to the output of the task.

```cpp
task_options.SetConvention("price");
...
service.Submit({TaskDefinition().WithInput("requests", BlobDefinition::FromData(json))}, handler);
```

`SetConvention` sets `ConventionVersion=v1` and the method name (`Symbol`, ignored by the worker): `Submit` does not
set them itself, and the dynamic library of `SetDynamicLibrary` is not needed by a Python worker. The handler receives
the content of each output, or the reason why its task failed.

## Running

Deploy the worker in a partition (see [python/quantlib](../../python/quantlib)), then build the client image:

```bash
make build_client       # armonik-samples-cpp-quantlib-client:0.0.0-sdk, SDK version from ../versions.env
```

And run it, from the repository root:

```bash
docker run --rm -v $PWD/python/quantlib/samples:/app/samples:ro \
  -e GrpcClient__Endpoint=http://<control plane>:5001 -e PartitionId=quantlib -e BatchSize=3 \
  armonik-samples-cpp-quantlib-client:0.0.0-sdk
```

| Key | Description | Default (`appsettings.json`) |
|-----|-------------|---------|
| `Input` | JSON file with the pricing requests, `{"evaluationDate": ..., "requests": [...]}` | `samples/mixed_batch.json` |
| `GrpcClient__Endpoint` | ArmoniK control-plane endpoint | `http://localhost:5001` |
| `PartitionId` | Partition running the QuantLib worker | `quantlib` |
| `BatchSize` | Pricing requests per task, at least 1 | `10` |
| `MaxDuration` | Task timeout in seconds, at least 1 | `300` |

Environment variables override `appsettings.json`. The results go to stdout, the logs to stderr. The exit code is 1
when any request or task failed, and also when the client itself fails (invalid setting or input file, unreachable
endpoint); the error is then logged and nothing is printed on stdout.

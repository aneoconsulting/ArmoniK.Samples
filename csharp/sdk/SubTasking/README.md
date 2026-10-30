# Subtasking from a dynamically loaded C# worker

This sample shows a worker library, loaded at runtime by the C# dynamic worker, that submits its own subtasks.

The client submits a single task computing

$$S(lo, hi) = \sum_{i=lo}^{hi-1} i^2$$

for $lo = 1$, $hi = N + 1$. The worker evaluates it by recursive splitting:

- If $hi - lo \le grain$, the task computes the sum directly and writes its `result` output.
- Otherwise it picks $mid = lo + \lfloor (hi - lo)/2 \rfloor$ and submits three subtasks:
  - `left` on $[lo, mid)$,
  - `right` on $[mid, hi)$,
  - `combine`, with inputs the outputs of `left` and `right`, and output the parent's own `result`.

The parent then returns without writing `result`. Ownership of that output passes to the `combine` subtask, so the client's
result is produced at the root of a task tree the client never sees. ArmoniK starts each `combine` task only once both
of its inputs exist.

With the defaults ($N = 1000$, $grain = 100$) the tree has 16 leaves, 15 split tasks and 15 combine tasks, and the
result is $N(N+1)(2N+1)/6 = 333\,833\,500$.

## Project layout

| Project | Description |
|---------|-------------|
| `SubTasking.Worker` | Class library (`net10.0`) with `RangeSumWorker : IWorker`. `dotnet publish` packages it into a zip. |
| `SubTasking.Client` | Console app that uploads the zip, submits the root task and checks the result. |

The worker uses `ISdkTaskHandler` from `ArmoniK.Extensions.CSharp.Worker.Interfaces`:

- `TaskOptions.GetDynamicLibrary()` recovers the library descriptor (blob id, dll path, symbol) of the running task,
  so subtasks run the same code on the same partition.
- `BlobDefinition.FromBlobHandle(...)` reuses existing blobs: the parent's inputs `lo`, `hi`, `grain` as subtask
  inputs, and the parent's output as the `combine` task's output.
- `SubmitTasksAsync` creates the new blobs (`mid`, `left`, `right`, payloads) and submits the three tasks in one call.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download).
- An ArmoniK cluster with a partition running `dockerhubaneo/armonik-dynamic-dotnet-worker`. The image must run on
  .NET 10 to load the worker library, and its tag should match the SDK version used here
  (`0.23.1`). Images up to `0.22.0` run on .NET 8.

## Building and running

1. Build and package the worker:

   ```bash
   cd csharp/sdk/SubTasking
   dotnet publish -c Release SubTasking.Worker
   ```

   This produces `SubTasking.Worker/packages/SubTasking.Worker-v1.0.0.zip`, with the dll at
   `SubTasking.Worker/1.0.0/SubTasking.Worker.dll` inside the archive.

2. Run the client from `csharp/sdk/SubTasking`:

   ```bash
   dotnet run --project SubTasking.Client -- --Grpc:EndPoint http://<ingress-ip>:5001
   ```

   | Key | Description | Default |
   |-----|-------------|---------|
   | `Grpc:EndPoint` | Control plane endpoint | `http://localhost:5001` |
   | `Partition` | Partition running the C# dynamic worker | `default` |
   | `WorkerZip` | Path to the worker zip, relative to the current directory | `SubTasking.Worker/packages/SubTasking.Worker-v1.0.0.zip` |
   | `WorkerVersion` | Worker version, used to locate the dll in the zip | `1.0.0` |
   | `N` | Upper bound of the sum | `1000` |
   | `Grain` | Largest range computed directly by a leaf | `100` |

   Values come from `appsettings.json`, environment variables (`Grpc__EndPoint`, ...) or command-line flags, in
   increasing priority.

3. Expected output:

   ```
   [INF] Session opened: <session-id>
   [INF] Uploaded worker library blob: <blob-id>
   [INF] Root task submitted: <task-id> (N = 1000, grain = 100)
   [INF] Result: 333833500, expected: 333833500
   ```

   The worker logs (`kubectl -n armonik logs <pod> -c worker`) show the `Split`, `Leaf` and `Combine` steps.

## Notes

- Sums are computed in `long` with overflow checks, so $N$ must stay below about $2 \times 10^6$.
- `WithLibrary` adds the library blob to each subtask's data dependencies, which the dynamic worker needs to load it.

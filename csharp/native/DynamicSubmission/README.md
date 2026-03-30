# Dynamic Submission

Shows dynamic sub-task submission: a worker splits a summation problem into threshold-sized chunks, submits one sub-task per chunk, and then submits an aggregation task to sum the partial results.

## What it does

1. The client builds a `Table` containing the sequence `[1, 2, …, n]`, uploads it as a payload, and submits a single root task.
2. The root task checks whether the table is larger than the threshold:
   - If yes: splits the table into `⌈size / threshold⌉` sub-tables, submits one sub-task per chunk, then submits an aggregation task that declares all partial-result IDs as data dependencies.
   - If no (leaf task or aggregation task): sums the values directly, or sums the data-dependency results if dependencies are present, and writes the final `uint` result.
3. The client verifies the result equals `n*(n+1)/2` and prints it.

The `Table` payload (defined in `Common/Payload.cs`) is JSON-serialised and shared between client and worker.

## Project structure

```
DynamicSubmission/
├── Common/
│   └── Payload.cs        # Table record: Size, Threshold, Values[], Serialize/Deserialize
├── Client/
│   └── Program.cs        # Builds Table payload, submits root task, validates result
└── Worker/
    ├── Program.cs         # Worker entry point
    └── DynamicSubmission.cs # WorkerStreamWrapper with split/aggregate logic
```

## Key concepts

- Shared payload type (`Table`) used by both client and worker
- Worker dynamically partitioning work and spawning sub-tasks at runtime
- Aggregation task using `DataDependencies` to collect partial results
- `taskHandler.CreateResultsMetaDataAsync` and `taskHandler.SubmitTasksAsync` for bulk sub-task creation

## Running

### Client

```bash
dotnet run --project Client -- \
  --endpoint http://localhost:5001 \
  --partition default \
  --n 10 \
  --threshold 3
```

| Option | Default | Description |
|---|---|---|
| `--endpoint` | `http://localhost:5001` | ArmoniK control plane URL |
| `--partition` | `default` | Partition to submit tasks to |
| `--n` | `5` | Compute the sum `1 + 2 + … + n` |
| `--threshold` | `2` | Chunk size before splitting into sub-tasks |

### Worker

```bash
dotnet run --project Worker
```

The worker reads its gRPC endpoint from environment variables (standard ArmoniK worker configuration).

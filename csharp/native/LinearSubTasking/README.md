# Linear SubTasking

Shows linear sub-tasking: a task reduces its input by 2 and spawns a new sub-task until the value reaches 0 or 1, computing `n % 2` through a chain of sub-tasks.

## What it does

1. The client sends an integer `n` as the payload of a single task.
2. The worker adjusts the value by ±2 and, if the result is still not 0 or 1, submits a new sub-task with the adjusted value and the same result ID.
3. This repeats until a task reaches 0 or 1, at which point it writes the final result.
4. The client waits for the original result ID to be populated, verifies it equals `abs(n) % 2`, and prints it.

## Project structure

```
LinearSubTasking/
├── Client/
│   └── Program.cs          # Submits the initial task with integer input, validates result
└── Worker/
    ├── Program.cs           # Worker entry point
    └── LinearSubTasking.cs  # WorkerStreamWrapper with recursive sub-task submission
```

## Key concepts

- Reusing a result ID across a chain of sub-tasks: each sub-task in the chain inherits the original `ExpectedOutputKeys`, so the client always waits on the same result ID
- Linear sub-task chain with no fan-out
- Integer payload serialisation with `BitConverter`
- `taskHandler.SubmitTasksAsync` called from inside a worker

## Running

### Client

```bash
dotnet run --project Client -- \
  --endpoint http://localhost:5001 \
  --partition default \
  --integer 7
```

| Option | Default | Description |
|---|---|---|
| `--endpoint` | `http://localhost:5001` | ArmoniK control plane URL |
| `--partition` | `default` | Partition to submit tasks to |
| `--integer` | `3` | Integer for which `n % 2` is computed |

### Worker

```bash
dotnet run --project Worker
```

The worker reads its gRPC endpoint from environment variables (standard ArmoniK worker configuration).

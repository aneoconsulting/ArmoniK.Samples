# SubTasking

Shows sub-tasking with fan-out and fan-in: a parent task spawns multiple worker sub-tasks, then a joiner task aggregates their results.

## What it does

1. The client submits a single "Launch" task with `"Hello"` as the payload.
2. The Launch task creates 5 "HelloWorker" sub-tasks, each producing its own result, then creates a "Joiner" task that declares those 5 results as data dependencies.
3. Each HelloWorker task appends its own task ID to the payload and writes the result.
4. The Joiner task reads all 5 dependencies, appends `"_Joined"` to each, concatenates them line by line, and writes the final result.
5. The client waits for the final result and prints each line.

The use-case is communicated through a custom `TaskOptions.Options["UseCase"]` field (`"Launch"`, `"HelloWorker"`, `"Joiner"`).

## Project structure

```
SubTasking/
├── Client/
│   └── Program.cs          # Submits the initial Launch task and retrieves the final result
└── Worker/
    ├── Program.cs           # Worker entry point
    └── SubTaskingWorker.cs  # ProcessAsync dispatcher: Launch / HelloWorker / Joiner
```

## Key concepts

- Dispatching task behaviour at runtime via `TaskOptions.Options`
- Creating sub-task results with `taskHandler.CreateResultsMetaDataAsync`
- Submitting sub-tasks from within a worker with `taskHandler.SubmitTasksAsync`
- Expressing data dependencies between tasks using `DataDependencies` in `TaskCreation`
- Reading dependency data via `taskHandler.DataDependencies`

## Running

### Client

```bash
dotnet run --project Client -- \
  --endpoint http://localhost:5001 \
  --partition subtasking
```

| Option | Default | Description |
|---|---|---|
| `--endpoint` | `http://172.22.89.16:5001` | ArmoniK control plane URL |
| `--partition` | `subtasking` | Partition to submit tasks to |

### Worker

```bash
dotnet run --project Worker
```

The worker reads its gRPC endpoint from environment variables (standard ArmoniK worker configuration).

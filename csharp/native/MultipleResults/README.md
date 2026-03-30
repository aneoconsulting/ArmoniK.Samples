# Multiple Results

Shows how a single task can produce multiple results in ArmoniK.

## What it does

The client creates a session, pre-allocates N result slots (controlled by `--nResultsPerTask`), uploads an input string as the payload, submits one task that is expected to fill all N results, waits for all of them, then prints each one.

The worker iterates over every result ID it is expected to produce and writes `"<input> World_ <resultId>"` to each one.

## Project structure

```
MultipleResults/
├── Client/
│   └── Program.cs              # Session creation, multi-result task submission, result download loop
└── Worker/
    ├── Program.cs              # Worker entry point
    └── MultipleResultsWorker.cs # WorkerStreamWrapper iterating over ExpectedResults
```

## Key concepts

- Pre-allocating multiple result IDs before task submission with `CreateResultsMetaData`
- Passing multiple IDs in `ExpectedOutputKeys` when submitting a single task
- Worker iterating over `taskHandler.ExpectedResults` to produce N outputs
- Waiting for a list of results with `EventsClient.WaitForResultsAsync`

## Running

### Client

```bash
dotnet run --project Client -- \
  --endpoint http://localhost:5001 \
  --partition default \
  --input "Hello" \
  --nResultsPerTask 10
```

| Option | Default | Description |
|---|---|---|
| `--endpoint` | `http://localhost:5001` | ArmoniK control plane URL |
| `--partition` | `default` | Partition to submit tasks to |
| `--input` | `Hello` | Input string sent as payload |
| `--nResultsPerTask` | `10` | Number of results the single task must produce |

### Worker

```bash
dotnet run --project Worker
```

The worker reads its gRPC endpoint from environment variables (standard ArmoniK worker configuration).

# Hello World

A minimal end-to-end sample showing the basic task submission workflow with ArmoniK's native C# API.

## What it does

The client creates a session, uploads the string `"Hello"` as a payload, submits one task, waits for its result, then prints the returned string.

The worker reads the payload string and appends `" World_ <resultId>"` to it, then sends that as the task result.

## Project structure

```
HelloWorld/
├── Client/
│   └── Program.cs   # Session creation, task submission, result retrieval
└── Worker/
    ├── Program.cs           # Worker entry point (WorkerServer.Create<HelloWorldWorker>)
    └── HelloWorldWorker.cs  # WorkerStreamWrapper implementation
```

## Key concepts

- Creating a session with `SessionsClient`
- Uploading payload data inline with `ResultsClient.CreateResults`
- Reserving a result slot with `ResultsClient.CreateResultsMetaData`
- Submitting a task via `TasksClient.SubmitTasks`
- Waiting for results with `EventsClient.WaitForResultsAsync`
- Downloading result bytes with `ResultsClient.DownloadResultData`
- Implementing a worker by overriding `WorkerStreamWrapper.Process`

## Running

### Client

```bash
dotnet run --project Client -- \
  --endpoint http://localhost:5001 \
  --partition default
```

| Option | Default | Description |
|---|---|---|
| `--endpoint` | `http://localhost:5001` | ArmoniK control plane URL |
| `--partition` | `default` | Partition to submit tasks to |

### Worker

```bash
dotnet run --project Worker
```

The worker reads its gRPC endpoint from environment variables (standard ArmoniK worker configuration).

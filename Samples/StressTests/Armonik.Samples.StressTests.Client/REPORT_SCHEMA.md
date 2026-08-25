Report JSON schema and explanation

This document explains the JSON report produced by the StressTests client via `TasksStats.PrintToJson(jsonPath)`.

The report is a JSON object with these top-level fields:

- `generatedAt` (string, ISO 8601 UTC)
  - Timestamp when the report was written.

- `context` (object)
  - `nbTasks` (string|null): number of tasks that were submitted.
  - `nbInputBytes` (string|null): requested input payload size per task (bytes).
  - `nbOutputBytes` (string|null): requested output/result size per task (bytes).
  - `workloadTimeInMs` (string|null): workload simulated time per task (milliseconds).
  - `tasksPerBuffer` (string|null): submission buffering configuration - MaxTasksPerBuffer.
  - `nbChannel` (string|null): MaxParallelChannels.
  - `nbConcurrentBufferPerChannel` (string|null): MaxConcurrentBuffers.

- `kpis` (object)
  - Map of KPI key names to string values. Keys include:
    - `TEST`: The test name (usually "StressTest").
    - `COMPLETED_TASKS`: Number of tasks retrieved from control plane.
    - `TIME_SUBMITTED_TASKS`: Human readable time to submit all tasks.
    - `TIME_THROUGHPUT_SUBMISSION`: Submission throughput (tasks/s).
    - `TIME_THROUGHPUT_PROCESS`: Processing throughput (tasks/s).
    - `TIME_PROCESSED_TASKS`: Human readable time to process tasks.
    - `TIME_RETRIEVE_RESULTS`: Human readable time to retrieve results.
    - `TIME_THROUGHPUT_RESULTS`: Retrieval throughput (results/s).
    - `TOTAL_TIME`: End-to-end duration string.
    - `NB_TASKS`, `NB_INPUTBYTES`, `NB_OUTPUTBYTES`, `TIME_WORKLOAD_IN_MS`: repeat of context numeric values as strings.
    - `TASKS_PER_BUFFER`, `NB_CHANNEL`, `NB_CONCURRENT_BUFFER_PER_CHANNEL`: repeat of context.
    - `UPLOAD_SPEED_KB`, `DOWNLOAD_SPEED_KB`: throughput speeds in KB/s.
    - `NB_POD_USED`: number of distinct owner pod IDs observed.

- `tasks` (array)
  - Each element is a small summary of a task retrieved from the control plane. Fields include:
    - `taskId` (string): the task identifier.
    - `ownerPodId` (string|null): the id of the pod that executed the task.
    - `state` (string): task state according to the control plane Task.State enum.
    - `createdAt`, `startedAt`, `endedAt` (ISO 8601 strings or null): timestamps for task lifecycle events.
    - `resultSizeBytes` (number): size in bytes of the result payload attached to the task (0 if not present).
    - `errorMessage` (string|null): the textual error message if the task ended with an error.

Note: this client runs a single default stress test. The report `context` and `kpis` contain standard configuration and timing fields as described above.

Notes and interpretation guidance

- All numeric and configuration values are currently serialised as strings in `kpis` (preserves previous behaviour). The `context` block repeats important configuration values for easy programmatic access.

- Timestamps are ISO 8601 `'o'` format (UTC). To compute durations between events, parse them with timezone-aware parsing (they are UTC). Example: `DateTime.Parse("2025-10-02T12:34:56.789Z", null, DateTimeStyles.AdjustToUniversal)` in .NET.

- `resultSizeBytes` indicates how many bytes were returned by the worker for that task. If you used `outputVariation`, this can differ across tasks.

- If `tasks` array is large (many tasks) you can pipe the JSON through `jq` and extract aggregates, e.g.:

```bash
jq '.tasks | map(.resultSizeBytes) | add' report.json   # total result bytes
jq '.tasks | length' report.json                        # number of tasks returned by control plane
jq '.kpis.TIME_THROUGHPUT_PROCESS' report.json         # processing throughput
```

- Missing tasks detection: compare `context.nbTasks` to `kpis.COMPLETED_TASKS` and to the number of elements in `tasks` to find discrepancies. The client logs missing task IDs when they are detectable via callback bookkeeping.

Parameters

  - The following parameters are recorded as part of the test `context` and (where relevant) as `kpis`:
    - `submissionDelayMs` (number|null): delay in milliseconds applied between task submissions (used to throttle submission rate).
    - `payloadVariationPercent` (number|null): percentage of variation applied to input payload sizes around the mean (`nbInputBytes`).
    - `outputVariationPercent` (number|null): percentage of variation applied to output/result sizes around the mean (`nbOutputBytes`).
    - `variationDistribution` (string|null): distribution used for variation: `uniform`, `gaussian`, or `exponential`.
    - `grpcEndpoint` (string|null): the gRPC control-plane endpoint used for the run (comes from the environment variable `Grpc__Endpoint` or runner `--endpoint`).

Compatibility & extension

- The report format is intentionally simple (stringified KPIs + per-task small summaries). If you want richer per-task metadata (full proto dumps, arbitrary result contents), we can add optional full serialization (base64 for result bytes, full error details) behind a CLI flag like `--report-detail level`.


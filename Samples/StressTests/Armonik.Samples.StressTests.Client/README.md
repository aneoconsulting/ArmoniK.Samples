# ArmoniK Stress Test Client

This repository contains a stress test client for ArmoniK. The client runs a single default stress test which submits tasks with configurable payload, output sizes and simulated workload.

## Quick Start

### Prerequisites

- .NET 6.0+ SDK
- ArmoniK cluster running and accessible (local environment or on cloud)
- bash shell (for the convenience script)

### Running Tests

```bash
# Run the default stress test
./run-stress-tests.sh

# Custom run with options
./run-stress-tests.sh --tasks 2000 --workload-ms 50 --report ./reports/myreport.json
```

### Direct .NET Execution

```bash
# Build the project
dotnet build --configuration Release

# Run the stress test
dotnet run --configuration Release -- stressTest \
  --nbTask 2000 \
  --workLoadTimeInMs 100
```

## Options

Use `./run-stress-tests.sh --help` to see available options (task count, payload sizes, submission delay, report path, etc.).

## Report

The client writes a JSON report when invoked with `--report` (or `--jsonPath` forwarded by the runner). The JSON report contains a `kpis` map and a `tasks` array describing each returned task. See `REPORT_SCHEMA.md` for details.

## License

Part of the ArmoniK project under GNU Affero General Public License v3.0.
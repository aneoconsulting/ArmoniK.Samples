# ArmoniK Advanced Stress Test Suite

A comprehensive and modernized stress testing framework for ArmoniK, featuring multiple test scenarios, detailed reporting, and enhanced reliability.

## Features

### 🚀 **Two Testing Modes**

1. **Basic Stress Test** - Single scenario testing with customizable parameters
2. **Advanced Stress Test Suite** - Comprehensive multi-scenario testing with detailed reports

### 📊 **Test Scenarios (Advanced Mode)**

1. **High Volume Quick Tasks** - 5,000 tasks with 10ms workload (concurrency stress)
2. **Medium Volume Standard Tasks** - 2,000 tasks with 100ms workload (balanced test)
3. **Low Volume Heavy Tasks** - 500 tasks with 1,000ms workload (resource stress)
4. **Burst Load Test** - Multiple rapid submissions to test system resilience
5. **Endurance Test** - Long-running tasks to test system stability

### 📈 **Enhanced Reporting**

- **JSON Reports** - Machine-readable detailed metrics
- **HTML Reports** - Beautiful visual reports with charts and summaries
- **Individual Test Reports** - Per-scenario detailed analysis
- **Comprehensive Metrics** - Throughput, success rates, error analysis
- **Missing Task Detection** - Precise identification of lost tasks

### 🔧 **Technical Improvements**

- **Thread-Safe Operations** - Concurrent collections and atomic counters
- **Robust Error Handling** - Comprehensive exception management
- **Detailed Logging** - Structured, professional logging output
- **Performance Monitoring** - Real-time progress tracking with ETA
- **Resource Tracking** - Memory, CPU, and system information

## Quick Start

### Prerequisites

- .NET 6.0+ SDK
- ArmoniK cluster running and accessible
- bash shell (for the convenience script)

### Running Tests

#### Basic Usage

```bash
# Simple basic test
./run-stress-tests.sh basic

# Custom basic test
./run-stress-tests.sh basic --tasks 2000 --workload-ms 50

# Advanced comprehensive suite
./run-stress-tests.sh advanced
```

#### Advanced Usage

```bash
# Basic test with custom configuration
./run-stress-tests.sh \
  --partition my-partition \
  --channels 10 \
  basic \
  --tasks 5000 \
  --payload-kb 1024 \
  --workload-ms 10 \
  --json-report ./results.json

# Advanced test with custom ArmoniK settings
./run-stress-tests.sh \
  --partition production \
  --tasks-per-buffer 100 \
  --buffers-per-channel 10 \
  --channels 8 \
  advanced
```

### Direct .NET Execution

```bash
# Build the project
dotnet build --configuration Release

# Run basic test
dotnet run --configuration Release -- stressTest \
  --nbTask 2000 \
  --workLoadTimeInMs 100

# Run advanced test suite
dotnet run --configuration Release -- advancedTest \
  --partition my-partition
```

## Configuration Options

### Global Options
- `--partition` - ArmoniK partition name
- `--tasks-per-buffer` - Number of tasks per buffer (default: 50)
- `--buffers-per-channel` - Number of concurrent buffers per channel (default: 5)
- `--channels` - Number of gRPC channels (default: 5)

### Basic Test Options
- `--tasks` - Number of tasks to execute
- `--payload-kb` - Payload size in kilobytes
- `--workload-ms` - Task execution time in milliseconds
- `--json-report` - Path for JSON report output

## Report Output

### Advanced Test Reports

When running the advanced test suite, reports are automatically generated in:
```
./stress-test-reports/YYYY-MM-DD_HH-mm-ss/
├── comprehensive-report.html      # Visual HTML report
├── comprehensive-report.json      # Complete JSON data
├── test-HighVolumeQuick-xxxxx.json
├── test-MediumVolumeStandard-xxxxx.json
├── test-LowVolumeHeavy-xxxxx.json
├── test-BurstLoad-xxxxx.json
└── test-Endurance-xxxxx.json
```

### Report Contents

- **Executive Summary** - Overall success rates and performance metrics
- **Test Scenarios** - Individual scenario results and analysis
- **Performance Analysis** - Throughput, latency, and resource utilization
- **Error Analysis** - Detailed error reporting and missing task identification
- **Environment Information** - System specs and ArmoniK configuration
- **Visual Charts** - HTML report includes interactive performance charts

## Architecture

### Core Components

1. **StressTests.cs** - Original stress test with improvements
2. **AdvancedStressTests.cs** - Comprehensive test suite
3. **Result Handlers** - Thread-safe callback management
4. **Report Generation** - JSON and HTML report creation
5. **Progress Tracking** - Real-time monitoring and ETA calculation

### Key Improvements Over Original

1. **Thread Safety** - Fixed race conditions with concurrent collections
2. **Callback System** - Proper use of ArmoniK's callback mechanisms  
3. **Error Handling** - Comprehensive exception management
4. **Missing Task Detection** - Precise identification of lost tasks
5. **Professional Logging** - Structured, readable log output
6. **Comprehensive Reporting** - Multiple report formats with detailed metrics

## Performance Expectations

Based on testing with various ArmoniK configurations:

- **High Volume (5K tasks, 10ms)** - ~15-30 seconds execution
- **Medium Volume (2K tasks, 100ms)** - ~30-60 seconds execution  
- **Low Volume (500 tasks, 1000ms)** - ~60-120 seconds execution
- **Complete Suite** - ~10-30 minutes total (depending on cluster)

## Troubleshooting

### Common Issues

1. **Connection Errors** - Verify ArmoniK cluster accessibility
2. **Timeout Issues** - Check cluster resources and scaling
3. **Missing Tasks** - Review worker logs and resource constraints
4. **Performance Issues** - Analyze HTML report for bottlenecks

### Debug Mode

Enable detailed logging by setting environment variable:
```bash
export DOTNET_ENVIRONMENT=Development
./run-stress-tests.sh advanced
```

### Log Analysis

Check the structured logs for:
- Submission throughput issues
- Execution bottlenecks  
- Error patterns
- Resource constraints

## Contributing

This stress test suite is designed to be easily extensible:

1. **Add New Scenarios** - Extend `AdvancedStressTests.cs`
2. **Custom Metrics** - Modify report generation
3. **New Test Types** - Add commands to `Program.cs`
4. **Enhanced Reporting** - Extend HTML/JSON generation

## License

Part of the ArmoniK project under GNU Affero General Public License v3.0.
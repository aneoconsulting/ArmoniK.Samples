# ArmoniK Command Line Client

A proof of concept for running shell commands and scripts through ArmoniK distributed computing platform.

## Overview

This project demonstrates how to submit shell commands and scripts to ArmoniK for remote execution. It enables:

- Running bash commands and scripts on worker nodes
- Collecting and aggregating results
- Testing different command types in a distributed environment
- Generating detailed execution reports

## Prerequisites

- Python 3.7+
- ArmoniK cluster running and accessible
- Required Python packages:
  - `python-dotenv`
  - `requests` (for the client)
  - `ArmoniK` (both client and worker)

## Installation

1. Create virtual environment:
   ```
   python -m venv venv
   source venv/bin/activate  # On Windows use: venv\Scripts\activate
   ```

2. Install dependencies:
   ```
   pip install -r requirements.txt
   ```

3. Configure environment:
   Create a `.env` file in the client directory with the following variables:
   ```
   ARMONIK_ENDPOINT=<your-armonik-endpoint>  # Example: localhost:5001
   ARMONIK_PARTITION=<target-partition>      # Example: default
   ```

   Alternatively, configure ArmoniK using `client/armonik_config.sh`:
   ```
   ARMONIK_ENDPOINT=<your-armonik-endpoint>
   ARMONIK_PARTITION=cmdline  # Default partition for command line tests
   ```

## Project Structure

```
python/commandLine/
├── client/
│   ├── main.py                # Main client application
│   ├── test_client.py         # Test framework
│   ├── run_command_tests.sh   # Shell script test runner
│   ├── armonik_config.sh      # ArmoniK configuration
│   ├── test_commands/         # Directory for test command files
│   │   ├── data_processing.cmd
│   │   ├── file_operations.cmd
│   │   ├── multi_step.cmd
│   │   ├── network_diagnostics.cmd
│   │   └── system_info.cmd
│   ├── test_reports/          # Directory for test reports
│   └── test_outputs/          # Directory for test outputs
├── worker/
│   └── worker.py              # Worker implementation
└── README.md                  # This file
```

## Usage

Partition `cmdline` is used for command line tests. The client submits commands to the ArmoniK worker, which executes them and returns the results.

### Running the Client

To submit a shell command for execution:

```bash
python client/main.py --cmd "echo Hello from ArmoniK"
```

To submit a script file:

```bash
python client/main.py --file my_script.sh
```

### Command Options

```
--cmd TEXT          Shell command to execute
--file PATH         Path to a shell script file
--partition TEXT    Partition to use for execution
--timeout INTEGER   Maximum execution time in seconds
--output PATH       Path to save command output
--verbose           Enable verbose output
```

## Running Tests

### Using the Test Shell Script

The included shell script provides a convenient way to run all test commands:

```bash
cd client
./run_command_tests.sh
```

This script:
- Loads configuration from `armonik_config.sh`
- Ensures all test command files exist
- Executes all test commands sequentially
- Saves outputs to timestamped files
- Generates test reports and summaries

### Using the Python Test Framework

The test framework validates various command types with ArmoniK:

```bash
cd client
python test_client.py
```

Test reports are generated in the `test_reports` directory.

### Available Tests

- **System Information**: Retrieves basic system details from the worker
- **Network Diagnostics**: Tests connectivity and network configuration
- **File Operations**: Creates, modifies, and manages files on the worker
- **Data Processing**: Demonstrates basic data manipulation capabilities
- **Multi-step Process**: Chains multiple commands in a workflow

## Example Scripts

### Basic Example

```bash
# Save as example.sh
echo "=== Running on $(hostname) ==="
echo "Current time: $(date)"
echo "Working directory: $(pwd)"
echo "Hello from ArmoniK!"
```

Submit with:
```bash
python client/main.py --file example.sh
```

### Data Processing Example

```bash
# Save as process_data.sh
echo "Processing data..."
echo "Name,Value" > results.csv
for i in {1..10}; do
  echo "Item$i,$((RANDOM % 100))" >> results.csv
done
cat results.csv
echo "Processing complete!"
```

## Monitoring and Results

The test framework provides comprehensive reporting:

- **Test Outputs**: Individual command outputs are saved in the `test_outputs` directory with timestamps
- **Test Reports**: Full execution details are available in the `test_reports` directory
  - `test_report_[timestamp].txt`: Detailed execution logs
  - `test_summary_[timestamp].txt`: Execution summary with metrics

Example test summary:
```
=== Test Summary ===
Total tests: 5
Passed: 5
Failed: 0
Success rate: 100.00%
Total execution time: 2.27 seconds
```

## Contributing

1. Create your test command files in the `test_commands` directory
2. Add test methods in `test_client.py` for new command types
3. Run the test suite to validate your commands

## License

This project is licensed under the terms of the GNU Affero General Public License as published by the Free Software Foundation.

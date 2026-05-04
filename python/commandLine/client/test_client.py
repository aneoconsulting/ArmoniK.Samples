#!/usr/bin/env python3
# filepath: /home/mkgharbi/aneo/ArmoniK.Samples/python/commandLine/client/test_client.py
import json
import logging
import os
import subprocess
import time
import unittest
from datetime import datetime
from pathlib import Path

from dotenv import load_dotenv

# Configure logging for tests
logging.basicConfig(
    level=logging.INFO, format="%(asctime)s - %(levelname)s - %(message)s"
)
logger = logging.getLogger("TestClient")

# Define paths
BASE_DIR = Path(__file__).resolve().parent
CLIENT_PATH = BASE_DIR / "main.py"
TEST_DIR = BASE_DIR / "test_commands"
REPORTS_DIR = BASE_DIR / "test_reports"
OUTPUTS_DIR = BASE_DIR / "test_outputs"  # Default output folder
ENV_FILE = BASE_DIR / ".env"  # Path to the .env file

# Load environment variables from .env file
if ENV_FILE.exists():
    load_dotenv(dotenv_path=ENV_FILE)
else:
    logging.warning(f"{ENV_FILE} not found. Using default values.")

# Get endpoint and partition from the environment variables
ENDPOINT = os.getenv("ARMONIK_ENDPOINT", "localhost:5001")
PARTITION = os.getenv("ARMONIK_PARTITION", "default")

# Create directories if they don't exist
TEST_DIR.mkdir(exist_ok=True)
REPORTS_DIR.mkdir(exist_ok=True)
OUTPUTS_DIR.mkdir(exist_ok=True)


class CommandLineClientTest(unittest.TestCase):
    """Test suite for ArmoniK Command Line Client"""

    @classmethod
    def setUpClass(cls):
        """Create test command files once for the entire suite and prepare report"""
        logging.info("Setting up test files...")
        cls.test_results = []  # To store results for report
        cls.timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        cls.report_file = REPORTS_DIR / f"test_report_{cls.timestamp}.json"
        cls.create_test_files()

    @classmethod
    def tearDownClass(cls):
        """Generate a detailed report after all tests"""
        logging.info("Generating test report...")

        # Calculate summary statistics
        total_tests = len(cls.test_results)
        passed_tests = sum(1 for result in cls.test_results if result["passed"])
        failed_tests = total_tests - passed_tests
        success_rate = (passed_tests / total_tests * 100) if total_tests > 0 else 0
        total_duration = sum(result["duration"] for result in cls.test_results)

        # Create report data
        report_data = {
            "timestamp": cls.timestamp,
            "endpoint": ENDPOINT,
            "partition": PARTITION,
            "summary": {
                "total_tests": total_tests,
                "passed_tests": passed_tests,
                "failed_tests": failed_tests,
                "success_rate": success_rate,
                "total_duration": total_duration,
            },
            "test_results": cls.test_results,
        }

        # Save report as JSON
        with open(cls.report_file, "w") as f:
            json.dump(report_data, f, indent=2)

        # Generate human-readable report
        readable_report_file = REPORTS_DIR / f"test_report_{cls.timestamp}.txt"
        with open(readable_report_file, "w") as f:
            f.write("=== ArmoniK Command Line Client Test Report ===\n")
            f.write(f"Date: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write(f"Endpoint: {ENDPOINT}\n")
            f.write(f"Partition: {PARTITION}\n\n")

            f.write("--- Summary ---\n")
            f.write(f"Total Tests: {total_tests}\n")
            f.write(f"Passed: {passed_tests}\n")
            f.write(f"Failed: {failed_tests}\n")
            f.write(f"Success Rate: {success_rate:.2f}%\n")
            f.write(f"Total Duration: {total_duration:.3f}s\n\n")

            f.write("--- Test Details ---\n")
            for result in cls.test_results:
                status = "PASS" if result["passed"] else "FAIL"
                f.write(
                    f"Test: {result['name']} - {status} ({result['duration']:.3f}s)\n"
                )
                if "output_file" in result:
                    f.write(f"  Output: {result['output_file']}\n")
                if not result["passed"] and "error" in result:
                    f.write(f"  Error: {result['error']}\n")
                f.write("\n")

        logging.info(
            f"Test reports generated at {cls.report_file} and {readable_report_file}"
        )

    @classmethod
    def create_test_files(cls):
        """Create all test command files"""
        # 1. System Information
        system_info_content = """# System Information Commands
echo "=== System Information ==="
hostname
uname -a
cat /etc/os-release | grep PRETTY_NAME || echo "OS info not found"
echo "=== CPU Information ==="
lscpu | grep "Model name" || echo "CPU info not found"
echo "=== Memory Information ==="
free -h || echo "Memory info not found"
echo "=== Disk Usage ==="
df -h | grep -v tmpfs || echo "Disk info not found"
"""
        (TEST_DIR / "system_info.cmd").write_text(system_info_content)

        # 2. Network Diagnostics
        network_diagnostics_content = """# Network Diagnostics Commands
echo "=== Network Interfaces ==="
ip addr | grep -E "inet|ether" | grep -v "127.0.0.1" || echo "Network interfaces not found"
echo "=== Network Routes ==="
ip route || echo "Routes not found"
echo "=== DNS Configuration ==="
cat /etc/resolv.conf || echo "resolv.conf not found"
echo "=== Internet Connectivity ==="
ping -c 1 8.8.8.8 || echo "Ping failed"
echo "=== DNS Resolution ==="
nslookup google.com || echo "DNS resolution failed"
"""
        (TEST_DIR / "network_diagnostics.cmd").write_text(network_diagnostics_content)

        # 3. File Operations - Enhanced version
        file_operations_content = """#!/bin/bash
# File Operations Commands - Comprehensive test
# This script tests various file operations in ArmoniK

# Generate a unique test directory using current PID
TEST_DIR="/tmp/armonik_test_$$"
echo "=== Creating Test Environment ==="
echo "Creating test directory: $TEST_DIR"
mkdir -p "$TEST_DIR"
cd "$TEST_DIR"
echo "Current working directory: $(pwd)"
echo

# Creating various test files with different content
echo "=== Creating Test Files ==="
echo "Hello from ArmoniK! This is the first test file." > file1.txt
echo "This is the second test file with no special keywords." > file2.txt
echo "ArmoniK provides distributed computing capabilities." > file3.txt
echo "Line 1: Testing search capabilities" > search_file.txt
echo "Line 2: This line contains ArmoniK keyword" >> search_file.txt
echo "Line 3: This line doesn't contain any special keywords" >> search_file.txt
echo "Line 4: Another ArmoniK reference here" >> search_file.txt
echo "Line 5: Final line for testing" >> search_file.txt

# Create a small CSV file for data processing tests
cat > data.csv << EOF
Name,Department,Salary
John,Engineering,75000
Alice,Marketing,65000
Bob,Engineering,72000
Carol,HR,58000
David,Marketing,68000
EOF

# Create a binary file
dd if=/dev/urandom of=binary_file.bin bs=1024 count=10 2>/dev/null

# Create a symbolic link
ln -s file1.txt link_to_file1

# Create a subdirectory with more files
mkdir -p subdir/nested
echo "This is a file in a subdirectory" > subdir/subfile.txt
echo "This is a file in a nested subdirectory" > subdir/nested/nestedfile.txt

# Display created file structure
echo "=== File Structure Created ==="
echo "Total files created: $(find . -type f | wc -l)"
echo "Total directories created: $(find . -type d | wc -l)"
echo

# File contents display
echo "=== File Contents ==="
echo "----- file1.txt -----"
cat file1.txt
echo "----- file2.txt -----"
cat file2.txt
echo "----- file3.txt -----"
cat file3.txt
echo "----- search_file.txt -----"
cat search_file.txt
echo "----- data.csv -----"
cat data.csv
echo

# Basic file operations
echo "=== Basic File Operations ==="
echo "File sizes:"
ls -lh file*.txt data.csv binary_file.bin | awk '{print $9 ": " $5}'
echo

# Text processing
echo "=== Text Processing ==="
echo "Word count in all text files:"
wc -w file*.txt
echo
echo "Total word count: $(cat file*.txt | wc -w) words"
echo "Total line count: $(cat file*.txt | wc -l) lines"
echo "Total character count: $(cat file*.txt | wc -c) characters"
echo

# Search operations
echo "=== Search Operations ==="
echo "Files containing 'ArmoniK':"
grep -l "ArmoniK" file*.txt search_file.txt
echo
echo "Instances of 'ArmoniK' in all files:"
grep -n "ArmoniK" file*.txt search_file.txt
echo
echo "Count of 'ArmoniK' occurrences: $(grep -c "ArmoniK" file*.txt search_file.txt)"
echo

# File modification
echo "=== File Modification ==="
echo "Appending to file1.txt..."
echo "This line was appended for testing purposes." >> file1.txt
echo "New content of file1.txt:"
cat file1.txt
echo

# File permissions
echo "=== File Permissions ==="
echo "Current permissions:"
ls -la
echo
echo "Changing permissions on file3.txt..."
chmod 600 file3.txt
echo "New permissions for file3.txt:"
ls -la file3.txt
echo

# File transformation
echo "=== File Transformation ==="
echo "Converting data.csv to formatted table:"
column -t -s, data.csv
echo
echo "Extracting Engineering department from CSV:"
grep "Engineering" data.csv | column -t -s,
echo

# File comparison
echo "=== File Comparison ==="
cp file1.txt file1_copy.txt
echo "Comparing original and copy:"
diff file1.txt file1_copy.txt && echo "Files are identical"
echo "Modifying copy and comparing again:"
echo "This makes the copy different." >> file1_copy.txt
diff file1.txt file1_copy.txt || echo "Files are now different"
echo

# File archiving
echo "=== File Archiving ==="
echo "Creating archive of text files..."
tar -cf text_files.tar file*.txt
echo "Archive created: $(ls -lh text_files.tar | awk '{print $9 ": " $5}')"
echo

# Cleanup
echo "=== Cleaning Up ==="
cd /tmp
echo "Removing test directory: $TEST_DIR"
rm -rf "$TEST_DIR"
echo "Cleanup complete for process $$"
"""
        (TEST_DIR / "file_operations.cmd").write_text(file_operations_content)

        # 4. Data Processing
        data_processing_content = """# Data Processing Commands
echo "=== Generating Sample Data ==="
echo "Name,Age,City" > /tmp/sample_$$.csv
echo "John,35,New York" >> /tmp/sample_$$.csv
echo "Alice,29,London" >> /tmp/sample_$$.csv
echo "Bob,42,Paris" >> /tmp/sample_$$.csv
echo "Jane,31,Tokyo" >> /tmp/sample_$$.csv
echo "=== Basic Processing ==="
cat /tmp/sample_$$.csv
echo "=== Filtering with grep ==="
grep "Alice" /tmp/sample_$$.csv
echo "=== Counting with wc ==="
wc -l /tmp/sample_$$.csv
echo "=== Field extraction with cut ==="
cut -d, -f2,3 /tmp/sample_$$.csv
echo "=== Complex processing with awk ==="
awk -F, 'NR>1{print $1 " from " $3 " is " $2 " years old"}' /tmp/sample_$$.csv
echo "=== Cleaning Up ==="
rm /tmp/sample_$$.csv
echo "Cleanup complete for $$"
"""
        (TEST_DIR / "data_processing.cmd").write_text(data_processing_content)

        # 5. Multi-step Process
        multi_step_content = """# Multi-step Process Commands
echo "=== Environment Information ==="
echo "Current directory: $(pwd)"
echo "User: $(whoami)"
echo "Shell: $SHELL"
echo "PATH: $PATH"

echo "=== Date and Time Operations ==="
echo "Current date and time: $(date)"
cal || echo "Calendar command not available"

echo "=== Process Information ==="
echo "Current running processes for this user:"
ps aux | grep $(whoami) | head -5 || echo "ps command failed"

echo "=== Text Processing ==="
echo "Creating sample text file..."
cat > /tmp/sample_multi_$$.txt << 'EOF'
Line 1: This is a test file
Line 2: Created for ArmoniK testing
Line 3: It contains multiple lines
Line 4: To demonstrate text processing
Line 5: The End
EOF

echo "Displaying file content with line numbers:"
cat -n /tmp/sample_multi_$$.txt

echo "Searching for 'ArmoniK':"
grep "ArmoniK" /tmp/sample_multi_$$.txt

echo "Counting words:"
wc -w /tmp/sample_multi_$$.txt

echo "Cleaning up..."
rm /tmp/sample_multi_$$.txt
echo "Cleanup complete for $$"

echo "=== Multi-step process completed ==="
"""
        (TEST_DIR / "multi_step.cmd").write_text(multi_step_content)

    def run_test_with_reporting(self, test_name, command_file):
        """Run a test and record results for reporting"""
        start_time = time.time()
        test_output_file = OUTPUTS_DIR / f"{test_name}_{self.timestamp}.output"

        # First run directly with bash to test the command itself
        logging.info(f"Running test {test_name}...")
        result = subprocess.run(
            ["bash", str(command_file)],
            capture_output=True,
            text=True,
        )

        # Save output
        with open(test_output_file, "w") as f:
            f.write(result.stdout)
            if result.stderr:
                f.write("\n=== STDERR ===\n")
                f.write(result.stderr)

        # Calculate duration
        duration = time.time() - start_time

        # Prepare result data
        test_result = {
            "name": test_name,
            "command_file": str(command_file),
            "output_file": str(test_output_file),
            "passed": result.returncode == 0,
            "duration": duration,
            "return_code": result.returncode,
        }

        if result.returncode != 0:
            test_result["error"] = result.stderr if result.stderr else "Unknown error"

        # Store result for report
        self.__class__.test_results.append(test_result)

        # Log results
        status = "PASSED" if result.returncode == 0 else "FAILED"
        logging.info(f"Test {test_name} {status} in {duration:.3f}s")
        logging.info(f"Output saved to {test_output_file}")

        return result

    def test_system_info(self):
        """Test system information commands"""
        test_name = "system_info"
        command_file = TEST_DIR / f"{test_name}.cmd"
        result = self.run_test_with_reporting(test_name, command_file)
        self.assertEqual(result.returncode, 0)
        self.assertIn("System Information", result.stdout)

    def test_network_diagnostics(self):
        """Test network diagnostics commands"""
        test_name = "network_diagnostics"
        command_file = TEST_DIR / f"{test_name}.cmd"
        result = self.run_test_with_reporting(test_name, command_file)
        self.assertEqual(result.returncode, 0)
        self.assertIn("Network Interfaces", result.stdout)

    def test_file_operations(self):
        """Test file operations commands"""
        test_name = "file_operations"
        command_file = TEST_DIR / f"{test_name}.cmd"
        result = self.run_test_with_reporting(test_name, command_file)
        self.assertEqual(result.returncode, 0)
        self.assertIn("Creating Test Files", result.stdout)

    def test_data_processing(self):
        """Test data processing commands"""
        test_name = "data_processing"
        command_file = TEST_DIR / f"{test_name}.cmd"
        result = self.run_test_with_reporting(test_name, command_file)
        self.assertEqual(result.returncode, 0)
        self.assertIn("Generating Sample Data", result.stdout)

    def test_multi_step_process(self):
        """Test multi-step process commands"""
        test_name = "multi_step"
        command_file = TEST_DIR / f"{test_name}.cmd"
        result = self.run_test_with_reporting(test_name, command_file)
        self.assertEqual(result.returncode, 0)
        self.assertIn("Multi-step process completed", result.stdout)


if __name__ == "__main__":
    # Run tests
    unittest.main()

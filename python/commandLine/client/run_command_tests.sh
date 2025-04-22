#!/bin/bash

# Colors for formatting
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
YELLOW='\033[0;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Get the directory of this script
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CLIENT="${DIR}/main.py"
TEST_DIR="${DIR}/test_commands"
OUTPUT_DIR="${DIR}/test_outputs"

REPORT_DIR="${DIR}/test_reports"
CONFIG_FILE="${DIR}/armonik_config.sh"

# Load config file if exists
if [ -f "$CONFIG_FILE" ]; then
    echo -e "${BLUE}Loading configuration from ${CONFIG_FILE}${NC}"
    source "$CONFIG_FILE"
else
    echo -e "${RED}Configuration file ${CONFIG_FILE} not found. Using default values.${NC}"
fi

# Debug: Print loaded endpoint and partition
echo -e "${YELLOW}Loaded endpoint: ${ARMONIK_ENDPOINT}${NC}"
echo -e "${YELLOW}Loaded partition: ${ARMONIK_PARTITION}${NC}"

# Default values
VERBOSE=false
RUN_ALL=false
SPECIFIC_TESTS=()

# Show usage information
function show_usage {
    echo "Usage: $0 [options] [test_files]"
    echo ""
    echo "Options:"
    echo "  -e ENDPOINT   Specify ArmoniK endpoint"
    echo "  -p PARTITION  Specify ArmoniK partition"
    echo "  -v            Enable verbose mode"
    echo "  -a            Run all tests"
    echo "  -h            Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 -a                     # Run all tests"
    echo "  $0 system_info.cmd        # Run specific test"
    echo "  $0 -v -e localhost:5001   # Run with custom endpoint"
}

while getopts "e:p:vah" opt; do
  case $opt in
    e) armonik_endpoint="$OPTARG" ;;
    p) armonik_partition="$OPTARG" ;;
    v) VERBOSE=true ;;
    a) RUN_ALL=true ;;
    h) show_usage; exit 0 ;;
    *) show_usage; exit 1 ;;
  esac
done

shift $((OPTIND-1))
if [ $# -gt 0 ]; then
    for arg in "$@"; do
        [[ $arg != *.cmd ]] && arg="${arg}.cmd"
        SPECIFIC_TESTS+=("$arg")
    done
else
    RUN_ALL=true
fi

# Use the loaded or default values
export ARMONIK_ENDPOINT="${armonik_endpoint:-${ARMONIK_ENDPOINT:-localhost:5001}}"
export ARMONIK_PARTITION="${armonik_partition:-${ARMONIK_PARTITION:-default}}"

# Create necessary directories
mkdir -p "$OUTPUT_DIR" "$REPORT_DIR" "$TEST_DIR"

echo -e "${BLUE}=== ArmoniK Command Line Test Runner ===${NC}"
echo -e "${YELLOW}Using endpoint: ${ARMONIK_ENDPOINT}${NC}"
echo -e "${YELLOW}Using partition: ${ARMONIK_PARTITION}${NC}"

# Function to create test files
function create_test_files {
    echo -e "${BLUE}Ensuring test command files exist...${NC}"
    python3 -c "
from pathlib import Path
from test_client import CommandLineClientTest
CommandLineClientTest.create_test_files()
print('Test files created successfully.')
" || { echo -e "${RED}Failed to create test command files.${NC}"; exit 1; }
}

# Ensure test files exist
create_test_files

# Create test timestamp for reports/outputs
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
REPORT_FILE="${REPORT_DIR}/test_report_${TIMESTAMP}.txt"
SUMMARY_FILE="${REPORT_DIR}/test_summary_${TIMESTAMP}.txt"

# Initialize report header
cat > "$REPORT_FILE" << EOF
=== ArmoniK Command Line Test Report ===
Date: $(date)
Endpoint: ${ARMONIK_ENDPOINT}
Partition: ${ARMONIK_PARTITION}

EOF

# Initialize summary statistics
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0
TOTAL_TIME=0

# Run tests
echo -e "${BLUE}=== Running Tests ===${NC}"
if [ "$RUN_ALL" = true ]; then
    # Run all tests if no specific tests are provided
    SPECIFIC_TESTS=($(find "$TEST_DIR" -type f -name "*.cmd" | sort))
fi

# Print test list
echo -e "${BLUE}Tests to run (${#SPECIFIC_TESTS[@]} total):${NC}"
for test_file in "${SPECIFIC_TESTS[@]}"; do
    echo -e "  - $(basename "$test_file")"
done
echo ""

for test_file in "${SPECIFIC_TESTS[@]}"; do
    test_name=$(basename "$test_file")
    ((TOTAL_TESTS++))
    
    echo -e "${CYAN}Running test: ${test_name} (${TOTAL_TESTS}/${#SPECIFIC_TESTS[@]})${NC}"
    echo "=== Test: ${test_name} ===" >> "$REPORT_FILE"
    echo "Command file: ${test_file}" >> "$REPORT_FILE"
    
    # Record start time
    start_time=$(date +%s.%N)
    
    # Create output file path
    output_file="${OUTPUT_DIR}/${test_name%.cmd}_${TIMESTAMP}.output"
    
    # Run the command with our client and measure execution time
    if [ "$VERBOSE" = true ]; then
        echo -e "${YELLOW}Running with verbose output${NC}"
        python3 "$CLIENT" --file "$test_file" --endpoint "$ARMONIK_ENDPOINT" \
                         --partition "$ARMONIK_PARTITION" --output "$output_file" --verbose
        exit_code=$?
    else
        python3 "$CLIENT" --file "$test_file" --endpoint "$ARMONIK_ENDPOINT" \
                         --partition "$ARMONIK_PARTITION" --output "$output_file"
        exit_code=$?
    fi
    
    # Calculate duration
    end_time=$(date +%s.%N)
    duration=$(echo "$end_time - $start_time" | bc)
    TOTAL_TIME=$(echo "$TOTAL_TIME + $duration" | bc)
    
    # Check result and update report
    if [ $exit_code -eq 0 ]; then
        echo -e "${GREEN}✓ Test ${test_name} passed (${duration} seconds)${NC}"
        echo "Result: PASS" >> "$REPORT_FILE"
        ((PASSED_TESTS++))
    else
        echo -e "${RED}✗ Test ${test_name} failed (${duration} seconds)${NC}"
        echo "Result: FAIL" >> "$REPORT_FILE"
        ((FAILED_TESTS++))
    fi
    
    echo "Duration: ${duration} seconds" >> "$REPORT_FILE"
    echo "Output file: ${output_file}" >> "$REPORT_FILE"
    
    # Add output summary to report if file exists
    if [ -f "$output_file" ]; then
        output_size=$(wc -l < "$output_file")
        echo "Output size: ${output_size} lines" >> "$REPORT_FILE"
        
        echo "" >> "$REPORT_FILE"
        echo "Output Preview (first 10 lines):" >> "$REPORT_FILE"
        head -n 10 "$output_file" >> "$REPORT_FILE"
        
        if [ "$output_size" -gt 10 ]; then
            echo "..." >> "$REPORT_FILE"
            echo "(${output_size} total lines, see output file for complete output)" >> "$REPORT_FILE"
        fi
    else
        echo "No output file generated" >> "$REPORT_FILE"
    fi
    
    echo "" >> "$REPORT_FILE"
    echo "-----------------------------------------" >> "$REPORT_FILE"
    echo "" >> "$REPORT_FILE"
done

# Calculate success rate
if [ "$TOTAL_TESTS" -gt 0 ]; then
    SUCCESS_RATE=$(echo "scale=2; $PASSED_TESTS * 100 / $TOTAL_TESTS" | bc)
else
    SUCCESS_RATE="N/A"
fi

# Generate summary report
cat > "$SUMMARY_FILE" << EOF
=== ArmoniK Command Line Test Summary ===
Date: $(date)
Endpoint: ${ARMONIK_ENDPOINT}
Partition: ${ARMONIK_PARTITION}

Total tests: ${TOTAL_TESTS}
Passed: ${PASSED_TESTS}
Failed: ${FAILED_TESTS}
Success rate: ${SUCCESS_RATE}%
Total execution time: ${TOTAL_TIME} seconds

Test output directory: ${OUTPUT_DIR}
Full report file: ${REPORT_FILE}
EOF

# Append summary to main report
echo "" >> "$REPORT_FILE"
echo "=== Test Summary ===" >> "$REPORT_FILE"
echo "Total tests: ${TOTAL_TESTS}" >> "$REPORT_FILE"
echo "Passed: ${PASSED_TESTS}" >> "$REPORT_FILE"
echo "Failed: ${FAILED_TESTS}" >> "$REPORT_FILE"
echo "Success rate: ${SUCCESS_RATE}%" >> "$REPORT_FILE"
echo "Total execution time: ${TOTAL_TIME} seconds" >> "$REPORT_FILE"

# Display summary
echo ""
echo -e "${BLUE}=== Test Summary ===${NC}"
echo -e "Total tests: ${CYAN}${TOTAL_TESTS}${NC}"
echo -e "Passed: ${GREEN}${PASSED_TESTS}${NC}"
echo -e "Failed: ${RED}${FAILED_TESTS}${NC}"
echo -e "Success rate: ${CYAN}${SUCCESS_RATE}%${NC}"
echo -e "Total execution time: ${CYAN}${TOTAL_TIME}${NC} seconds"

echo -e "${BLUE}=== All Tests Completed ===${NC}"
echo -e "Full report saved to: ${YELLOW}${REPORT_FILE}${NC}"
echo -e "Summary report saved to: ${YELLOW}${SUMMARY_FILE}${NC}"
echo -e "Test outputs saved to: ${YELLOW}${OUTPUT_DIR}${NC}"
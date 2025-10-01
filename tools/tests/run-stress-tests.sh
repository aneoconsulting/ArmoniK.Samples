#!/bin/bash

# Advanced ArmoniK Stress Test Runner
# This script provides easy access to both basic and comprehensive stress tests

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
PURPLE='\033[0;35m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color
#TODO ENLEVER LE TEST ENDURANCE
# Default values
# Default partition to use when --partition is not provided
PARTITION="default"
NB_TASK_PER_BUFFER=50
NB_BUFFER_PER_CHANNEL=5
NB_CHANNEL=5
ENDPOINT=""
OUTPUT_JSON=""

# Function to print colored output
print_info() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

print_header() {
    echo -e "${PURPLE}"
    echo "================================================================================"
    echo "                      ARMONIK ADVANCED STRESS TEST RUNNER"
    echo "================================================================================"
    echo -e "${NC}"
}



# Function to show usage
show_usage() {
    echo "Usage: $0 [OPTIONS] <COMMAND>"
    echo ""
    echo "Commands:"
    echo "  basic          Run basic stress test (single scenario)"
    echo "  advanced       Run comprehensive stress test suite"
    echo "  help           Show this help message"
    echo ""
    echo "Options:"
    echo "  --partition <name>           Partition name (default: empty)"
    echo "  --tasks-per-buffer <num>     Tasks per buffer (default: 50)"
    echo "  --buffers-per-channel <num>  Buffers per channel (default: 5)"
    echo "  --channels <num>             Number of channels (default: 5)"
    echo "  --endpoint <url>             ArmoniK control plane endpoint (default: http://35.187.116.120:5001)"
    echo ""
    echo "Basic test additional options:"
    echo "  --tasks <num>                Number of tasks (default: 1000)"
    echo "  --payload-kb <num>           Payload size in KB (default: 512)"
    echo "  --output-kb <num>            Output size in KB (default: 8)"
    echo "  --workload-ms <num>          Workload time in ms (default: 100)"
    echo "  --tasks-per-buffer <num>     Tasks per buffer for basic test (default: 50)"
    echo "  --buffers-per-channel <num>  Buffers per channel for basic test (default: 5)"
    echo "  --channels <num>             Channels for basic test (default: 5)"
    echo "  --submission-delay-ms <num>  Delay in ms between task submissions (default: 0 = no delay)"
    echo "  --payload-variation <0-100>  Payload size variation in percent (default: 0 = fixed size)"
    echo "  --output-variation <0-100>   Output size variation in percent (default: 0 = fixed size)"
    echo "  --variation-distribution <type> Distribution type: uniform, gaussian, exponential (default: uniform)"
    echo "  --report <path>              Report output path"
    echo ""
    echo "Examples:"
    echo "  $0 basic --tasks 2000 --workload-ms 50"
    echo "  $0 --endpoint http://35.187.116.120:5001 basic --tasks 1000"
    echo "  $0 basic --tasks 5000 --payload-kb 1024 --workload-ms 10 --report ./report.json"
    echo "  $0 basic --tasks 1000 --submission-delay-ms 100 --report ./throttled.json"
    echo "  $0 basic --tasks 2000 --payload-kb 512 --payload-variation 30 --output-variation 20"
    echo "  $0 basic --tasks 1000 --payload-variation 50 --variation-distribution gaussian"
}

# Function to build the project
build_project() {
    print_info "Building stress test project..."
    
    if dotnet build --configuration Release > /dev/null 2>&1; then
        print_success "Project built successfully"
    else
        print_error "Failed to build project"
        exit 1
    fi
}
# Function to check prerequisites
check_prerequisites() {
    print_info "Checking prerequisites..."
    
    # Check if dotnet is available
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed or not in PATH"
        exit 1
    fi
    
    # Find the stress test client directory using find command
    local stress_test_dir=""
    local project_file="Armonik.Samples.StressTests.Client.csproj"
    
    # First check if we're already in the right directory
    if [[ -f "$project_file" ]]; then
        stress_test_dir="."
    else
        # Search for the project file starting from current directory and going up
        local search_paths=(
            "."
            ".."
            "../.."
            "../../.."
        )
        
        for base_path in "${search_paths[@]}"; do
            if [[ -d "$base_path" ]]; then
                local found_path=$(find "$base_path" -name "$project_file" -type f 2>/dev/null | head -n1)
                if [[ -n "$found_path" ]]; then
                    stress_test_dir="$(dirname "$found_path")"
                    break
                fi
            fi
        done
    fi
    
    if [[ -z "$stress_test_dir" ]]; then
        print_error "Cannot find stress test client directory ($project_file)"
        print_error "Please make sure you're in the ArmoniK.Samples repository"
        exit 1
    fi
    
    # Change to the stress test directory
    print_info "Found stress test directory: ${stress_test_dir}"
    cd "${stress_test_dir}"
    
    print_success "Prerequisites check passed"
}

# Function to set up endpoint
setup_endpoint() {
    local default_endpoint="http://35.187.116.120:5001"
    
    # Use provided endpoint or default
    if [[ -n "${ENDPOINT:-}" ]]; then
        export Grpc__Endpoint="$ENDPOINT"
        print_info "Using provided endpoint: $ENDPOINT"
    else
        export Grpc__Endpoint="$default_endpoint"
        print_info "Using default endpoint: $default_endpoint"
    fi
}

# Function to run basic stress test
run_basic_test() {
    # Local variables for this test run
    local tasks=1000
    local payload_kb=512
    local output_kb=8
    local workload_ms=100
    local tasks_per_buffer=50
    local buffers_per_channel=5
    local channels=5
    local submission_delay_ms=0
    local payload_variation=0
    local output_variation=0
    local variation_distribution="uniform"
    local json_report=""
    
    # Parse command-specific arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            --tasks)
                tasks="$2"
                shift 2
                ;;
            --payload-kb)
                payload_kb="$2"
                shift 2
                ;;
            --output-kb)
                output_kb="$2"
                shift 2
                ;;
            --workload-ms)
                workload_ms="$2"
                shift 2
                ;;
            --tasks-per-buffer)
                tasks_per_buffer="$2"
                shift 2
                ;;
            --buffers-per-channel)
                buffers_per_channel="$2"
                shift 2
                ;;
            --channels)
                channels="$2"
                shift 2
                ;;
            --submission-delay-ms)
                submission_delay_ms="$2"
                shift 2
                ;;
            --payload-variation)
                payload_variation="$2"
                shift 2
                ;;
            --output-variation)
                output_variation="$2"
                shift 2
                ;;
            --variation-distribution)
                variation_distribution="$2"
                shift 2
                ;;
            --report)
                # If next arg is missing or looks like an option, use default path
                if [[ $# -lt 2 || "$2" == --* ]]; then
                    mkdir -p ./reports
                    json_report="./reports/basic-$(date +%Y%m%d%H%M%S).json"
                else
                    json_report="$2"
                    shift
                fi
                shift
                ;;
            *)
                print_error "Unknown basic test option: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    print_info "Running basic stress test..."
    print_info "Configuration:"
    print_info "  Tasks: ${tasks}"
    print_info "  Payload size: ${payload_kb} KB"
    print_info "  Output size: ${output_kb} KB"
    print_info "  Workload duration: ${workload_ms} ms"
    print_info "  Tasks per buffer: ${tasks_per_buffer}"
    print_info "  Buffers per channel: ${buffers_per_channel}"
    print_info "  Channels: ${channels}"
    print_info "  Submission delay: ${submission_delay_ms} ms"
    print_info "  Payload variation: ${payload_variation}%"
    print_info "  Output variation: ${output_variation}%"
    print_info "  Variation distribution: ${variation_distribution}"
    if [[ -n "$PARTITION" ]]; then
        print_info "  Partition: ${PARTITION}"
    fi
    print_info ""
    
    # Setup endpoint
    setup_endpoint
    
    # Build test command
    test_cmd=(dotnet run -c Release --)
    test_cmd+=(stressTest)
    test_cmd+=(--nbTask "$tasks")
    test_cmd+=(--nbInputBytes "$((payload_kb * 1024))")
    test_cmd+=(--nbOutputBytes "$((output_kb * 1024))")
    test_cmd+=(--workLoadTimeInMs "$workload_ms")
    test_cmd+=(--nbTaskPerBuffer "$tasks_per_buffer")
    test_cmd+=(--nbBufferPerChannel "$buffers_per_channel")
    test_cmd+=(--nbChannel "$channels")
    test_cmd+=(--submissionDelayMs "$submission_delay_ms")
    test_cmd+=(--payloadVariation "$payload_variation")
    test_cmd+=(--outputVariation "$output_variation")
    test_cmd+=(--variationDistribution "$variation_distribution")

    # ajouter partition et json_path si nécessaire
    if [[ -n "${PARTITION:-}" ]]; then
      test_cmd+=(--partition "$PARTITION")
    fi
    if [[ -n "${json_report:-}" ]]; then
      test_cmd+=(--jsonPath "$json_report")
    fi

    print_info "Executing: $test_cmd"
    print_info "Using endpoint: $Grpc__Endpoint"
    print_info ""
    
    # Execute test
    "${test_cmd[@]}"
    local exit_code=$?
    
    if [[ $exit_code -eq 0 ]]; then
        print_success "Basic stress test completed successfully!"
        if [[ -n "${json_report}" ]]; then
            print_info "Report saved to: $json_report"
        fi
    else
        print_error "Basic stress test failed with exit code: $exit_code"
    fi
    
    return $exit_code
}

# Function to run advanced stress test
run_advanced_test() {
    # default values for advanced test
    local tasks=5000
    local payload_kb=512
    local output_kb=8
    local workload_ms=10
    local tasks_per_buffer=${NB_TASK_PER_BUFFER}
    local buffers_per_channel=${NB_BUFFER_PER_CHANNEL}
    local channels=${NB_CHANNEL}
    local json_report=""

    # Advanced test does not accept per-scenario CLI options in this runner
    # The advanced suite uses its internal scenario definitions and defaults.
    if [[ $# -gt 0 ]]; then
        print_warning "Advanced test ignores per-scenario options; using internal defaults. Provided args will be ignored."
    fi

    print_info "Running comprehensive stress test (advanced) ..."
    print_info "Configuration:"
    print_info "  Tasks: ${tasks}"
    print_info "  Payload size: ${payload_kb} KB"
    print_info "  Output size: ${output_kb} KB"
    print_info "  Workload duration: ${workload_ms} ms"
    print_info "  Tasks per buffer: ${tasks_per_buffer}"
    print_info "  Buffers per channel: ${buffers_per_channel}"
    print_info "  Channels: ${channels}"
    if [[ -n "$PARTITION" ]]; then
        print_info "  Partition: ${PARTITION}"
    fi
    if [[ -n "$json_report" ]]; then
        print_info "  Report: ${json_report}"
    fi
    print_info ""

    # ensure endpoint is set
    setup_endpoint

    # build advanced test command
    # Note: the advancedTest subcommand in the client only accepts
    # --partition, --nbTaskPerBuffer, --nbBufferPerChannel and --nbChannel.
    # We purposely do NOT forward per-scenario options like --nbTask,
    # --nbInputBytes, --nbOutputBytes or --workLoadTimeInMs because the
    # advanced suite uses internal scenario definitions.
    local cmd="dotnet run -c Release -- advancedTest"
    cmd="${cmd} --nbTaskPerBuffer ${tasks_per_buffer}"
    cmd="${cmd} --nbBufferPerChannel ${buffers_per_channel}"
    cmd="${cmd} --nbChannel ${channels}"

    if [[ -n "${PARTITION:-}" ]]; then
        cmd="${cmd} --partition ${PARTITION}"
    fi

    if [[ -n "${json_report}" ]]; then
        mkdir -p "$(dirname "$json_report")"
        cmd="${cmd} --jsonPath ${json_report}"
    fi

    print_info "Executing: $cmd"
    print_info "Using endpoint: $Grpc__Endpoint"
    print_info ""

    # execute advanced test
    eval $cmd
    local exit_code=${PIPESTATUS[0]}

    if [[ $exit_code -eq 0 ]]; then
        print_success "Advanced stress test completed successfully!"
        if [[ -n "${json_report}" ]]; then
            print_info "Report saved to: $json_report"
        fi
    else
        print_error "Advanced stress test failed with exit code: $exit_code"
    fi

    return $exit_code
}

# Main script
main() {
    print_header
    
    # Store all arguments for proper forwarding
    local original_args=("$@")
    local command=""
    local command_args=()
    local command_found=false
    
    # Parse arguments to find command and separate global from command-specific options
    for arg in "${original_args[@]}"; do
        if [[ "$arg" == "basic" || "$arg" == "advanced" || "$arg" == "help" || "$arg" == "--help" || "$arg" == "-h" ]]; then
            command="$arg"
            command_found=true
            continue
        fi
        
        if [[ "$command_found" == true ]]; then
            command_args+=("$arg")
        fi
    done
    
    # Parse global options
    while [[ $# -gt 0 ]]; do
        case $1 in
            --partition)
                PARTITION="$2"
                shift 2
                ;;
            --tasks-per-buffer)
                NB_TASK_PER_BUFFER="$2"
                shift 2
                ;;
            --buffers-per-channel)
                NB_BUFFER_PER_CHANNEL="$2"
                shift 2
                ;;
            --channels)
                NB_CHANNEL="$2"
                shift 2
                ;;
            --endpoint)
                ENDPOINT="$2"
                shift 2
                ;;
            basic|advanced|help|--help|-h)
                # Skip command and remaining args - they're handled separately
                break
                ;;
            *)
                print_error "Unknown global option: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Execute command with proper arguments
    case "$command" in
        basic)
            check_prerequisites
            build_project
            run_basic_test "${command_args[@]}"
            ;;
        advanced)
            check_prerequisites
            build_project
            run_advanced_test "${command_args[@]}"
            ;;
        help|--help|-h)
            show_usage
            ;;
        "")
            # No command provided - run basic test with default parameters
            print_info "No command specified, running basic test with default parameters"
            check_prerequisites
            build_project
            run_basic_test
            ;;
        *)
            print_error "Unknown command: $command"
            show_usage
            exit 1
            ;;
    esac
}

# Run main function with all arguments
main "$@"
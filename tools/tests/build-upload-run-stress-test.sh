#!/usr/bin/env bash
set -euo pipefail

# Build the worker, create a zip, upload it to GCS, then run the stress test runner.
# Usage examples:
#  ./build-upload-run-stress-test.sh --tasks 1000
#  ./build-upload-run-stress-test.sh -b nicodl-aneo-gcsfs --tasks 1000
#  ./build-upload-run-stress-test.sh --bucket gs://nicodl-aneo-gcsfs --tasks 1000

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)

DEFAULT_PACKAGE_NAME="Armonik.Samples.StressTests.Worker-v1.0.0-700.zip"
# TODO Change the default bucket if needed for the time being
DEFAULT_BUCKET="nico-aneo-gcsfs"

usage() {
  cat <<EOF
Usage: $(basename "$0") [options] <stress-runner-command> [runner-args...]

Options:
  -b|--bucket BUCKET        GCS bucket name or gs:// URL (default: gs://${DEFAULT_BUCKET})
  -o|--object OBJECT        Object name to use in the bucket (default: ${DEFAULT_PACKAGE_NAME})
  --package-name NAME       Local package filename to produce (defaults to ${DEFAULT_PACKAGE_NAME})
  --no-restart-compute      Do not restart compute-plane pods after upload (default: will not restart)
  --public                  Make uploaded object public (passes --public to upload script)
  -h|--help                 Show this help

Example:
  ${SCRIPT_DIR}/build-upload-run-stress-test.sh --tasks 1000

This script will:
  - dotnet publish the worker to a temporary folder
  - create a zip archive under Samples/StressTests/packages/
  - call tools/tests/upload-to-gcs.sh to upload the zip
  - call tools/tests/run-stress-tests.sh (with any provided args)
EOF
  exit 1
}

BUCKET=""
OBJECT=""
PACKAGE_NAME="${DEFAULT_PACKAGE_NAME}"
RESTART_COMPUTE=false
PUBLIC=false

OPTS=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    -b|--bucket)
      BUCKET="$2"; shift 2;;
    --bucket=*)
      BUCKET="${1#*=}"; shift;;
    --endpoint)
      ENDPOINT="$2"; shift 2;;
    --endpoint=*)
      ENDPOINT="${1#*=}"; shift;;
    -o|--object)
      OBJECT="$2"; shift 2;;
    --object=*)
      OBJECT="${1#*=}"; shift;;
    --package-name)
      PACKAGE_NAME="$2"; shift 2;;
    --package-name=*)
      PACKAGE_NAME="${1#*=}"; shift;;
    --no-restart-compute)
      RESTART_COMPUTE=false; shift;;
    --restart-compute)
      RESTART_COMPUTE=true; shift;;
    --public)
      PUBLIC=true; shift;;
    -h|--help)
      usage;;
    --)
      shift; break;;
    -* )
      # unknown option for this wrapper: stop parsing and let remaining args be runner args
      break;;
    *)
      # first non-option is the runner command 
      break;;
  esac
done

if [[ -z "$BUCKET" ]]; then
  BUCKET="$DEFAULT_BUCKET"
  echo "Using default bucket: $BUCKET"
fi

# Remaining arguments are the runner command and its args
RUNNER_ARGS=("$@")
if [[ -n "${ENDPOINT:-}" ]]; then
  # Prepend endpoint so run-stress-tests.sh sees it before the command
  RUNNER_ARGS=( --endpoint "${ENDPOINT}" "${RUNNER_ARGS[@]}" )
fi
# Runner args are optional: if none provided, the runner will run the default stress test

WORKER_DIR="$REPO_ROOT/Samples/StressTests/Armonik.Samples.StressTests.Worker"
PACKAGES_DIR="$REPO_ROOT/Samples/StressTests/packages"
mkdir -p "$PACKAGES_DIR"

PACKAGE_PATH="$PACKAGES_DIR/$PACKAGE_NAME"

# Build the client (runner) to ensure the end-to-end flow builds both client and worker
CLIENT_DIR="$REPO_ROOT/Samples/StressTests/Armonik.Samples.StressTests.Client"
CLIENT_PROJECT_FILE="$CLIENT_DIR/Armonik.Samples.StressTests.Client.csproj"
if [[ -f "$CLIENT_PROJECT_FILE" ]]; then
  echo "Building client project: $CLIENT_PROJECT_FILE"
  if ! command -v dotnet >/dev/null 2>&1; then
    echo "dotnet not found in PATH; cannot build client" >&2
    exit 6
  fi
  dotnet build "$CLIENT_PROJECT_FILE" -c Release || {
    echo "Client build failed" >&2
    exit 7
  }
else
  echo "Client project not found at $CLIENT_PROJECT_FILE; skipping client build"
fi

# Delegate build+package to build-worker.sh and capture created package path
BUILD_SCRIPT="$SCRIPT_DIR/build-worker.sh"
if [[ ! -x "$BUILD_SCRIPT" && -f "$BUILD_SCRIPT" ]]; then
  chmod +x "$BUILD_SCRIPT" || true
fi
if [[ ! -f "$BUILD_SCRIPT" ]]; then
  echo "build-worker.sh not found at $BUILD_SCRIPT" >&2
  exit 4
fi

PACKAGE_PATH="$("$BUILD_SCRIPT" --worker-dir "$WORKER_DIR" --package-name "$PACKAGE_NAME" --packages-dir "$PACKAGES_DIR")"
if [[ ! -f "$PACKAGE_PATH" ]]; then
  echo "Package not created: $PACKAGE_PATH" >&2
  exit 5
fi

echo "Zip created: $PACKAGE_PATH"

# Build upload command
UPLOAD_SCRIPT="$SCRIPT_DIR/upload-to-gcs.sh"
if [[ ! -x "$UPLOAD_SCRIPT" && -f "$UPLOAD_SCRIPT" ]]; then
  # make executable if not
  chmod +x "$UPLOAD_SCRIPT" || true
fi
if [[ ! -f "$UPLOAD_SCRIPT" ]]; then
  echo "upload-to-gcs.sh not found at $UPLOAD_SCRIPT" >&2
  exit 4
fi

UPLOAD_CMD=("$UPLOAD_SCRIPT" -f "$PACKAGE_PATH")
if [[ -n "$BUCKET" ]]; then
  # Normalize bucket name: strip gs:// prefix if present
  NORMALIZED_BUCKET="$BUCKET"
  if [[ "$BUCKET" =~ ^gs://(.+)$ ]]; then
    NORMALIZED_BUCKET="${BASH_REMATCH[1]}"
  fi
  UPLOAD_CMD+=( -b "$NORMALIZED_BUCKET" )
fi
if [[ -n "$OBJECT" ]]; then
  UPLOAD_CMD+=( -o "$OBJECT" )
fi
if $PUBLIC; then
  UPLOAD_CMD+=( --public )
fi

echo "Uploading package to bucket..."
set -x
"${UPLOAD_CMD[@]}"
set +x

echo "Starting stress test runner with: ${RUNNER_ARGS[*]}"
RUNNER_SCRIPT="$SCRIPT_DIR/run-stress-tests.sh"

"$RUNNER_SCRIPT" "${RUNNER_ARGS[@]}"

exit 0

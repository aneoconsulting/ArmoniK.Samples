#!/usr/bin/env bash
set -euo pipefail

# Build the worker, create a zip, upload it to GCS, then run the stress test runner.
# Usage examples:
#  ./build-upload-run-stress-test.sh basic --tasks 1000
#  ./build-upload-run-stress-test.sh -b nicodl-aneo-gcsfs basic --tasks 1000
#  ./build-upload-run-stress-test.sh --bucket gs://nicodl-aneo-gcsfs basic --tasks 1000

SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)

DEFAULT_PACKAGE_NAME="Armonik.Samples.StressTests.Worker-v1.0.0-700.zip"
DEFAULT_BUCKET="nicodl-aneo-gcsfs"

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
  ${SCRIPT_DIR}/build-upload-run-stress-test.sh basic --tasks 1000

This script will:
  - dotnet publish the worker to a temporary folder
  - create a zip archive under Samples/StressTests/packages/
  - call tools/tests/upload-to-gcs.sh to upload the zip
  - optionally restart compute-plane pods (disabled by default)
  - call tools/tests/run-stress-tests.sh with the provided command and args
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
      # first non-option is the runner command (e.g. basic or advanced)
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
if [[ ${#RUNNER_ARGS[@]} -eq 0 ]]; then
  echo "Error: missing stress runner command (e.g. basic) and its args" >&2
  usage
fi

WORKER_DIR="$REPO_ROOT/Samples/StressTests/Armonik.Samples.StressTests.Worker"
PACKAGES_DIR="$REPO_ROOT/Samples/StressTests/packages"
mkdir -p "$PACKAGES_DIR"

PACKAGE_PATH="$PACKAGES_DIR/$PACKAGE_NAME"

echo "Building worker from: $WORKER_DIR"
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found in PATH" >&2
  exit 2
fi

TMP_PUBLISH_DIR=$(mktemp -d)
STRUCTURED_DIR=$(mktemp -d)
trap 'rm -rf "$TMP_PUBLISH_DIR" "$STRUCTURED_DIR"' EXIT

echo "Publishing to temporary folder: $TMP_PUBLISH_DIR"
dotnet publish "$WORKER_DIR" -c Release -r linux-x64 -f net8.0 -o "$TMP_PUBLISH_DIR"

# Create the expected directory structure for the ZIP
EXPECTED_PATH="$STRUCTURED_DIR/Armonik.Samples.StressTests.Worker/1.0.0-700"
mkdir -p "$EXPECTED_PATH"
cp -r "$TMP_PUBLISH_DIR"/* "$EXPECTED_PATH/"

echo "Packaging published output into zip: $PACKAGE_PATH"
if command -v zip >/dev/null 2>&1; then
  # create zip (overwrite if exists)
  rm -f "$PACKAGE_PATH"
  (cd "$STRUCTURED_DIR" && zip -r "$PACKAGE_PATH" .) >/dev/null
else
  # fallback to python zip
  if command -v python3 >/dev/null 2>&1; then
    python3 - <<PY
import sys, zipfile, os
out = os.path.abspath(r"$PACKAGE_PATH")
root = os.path.abspath(r"$STRUCTURED_DIR")
with zipfile.ZipFile(out, 'w', zipfile.ZIP_DEFLATED) as z:
    for base, dirs, files in os.walk(root):
        for f in files:
            full = os.path.join(base, f)
            arcname = os.path.relpath(full, root)
            z.write(full, arcname)
print('created', out)
PY
  else
    echo "Neither zip nor python3 found to create archive" >&2
    exit 3
  fi
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

if $RESTART_COMPUTE; then
  if command -v kubectl >/dev/null 2>&1; then
    echo "Restarting compute-plane pods (namespace: armonik)"
    kubectl delete -n armonik pods -l service=compute-plane || true
  else
    echo "kubectl not found; cannot restart compute-plane pods" >&2
  fi
fi

echo "Starting stress test runner with: ${RUNNER_ARGS[*]}"
RUNNER_SCRIPT="$SCRIPT_DIR/run-stress-tests.sh"
if [[ ! -x "$RUNNER_SCRIPT" ]]; then
  chmod +x "$RUNNER_SCRIPT" || true
fi

"$RUNNER_SCRIPT" "${RUNNER_ARGS[@]}"

exit 0

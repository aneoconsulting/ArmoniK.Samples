#!/usr/bin/env bash
set -euo pipefail

# Build the worker and create a zip archive
# Usage:
#   ./build-worker.sh --worker-dir /path/to/worker --package-name MyPackage.zip --packages-dir /path/to/packages

WORKER_DIR=""
PACKAGE_NAME=""
PACKAGES_DIR=""

usage() {
  cat <<EOF
Usage: $(basename "$0") [options]

Options:
  --worker-dir PATH       Path to the worker project (default: ../Samples/StressTests/Armonik.Samples.StressTests.Worker)
  --package-name NAME     Output package filename (default: Armonik.Samples.StressTests.Worker-v1.0.0-700.zip)
  --packages-dir PATH     Directory to write the package (default: ./Samples/StressTests/packages)
  -h|--help               Show this help
EOF
  exit 1
}

# defaults (relative to script)
SCRIPT_DIR=$(cd "$(dirname "$0")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)
DEFAULT_WORKER_DIR="$REPO_ROOT/Samples/StressTests/Armonik.Samples.StressTests.Worker"
DEFAULT_PACKAGE_NAME="Armonik.Samples.StressTests.Worker-v1.0.0-700.zip"
DEFAULT_PACKAGES_DIR="$REPO_ROOT/Samples/StressTests/packages"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --worker-dir) WORKER_DIR="$2"; shift 2;;
    --worker-dir=*) WORKER_DIR="${1#*=}"; shift;;
    --package-name) PACKAGE_NAME="$2"; shift 2;;
    --package-name=*) PACKAGE_NAME="${1#*=}"; shift;;
    --packages-dir) PACKAGES_DIR="$2"; shift 2;;
    --packages-dir=*) PACKAGES_DIR="${1#*=}"; shift;;
    -h|--help) usage;;
    *) echo "Unknown arg: $1" >&2; usage;;
  esac
done

WORKER_DIR="${WORKER_DIR:-$DEFAULT_WORKER_DIR}"
PACKAGE_NAME="${PACKAGE_NAME:-$DEFAULT_PACKAGE_NAME}"
PACKAGES_DIR="${PACKAGES_DIR:-$DEFAULT_PACKAGES_DIR}"
mkdir -p "$PACKAGES_DIR"

PACKAGE_PATH="$PACKAGES_DIR/$PACKAGE_NAME"

echo "Building worker from: $WORKER_DIR" >&2
if ! command -v dotnet >/dev/null 2>&1; then
  echo "dotnet not found in PATH" >&2
  exit 2
fi

TMP_PUBLISH_DIR=$(mktemp -d)
STRUCTURED_DIR=$(mktemp -d)
trap 'rm -rf "$TMP_PUBLISH_DIR" "$STRUCTURED_DIR"' EXIT

echo "Publishing to temporary folder: $TMP_PUBLISH_DIR" >&2
dotnet publish "$WORKER_DIR" -c Release -r linux-x64 -f net8.0 -o "$TMP_PUBLISH_DIR"

# Create the expected directory structure for the ZIP
EXPECTED_PATH="$STRUCTURED_DIR/Armonik.Samples.StressTests.Worker/1.0.0-700"
mkdir -p "$EXPECTED_PATH"
cp -r "$TMP_PUBLISH_DIR"/* "$EXPECTED_PATH/"

echo "Packaging published output into zip: $PACKAGE_PATH" >&2
if command -v zip >/dev/null 2>&1; then
  rm -f "$PACKAGE_PATH"
  (cd "$STRUCTURED_DIR" && zip -r "$PACKAGE_PATH" .) >/dev/null
else
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
# created archive at out (no extra stdout to allow callers to capture only the path)
PY
  else
    echo "Neither zip nor python3 found to create archive" >&2
    exit 3
  fi
fi
echo "$PACKAGE_PATH"
exit 0
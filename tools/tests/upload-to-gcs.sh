#!/usr/bin/env bash
# Upload a file (zip or any file) to Google Cloud Storage (GCS)
# Usage:
#   upload-to-gcs.sh -f path/to/file -b my-bucket [-o object-name] [--project PROJECT] [--create-bucket] [--public]
#   upload-to-gcs.sh -f path/to/file --url <gcs-url-or-console-link>  # will try to extract bucket from URL
#   upload-to-gcs.sh -f path/to/file --from-json <file> [--json-key '<jq-path>']  # extract bucket from JSON

set -euo pipefail

PROG_NAME=$(basename "$0")

usage() {
  cat <<EOF
Usage: $PROG_NAME -f <file> -b <bucket> [options]

Options:
  -f, --file FILE          Path to the file to upload (required)
  -b, --bucket BUCKET      GCS bucket name (required unless --url or --from-json used)
  -o, --object OBJECT      Object name (defaults to basename of file)
  --project PROJECT        GCP project (not used; bucket must already exist)
  --public                 Make the uploaded object public (gsutil acl set)
  --url URL                GCS url or console link (script will try to extract bucket)
  --from-json FILE         Extract bucket name from a JSON file (uses jq if available)
  --json-key JQ_PATH       JQ path to extract bucket name from JSON, e.g. '.armonik.ingress.bucket'
  -h, --help               Show this help message

Notes:
 - This script uses gsutil. Ensure the Google Cloud SDK is installed and you are authenticated
   (gcloud auth login or set GOOGLE_APPLICATION_CREDENTIALS to a service account key file).
 - If gsutil is not available, the script will attempt to detect gcloud and print guidance.
EOF
  exit 1
}

# Parse args
FILE=""
BUCKET=""
OBJECT=""
PROJECT=""
  # CREATE_BUCKET removed: buckets must already exist
PUBLIC=false
URL=""
FROM_JSON=""
JSON_KEY=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    -f|--file)
      FILE="$2"; shift 2;;
    -b|--bucket)
      BUCKET="$2"; shift 2;;
    -o|--object)
      OBJECT="$2"; shift 2;;
    --project)
      PROJECT="$2"; shift 2;;
    --public)
      PUBLIC=true; shift;;
    --url)
      URL="$2"; shift 2;;
    --from-json)
      FROM_JSON="$2"; shift 2;;
    --json-key)
      JSON_KEY="$2"; shift 2;;
    -h|--help)
      usage;;
    *)
      echo "Unknown argument: $1" >&2; usage;;
  esac
done


if [[ -z "$FILE" ]]; then
  echo "Error: --file is required" >&2
  usage
fi

if [[ ! -f "$FILE" ]]; then
  echo "Error: file not found: $FILE" >&2
  exit 2
fi

if [[ -z "$OBJECT" ]]; then
  OBJECT=$(basename "$FILE")
fi

# Check gsutil
if ! command -v gsutil >/dev/null 2>&1; then
  echo "gsutil not found in PATH."
  if command -v gcloud >/dev/null 2>&1; then
    echo "gcloud is available. Ensure Google Cloud SDK 'gsutil' component is installed or run 'gcloud components install gsutil'."
  else
    echo "Install Google Cloud SDK (https://cloud.google.com/sdk/docs/install) or ensure gsutil is available."
  fi
  exit 3
fi

# Check auth: try a simple gsutil ls on the bucket (non-fatal)
if ! gsutil ls -p "${PROJECT:-}" >/dev/null 2>&1; then
  echo "Warning: gsutil listing failed; make sure you are authenticated and have project access."
  echo "Run 'gcloud auth login' or set GOOGLE_APPLICATION_CREDENTIALS to a service account key file." 
fi

# Helpers: extract bucket from URL or JSON
extract_bucket_from_url() {
  local u="$1"
  # gs://bucket/path or gs://bucket
  if [[ "$u" =~ ^gs://([^/]+)(/.*)?$ ]]; then
    echo "${BASH_REMATCH[1]}"; return 0
  fi

  # storage.googleapis.com/bucket/...
  if [[ "$u" =~ storage.googleapis.com/([^/]+)/? ]]; then
    echo "${BASH_REMATCH[1]}"; return 0
  fi

  # console.cloud.google.com/storage/browser/_details/bucket or /storage/browser/bucket
  if [[ "$u" =~ /storage/browser/([^/?#]+) ]]; then
    echo "${BASH_REMATCH[1]}"; return 0
  fi

  # fallback: try to extract first path segment
  if [[ "$u" =~ https?://[^/]+/([^/]+)/?.* ]]; then
    echo "${BASH_REMATCH[1]}"; return 0
  fi

  return 1
}

extract_bucket_from_json() {
  local f="$1"
  local key="$2"
  if [[ -n "$key" && $(command -v jq >/dev/null 2>&1; echo $?) -eq 0 ]]; then
    jq -r "$key" "$f" 2>/dev/null || true
    return
  fi
  if command -v jq >/dev/null 2>&1; then
    for q in '.armonik.ingress.bucket' '.armonik.bucket' '.minio.bucket' '.storage.bucket' '.bucket' '.bucket_name' '.s3.bucket'; do
      val=$(jq -r "$q" "$f" 2>/dev/null || echo "null")
      if [[ "$val" != "null" && -n "$val" ]]; then
        echo "$val"; return 0
      fi
    done
  else
    # fallback: grep for "bucket" key and extract value (naive)
    local v=$(grep -oE '"[^"]*bucket[^"]*"[[:space:]]*[:][[:space:]]*"[^"]+"' "$f" | head -n1 || true)
    if [[ -n "$v" ]]; then
      echo "$v" | sed -E 's/.*:[[:space:]]*"([^"]+)".*/\1/'
    fi
  fi
}

# If bucket not provided, try to detect from URL or JSON
if [[ -z "$BUCKET" && -n "$URL" ]]; then
  echo "Attempting to extract bucket from URL: $URL"
  BUCKET=$(extract_bucket_from_url "$URL" || true)
  if [[ -n "$BUCKET" ]]; then
    echo "Detected bucket: $BUCKET"
  else
    echo "Could not detect bucket from URL: $URL" >&2
  fi
fi

if [[ -z "$BUCKET" && -n "$FROM_JSON" ]]; then
  if [[ ! -f "$FROM_JSON" ]]; then
    echo "Error: JSON file not found: $FROM_JSON" >&2
    exit 5
  fi
  echo "Attempting to extract bucket from JSON file: $FROM_JSON"
  BUCKET=$(extract_bucket_from_json "$FROM_JSON" "$JSON_KEY" || true)
  if [[ -n "$BUCKET" ]]; then
    echo "Detected bucket from JSON: $BUCKET"
  else
    echo "Could not detect bucket from JSON: $FROM_JSON" >&2
  fi
fi

if [[ -z "$BUCKET" ]]; then
  echo "Error: bucket not specified and could not be detected. Provide --bucket or --url or --from-json." >&2
  usage
fi

# Create bucket if requested and if it does not exist
# Note: bucket must already exist. Do not attempt to create buckets from this script.

# Perform upload
echo "Uploading $FILE to gs://$BUCKET/$OBJECT"
set -x
gsutil cp "$FILE" "gs://$BUCKET/$OBJECT"
set +x

if $PUBLIC; then
  echo "Making gs://$BUCKET/$OBJECT public (allUsers:READER)"
  gsutil acl ch -u AllUsers:R "gs://$BUCKET/$OBJECT"
fi

echo "Upload complete: gs://$BUCKET/$OBJECT"
exit 0

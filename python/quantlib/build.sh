#!/usr/bin/env bash
# Build the QuantLib worker and client images.
#
#   ./build.sh                                   # worker + client for the local architecture
#   ./build.sh worker --test                     # also run the unit tests inside the image build
#   ./build.sh worker --platform linux/amd64     # one foreign architecture (QEMU binfmt), loaded locally
#   ./build.sh all --platform linux/amd64,linux/arm64 --push --registry docker.io/me
#                                                # multi-arch manifest, pushed to a registry
set -euo pipefail

cd "$(dirname "$0")"

target=all
tag=0.1.0
platform=""
registry=""
run_tests=false
push=false

usage() { sed -n '2,9p' "$0" | sed 's/^# \{0,1\}//'; exit "${1:-0}"; }

while [[ $# -gt 0 ]]; do
  case "$1" in
    worker | client | all) target=$1 ;;
    --tag) tag=$2; shift ;;
    --platform) platform=$2; shift ;;
    --registry) registry=${2%/}/; shift ;;
    --test) run_tests=true ;;
    --push) push=true ;;
    -h | --help) usage ;;
    *) echo "unknown argument: $1" >&2; usage 1 ;;
  esac
  shift
done

if [[ "$platform" == *,* && "$push" != true ]]; then
  echo "a multi-platform build cannot be loaded in the local image store: add --push" >&2
  exit 1
fi

build() {
  local name=$1 dir=$2 image="${registry}armonik_quantlib_$1:${tag}"
  local args=(--tag "$image")
  [[ -n "$platform" ]] && args+=(--platform "$platform")

  if [[ "$name" == worker && "$run_tests" == true ]]; then
    local p
    for p in ${platform//,/ }; do
      echo "==> testing $name ($p)"
      docker buildx build --target test --platform "$p" "$dir"
    done
    if [[ -z "$platform" ]]; then
      echo "==> testing $name (native)"
      docker buildx build --target test "$dir"
    fi
  fi

  echo "==> building $image (${platform:-native})"
  if [[ "$push" == true ]]; then
    docker buildx build "${args[@]}" --builder "$(multiarch_builder)" --push "$dir"
  else
    docker buildx build "${args[@]}" --load "$dir"
  fi
}

# The default "docker" buildx driver cannot produce multi-platform manifests: use a docker-container builder.
multiarch_builder() {
  local builder=armonik-multiarch
  docker buildx inspect "$builder" > /dev/null 2>&1 || docker buildx create --name "$builder" --driver docker-container > /dev/null
  echo "$builder"
}

[[ "$target" == worker || "$target" == all ]] && build worker worker
[[ "$target" == client || "$target" == all ]] && build client client

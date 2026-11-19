#!/usr/bin/env bash
# Refresh the *_SHA256 entries of versions.env for the versions it declares.
# Usage: ./update-checksums.sh
set -euo pipefail

cd "$(dirname "$0")"
# shellcheck source=versions.env
source versions.env

GH=https://github.com/aneoconsulting
GRPC_RPM="${GRPC_RPM_VERSION%.*}-1.el8.x86_64.rpm"

declare -A URLS=(
  [LIBARMONIK_DEB_SHA256]="$GH/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-${API_VERSION}-Linux.deb"
  [LIBARMONIK_DEV_DEB_SHA256]="$GH/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-dev-${API_VERSION}-Linux.deb"
  [LIBARMONIK_RPM_SHA256]="$GH/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-${API_VERSION}-Linux.rpm"
  [LIBARMONIK_DEVEL_RPM_SHA256]="$GH/ArmoniK.Api/releases/download/${API_VERSION}/libarmonik-devel-${API_VERSION}-Linux.rpm"
  [ARMONIKSDK_DEB_SHA256]="$GH/ArmoniK.Extensions.Cpp/releases/download/${SDK_VERSION}/armoniksdk-${SDK_VERSION}.deb"
  [ARMONIKSDK_RPM_SHA256]="$GH/ArmoniK.Extensions.Cpp/releases/download/${SDK_VERSION}/armoniksdk-${SDK_VERSION}.rpm"
  [GRPC_RPM_SHA256]="$GH/grpc-rpm/releases/download/${GRPC_RPM_VERSION}/grpc-${GRPC_RPM}"
  [GRPC_DEVEL_RPM_SHA256]="$GH/grpc-rpm/releases/download/${GRPC_RPM_VERSION}/grpc-devel-${GRPC_RPM}"
)

for key in "${!URLS[@]}"; do
  sum=$(curl -fsSL "${URLS[$key]}" | sha256sum | cut -d' ' -f1)
  sed -i "s/^${key}=.*/${key}=${sum}/" versions.env
  echo "${key}=${sum}"
done

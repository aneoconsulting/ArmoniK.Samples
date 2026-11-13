#!/usr/bin/env bash
# Annotate the fixable vulnerabilities of the blocking severities of a Trivy
# JSON report and, when enforced, exit 1 if there are any. The summary of every
# image is written by the trivyReport job (.github/scripts/trivy-report.py).
# Usage: check.sh REPORT.json NAME SEVERITIES(comma-separated) ENFORCE(true|false)
set -euo pipefail

report=$1 name=$2 severities=$3 enforce=$4

found=$(jq -r --arg sev "${severities}" '
  ($sev | split(",")) as $s
  | [ .Results[]?.Vulnerabilities[]?
      | select((.FixedVersion // "") != "" and (.Severity as $x | $s | index($x)))
      | "\(.VulnerabilityID) (\(.PkgName) \(.InstalledVersion) -> \(.FixedVersion))" ]
  | unique | (length | tostring) + "\t" + join(", ")' "${report}")
count=${found%%$'\t'*} list=${found#*$'\t'}

if [ "${count}" -eq 0 ]; then
  echo "${name}: no fixable ${severities} vulnerability"
  exit 0
fi
if [ "${enforce}" = "true" ]; then
  echo "::error title=Trivy ${name}::${count} fixable ${severities} vulnerabilities: ${list}"
  exit 1
fi
echo "::warning title=Trivy ${name}::${count} fixable ${severities} vulnerabilities: ${list}"

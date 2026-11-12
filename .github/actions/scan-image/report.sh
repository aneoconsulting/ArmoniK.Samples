#!/usr/bin/env bash
# Summarize a Trivy JSON report as Markdown on stdout and, when enforced, exit 1
# if it holds fixable vulnerabilities of the blocking severities.
# Usage: report.sh REPORT.json NAME SEVERITIES(comma-separated) ENFORCE(true|false)
set -euo pipefail

report=$1 name=$2 severities=$3 enforce=$4

# Vulnerabilities with a fixed version available, of a blocking severity
blocking=$(jq -c --arg sev "${severities}" '
  ($sev | split(",")) as $s
  | [ .Results[]? | .Target as $target | .Vulnerabilities[]?
      | select((.FixedVersion // "") != "" and (.Severity as $x | $s | index($x)))
      | {target: $target, id: .VulnerabilityID, pkg: .PkgName,
         installed: .InstalledVersion, fixed: .FixedVersion, severity: .Severity} ]
  | unique_by([.id, .pkg, .target])' "${report}")
count=$(jq 'length' <<< "${blocking}")

echo "### Trivy: ${name}"
echo
echo "All findings by severity: $(jq -r '
  [ .Results[]?.Vulnerabilities[]?.Severity ] | group_by(.) | map("\(.[0]) \(length)")
  | if length == 0 then "none" else join(", ") end' "${report}")"
echo
if [ "${count}" -eq 0 ]; then
  echo "No fixable ${severities} vulnerability."
else
  echo "${count} fixable ${severities} vulnerabilities:"
  echo
  echo "| Severity | ID | Package | Installed | Fixed | Target |"
  echo "|---|---|---|---|---|---|"
  jq -r '.[] | "| \(.severity) | \(.id) | \(.pkg) | \(.installed) | \(.fixed) | \(.target) |"' <<< "${blocking}"
fi
echo

if [ "${count}" -gt 0 ]; then
  ids=$(jq -r '[.[] | "\(.id) (\(.pkg) \(.installed) -> \(.fixed))"] | unique | join(", ")' <<< "${blocking}")
  if [ "${enforce}" = "true" ]; then
    echo "::error title=Trivy ${name}::${count} fixable ${severities} vulnerabilities: ${ids}" >&2
    exit 1
  fi
  # Report only: an annotation makes the findings visible on the checks of the commit
  echo "::warning title=Trivy ${name}::${count} fixable ${severities} vulnerabilities: ${ids}" >&2
fi

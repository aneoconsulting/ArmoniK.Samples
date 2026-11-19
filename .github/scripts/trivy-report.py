#!/usr/bin/env python3
"""Turns the Trivy image reports of the scan-image action into a job summary,
a SARIF file for code scanning and a CSV file. Report only: findings never
fail it; the release gate is in the scan-image action.

Reads <report-dir>/<image>.json, one Trivy JSON report per image. Appends the
markdown summary to <summary-file> and writes, under <output-dir>:
  images.sarif           every image, one run, for one code scanning category
  vulnerabilities.csv    every vulnerability, one row per image and affected package

SARIF_SEVERITY (default HIGH,CRITICAL) filters the SARIF file only: the CSV
file keeps every severity.

Adapted from the image report of ArmoniK.Infra (.github/scripts/trivy-images-report.py).
"""

import argparse
import csv
import json
import os
import re
import subprocess
import tempfile
from dataclasses import dataclass
from pathlib import Path
from typing import NamedTuple

# The rows of an image's critical and high vulnerabilities in the summary,
# which GitHub drops whole past 1 MiB.
CAP = 30

# The severities of the summary's columns, unknown folded into low.
COLUMNS = {"CRITICAL": ["CRITICAL"], "HIGH": ["HIGH"], "MEDIUM": ["MEDIUM"], "LOW": ["LOW", "UNKNOWN"]}


class Vulnerability(NamedTuple):
    """A vulnerability of a package of an image."""
    id: str
    package: str
    path: str
    installed: str
    fixed: str
    status: str
    severity: str
    title: str
    url: str


@dataclass
class Image:
    """A scanned image and its vulnerabilities."""
    name: str
    report: Path
    vulnerabilities: tuple = ()

    @property
    def label(self):
        return f"<code>{self.name}</code>"

    def count(self, column, fixable=False):
        """The vulnerabilities of a summary column, or the fixable ones."""
        return sum(
            1 for vulnerability in self.vulnerabilities
            if vulnerability.severity in COLUMNS[column] and (vulnerability.fixed or not fixable))


def emoji(severity):
    return {
        "CRITICAL": ":red_circle:", "HIGH": ":orange_circle:", "MEDIUM": ":yellow_circle:",
        "LOW": ":large_blue_circle:"}.get(severity, ":white_circle:")


def cell(text):
    """A table cell: no pipe, no tag, no line break."""
    return re.sub(r"[\t\n]", " ", text.replace("|", "\\|").replace("<", "&lt;"))


def trivy_version(key="Version"):
    """A line of `trivy --version`, the first with this key, empty when unknown."""
    try:
        output = subprocess.run(["trivy", "--version"], capture_output=True, encoding="utf-8", check=False).stdout
    except FileNotFoundError:
        return ""
    found = re.search(rf"^ *{key}: (.*)$", output, re.MULTILINE)
    return found.group(1) if found else ""


def load_vulnerabilities(report):
    """One entry per vulnerability and affected package of a Trivy image
    report, the most severe first, then the fixable: Trivy repeats a package
    found in several layers."""
    unique = {}
    for result in report.get("Results") or []:
        for found in result.get("Vulnerabilities") or []:
            vulnerability = Vulnerability(
                found.get("VulnerabilityID") or "", found.get("PkgName") or "", found.get("PkgPath") or "",
                found.get("InstalledVersion") or "", found.get("FixedVersion") or "", found.get("Status") or "",
                found.get("Severity") or "UNKNOWN", found.get("Title") or "", found.get("PrimaryURL") or "")
            unique.setdefault(vulnerability[:4], vulnerability)
    ranks = {"CRITICAL": 0, "HIGH": 1, "MEDIUM": 2, "LOW": 3}
    return sorted(unique.values(), key=lambda found: (ranks.get(found.severity, 4), not found.fixed, *found[:4]))


def sarif_runs(image, artifact_name, work):
    """The SARIF runs of an image. Every location is rewritten to name its
    image: Trivy locates an OS package at the scanned artifact (here an image
    archive or a local tag) and any other package at its path inside the
    image, which collides between images once merged."""
    converted = os.path.join(work, f"{image.name}.sarif")
    subprocess.run([
        "trivy", "convert", "--quiet", "--format", "sarif",
        "--severity", os.environ.get("SARIF_SEVERITY") or "HIGH,CRITICAL", "--output", converted,
        str(image.report)], check=True)
    with open(converted, encoding="utf-8") as file:
        runs = json.load(file)["runs"]
    for run in runs:
        for result in run.get("results") or []:
            result.pop("ruleIndex", None)  # it points into the rules of this run
            for location in result.get("locations") or []:
                artifact = location["physicalLocation"]["artifactLocation"]
                uri = artifact["uri"]
                artifact["uri"] = image.name if uri in (artifact_name, "") else f"{image.name}/{uri}"
    return runs


def merge_sarif(runs):
    """Code scanning rejects two runs of one tool under one category, so the
    runs are merged into one, rules deduplicated by id. With no image
    scanned, an empty run still clears the alerts no longer found."""
    trivy = {"tool": {"driver": {"name": "Trivy", "informationUri": "https://github.com/aquasecurity/trivy"}}}
    merged = runs[0] if runs else trivy
    merged.pop("properties", None)
    rules = {}
    for run in runs:
        for rule in run["tool"]["driver"].get("rules") or []:
            rules.setdefault(rule["id"], rule)
    merged["tool"]["driver"]["rules"] = [rules[rule] for rule in sorted(rules)]
    merged["results"] = [result for run in runs for result in run.get("results") or []]
    return {"$schema": "https://json.schemastore.org/sarif-2.1.0.json", "version": "2.1.0", "runs": [merged]}


def images_table(images):
    """A row per image, the most vulnerable first."""
    lines = [
        f"**{len(images)} images scanned**: "
        f":red_circle: {sum(1 for image in images if image.count('CRITICAL'))} with critical vulnerabilities, "
        f":orange_circle: {sum(1 for image in images if image.count('HIGH'))} with high ones.",
        "",
        "Counts: found (fixable), low including unknown. "
        "Every vulnerability is in `vulnerabilities.csv` of the `trivy-reports` artifact.",
        "",
        "| Image | Critical | High | Medium | Low |", "|-|-:|-:|-:|-:|"]
    for image in images:
        counts = [
            f"{emoji(column)} {image.count(column)} ({image.count(column, fixable=True)})" if image.count(column)
            else "-" for column in COLUMNS]
        lines.append(f"| {image.label} | {' | '.join(counts)} |")
    return lines


def vulnerabilities_details(image):
    """The critical and high vulnerabilities of an image, folded."""
    rows = []
    for found in image.vulnerabilities:
        row = (f"| {emoji(found.severity)} | [{found.id}]({found.url}) | `{cell(found.package)}` "
               f"| {cell(found.installed)} | {cell(found.fixed) or '-'} | {cell(found.title)} |")
        # One row for the copies of a package that differ by their path only.
        if found.severity in ("CRITICAL", "HIGH") and row not in rows[-1:]:
            rows.append(row)
    if len(rows) > CAP:
        rows[CAP:] = [f"| | ... and {len(rows) - CAP} more in `vulnerabilities.csv` | | | | |"]
    critical, high = image.count("CRITICAL"), image.count("HIGH")
    return [
        f"<details><summary>{emoji('CRITICAL' if critical else 'HIGH')} {image.label}: "
        f"{critical} critical ({image.count('CRITICAL', fixable=True)} fixable), "
        f"{high} high ({image.count('HIGH', fixable=True)} fixable)</summary>",
        "",
        "| | Vulnerability | Package | Installed | Fixed | Title |", "|-|-|-|-|-|-|", *rows, "", "</details>", ""]


def write_csv(path, header, rows):
    path.parent.mkdir(parents=True, exist_ok=True)
    with open(path, "w", newline="", encoding="utf-8") as file:
        csv.writer(file, quoting=csv.QUOTE_ALL, lineterminator="\n").writerows([header, *rows])


def main():
    parser = argparse.ArgumentParser(description=__doc__.split("\n\n")[0])
    parser.add_argument("reports", type=Path, metavar="<report-dir>")
    parser.add_argument("summary", type=Path, metavar="<summary-file>")
    parser.add_argument("output", type=Path, metavar="<output-dir>")
    options = parser.parse_args()

    images, runs, csv_rows = [], [], []
    with tempfile.TemporaryDirectory() as work:
        for report in sorted(options.reports.glob("*.json")):
            content = json.loads(report.read_text(encoding="utf-8"))
            image = Image(report.stem, report, load_vulnerabilities(content))
            images.append(image)
            runs += sarif_runs(image, content.get("ArtifactName") or "", work)
            csv_rows += [
                (image.name, found.severity, found.id, found.package, found.path,
                 found.installed, found.fixed, found.status, found.title, found.url)
                for found in image.vulnerabilities]

    options.output.mkdir(parents=True, exist_ok=True)
    write_csv(options.output / "vulnerabilities.csv", (
        "image", "severity", "vulnerability", "package", "path",
        "installed", "fixed", "status", "title", "url"), csv_rows)
    with open(options.output / "images.sarif", "w", encoding="utf-8") as file:
        json.dump(merge_sarif(runs), file, indent=2, ensure_ascii=False)
        file.write("\n")

    images.sort(key=lambda image: (*(-image.count(column) for column in COLUMNS), image.name))
    lines = [
        "## Trivy: images",
        "",
        f"Trivy {trivy_version() or 'unknown'}, vulnerability database of {trivy_version('UpdatedAt') or 'unknown date'}. "
        "This report blocks nothing; a release is blocked by fixable critical or high vulnerabilities, "
        "unless listed in `.trivyignore`.",
        "",
        *images_table(images),
        "", "### Critical and high vulnerabilities", "",
        f"The most severe first, then the fixable, at most {CAP} an image: the rest is in `vulnerabilities.csv`.",
        ""]
    for image in images:
        if image.count("CRITICAL") or image.count("HIGH"):
            lines += vulnerabilities_details(image)
    with open(options.summary, "a", encoding="utf-8") as file:
        file.write("\n".join(lines) + "\n")


if __name__ == "__main__":
    main()

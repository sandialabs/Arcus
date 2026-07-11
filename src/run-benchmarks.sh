#!/usr/bin/env bash
# Run Arcus benchmarks and export results to src/results/.
#
# Usage:
#   ./src/run-benchmarks.sh                       # run all benchmarks
#   ./src/run-benchmarks.sh -- --filter *Subnet*  # run a subset
#
# Results land in src/results/<timestamp>/ as JSON and Markdown files.
# Commit that directory when you want to record a progression snapshot.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

dotnet run --project "${SCRIPT_DIR}/Arcus.Benchmarks" \
    -c Release \
    -- --artifacts "${SCRIPT_DIR}/results" --exporters json markdown "$@"

#!/usr/bin/env bash
# Run Arcus NuGet package smoke tests on Linux/macOS.
#
# What this script does:
#   1. Packs the Arcus library into a local NuGet feed (smoketests/feed/).
#   2. Restores and builds the SmokeTests consumer project against that feed.
#   3. Runs the consumer for each supported framework and reports results.
#
# net48 is excluded because .NET Framework does not run natively on Linux/macOS.
# Use run-smoke-tests.ps1 on Windows to cover net48 (netstandard2.0 asset).
#
# Usage:
#   ./smoketests/run-smoke-tests.sh [VERSION]
#
#   VERSION  SemVer string for the packed library.  Defaults to 99.0.0-smoke.
#
# Exit codes:
#   0  all frameworks passed
#   1  one or more frameworks failed

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
FEED_DIR="${SCRIPT_DIR}/feed"
CONSUMER_PROJ="${SCRIPT_DIR}/SmokeTests/SmokeTests.csproj"
LIB_PROJ="${REPO_ROOT}/src/Arcus/Arcus.csproj"
FRAMEWORKS=("net8.0" "net9.0" "net10.0")
VERSION="${1:-99.0.0-smoke}"

# ── helpers ──────────────────────────────────────────────────────────────────

header() { printf '\n\033[1;36m%s\033[0m\n' "$*"; }
ok()     { printf '\033[0;32m  ✓ %s\033[0m\n' "$*"; }
err()    { printf '\033[0;31m  ✗ %s\033[0m\n' "$*" >&2; }

overall_exit=0

# ── 1. Pack ───────────────────────────────────────────────────────────────────

header "Step 1 — Pack Arcus ${VERSION}"

rm -rf "${FEED_DIR}"
mkdir -p "${FEED_DIR}"

dotnet pack "${LIB_PROJ}" \
    --configuration Release \
    --output "${FEED_DIR}" \
    -p:VersionFromCI="${VERSION}" \
    --verbosity quiet

ok "Packed to ${FEED_DIR}"

# ── 2. Restore ────────────────────────────────────────────────────────────────

header "Step 2 — Restore SmokeTests consumer"

dotnet restore "${CONSUMER_PROJ}" \
    -p:ArcusVersion="${VERSION}" \
    --verbosity quiet

ok "Restored"

# ── 3. Build ──────────────────────────────────────────────────────────────────

header "Step 3 — Build SmokeTests consumer (all frameworks)"

dotnet build "${CONSUMER_PROJ}" \
    --no-restore \
    --configuration Release \
    -p:ArcusVersion="${VERSION}" \
    --verbosity quiet

ok "Built"

# ── 4. Run per framework ─────────────────────────────────────────────────────

header "Step 4 — Run smoke tests"

for fw in "${FRAMEWORKS[@]}"; do
    printf '\n  Framework: %s\n' "${fw}"
    if dotnet run \
        --project "${CONSUMER_PROJ}" \
        --framework "${fw}" \
        --configuration Release \
        --no-build \
        -p:ArcusVersion="${VERSION}"; then
        ok "${fw} passed"
    else
        err "${fw} FAILED"
        overall_exit=1
    fi
done

# ── Summary ───────────────────────────────────────────────────────────────────

echo ""
if [ "${overall_exit}" -eq 0 ]; then
    printf '\033[1;32mAll smoke tests passed.\033[0m\n'
else
    printf '\033[1;31mOne or more frameworks FAILED.\033[0m\n' >&2
fi

exit "${overall_exit}"

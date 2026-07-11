#Requires -Version 5.1
<#
.SYNOPSIS
    Run Arcus NuGet package smoke tests on Windows (all frameworks including net48).

.DESCRIPTION
    1. Packs the Arcus library into a local NuGet feed (smoketests\feed\).
    2. Restores and builds the SmokeTests consumer project against that feed.
    3. Runs the consumer for each supported framework and reports results.

    net48 validates the netstandard2.0 asset on .NET Framework — the TFM that cannot
    be covered by a Linux runner.  The script also covers net8.0, net9.0, and net10.0.

.PARAMETER Version
    SemVer string for the packed library.  Defaults to "99.0.0-smoke".

.EXAMPLE
    .\smoketests\run-smoke-tests.ps1

    .\smoketests\run-smoke-tests.ps1 -Version 4.1.0-preview
.NOTES
    Exit code 0 = all frameworks passed; 1 = one or more frameworks failed.
#>

param(
    [string]$Version = "99.0.0-smoke"
)

$ErrorActionPreference = "Stop"

$ScriptDir    = $PSScriptRoot
$RepoRoot     = (Get-Item $ScriptDir).Parent.FullName
$FeedDir      = Join-Path $ScriptDir "feed"
$ConsumerProj = Join-Path $ScriptDir "SmokeTests\SmokeTests.csproj"
$LibProj      = Join-Path $RepoRoot  "src\Arcus\Arcus.csproj"
$Frameworks   = @("net48", "net8.0", "net9.0", "net10.0")

$OverallExit = 0

function Write-Header([string]$Text) {
    Write-Host "`n$Text" -ForegroundColor Cyan
}
function Write-Ok([string]$Text)  { Write-Host "  OK  $Text" -ForegroundColor Green }
function Write-Err([string]$Text) { Write-Host "  ERR $Text" -ForegroundColor Red }

# ── 1. Pack ───────────────────────────────────────────────────────────────────

Write-Header "Step 1 — Pack Arcus $Version"

if (Test-Path $FeedDir) { Remove-Item $FeedDir -Recurse -Force }
New-Item -ItemType Directory -Path $FeedDir | Out-Null

dotnet pack $LibProj `
    --configuration Release `
    --output $FeedDir `
    "-p:VersionFromCI=$Version" `
    --verbosity quiet

if ($LASTEXITCODE -ne 0) { Write-Err "Pack failed"; exit 1 }
Write-Ok "Packed to $FeedDir"

# ── 2. Restore ────────────────────────────────────────────────────────────────

Write-Header "Step 2 — Restore SmokeTests consumer"

dotnet restore $ConsumerProj `
    "-p:ArcusVersion=$Version" `
    --verbosity quiet

if ($LASTEXITCODE -ne 0) { Write-Err "Restore failed"; exit 1 }
Write-Ok "Restored"

# ── 3. Build ──────────────────────────────────────────────────────────────────

Write-Header "Step 3 — Build SmokeTests consumer (all frameworks)"

dotnet build $ConsumerProj `
    --no-restore `
    --configuration Release `
    "-p:ArcusVersion=$Version" `
    --verbosity quiet

if ($LASTEXITCODE -ne 0) { Write-Err "Build failed"; exit 1 }
Write-Ok "Built"

# ── 4. Run per framework ─────────────────────────────────────────────────────

Write-Header "Step 4 — Run smoke tests"

foreach ($fw in $Frameworks) {
    Write-Host "`n  Framework: $fw"

    dotnet run `
        --project $ConsumerProj `
        --framework $fw `
        --configuration Release `
        --no-build `
        "-p:ArcusVersion=$Version"

    if ($LASTEXITCODE -eq 0) {
        Write-Ok "$fw passed"
    } else {
        Write-Err "$fw FAILED"
        $OverallExit = 1
    }
}

# ── Summary ───────────────────────────────────────────────────────────────────

Write-Host ""
if ($OverallExit -eq 0) {
    Write-Host "All smoke tests passed." -ForegroundColor Green
} else {
    Write-Host "One or more frameworks FAILED." -ForegroundColor Red
}

exit $OverallExit

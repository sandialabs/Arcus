#Requires -Version 5.1
<#
.SYNOPSIS
    Run Arcus benchmarks and export results to src\results\.

.DESCRIPTION
    Builds the benchmark project in Release mode and runs all benchmarks.
    Results land in src\results\<timestamp>\ as JSON and Markdown files.
    Commit that directory when you want to record a progression snapshot.

.PARAMETER AdditionalArgs
    Extra arguments forwarded to BenchmarkDotNet (e.g. "--filter *Subnet*").

.EXAMPLE
    .\src\run-benchmarks.ps1
    .\src\run-benchmarks.ps1 -AdditionalArgs "--filter","*Subnet*"
#>

param(
    [string[]]$AdditionalArgs = @()
)

$ErrorActionPreference = "Stop"

$ScriptDir = $PSScriptRoot

dotnet run --project "$ScriptDir\Arcus.Benchmarks" `
    -c Release `
    -- --artifacts "$ScriptDir\results" --exporters json markdown @AdditionalArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Benchmarks failed with exit code $LASTEXITCODE" -ForegroundColor Red
    exit $LASTEXITCODE
}

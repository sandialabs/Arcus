# Arcus Smoke Tests

Smoke tests that prove the **packed NuGet package** works for every supported target framework.

## Why these exist

The main `src/Arcus.Tests/` suite references the library via a *project reference*, which
bypasses NuGet asset-selection entirely.  A project reference can never surface:

- Incorrect `lib/<tfm>/` folder layout in the `.nupkg`
- Wrong TFM asset being chosen by a consumer (e.g. `net9.0` falling back to `netstandard2.0`
  when a dedicated `net9.0` asset should exist)
- BCL types referenced by a C# 14 language feature that are missing on an older runtime
- Missing or mismatched transitive dependencies declared in the `.nuspec`

Smoke tests fix this by **packing the library first**, then referencing it as a NuGet package
from a separate consumer project — exactly as an end-user would.

## Structure

```
smoketests/
├── nuget.config              # adds smoketests/feed/ as a local NuGet source
├── run-smoke-tests.sh        # Linux / macOS (net8.0, net9.0, net10.0)
├── run-smoke-tests.ps1       # Windows     (net48, net8.0, net9.0, net10.0)
└── SmokeTests/
    ├── SmokeTests.csproj     # multi-target console app; PackageReference to Arcus
    └── Program.cs            # exercises the full public API surface
```

`smoketests/feed/` is git-ignored; the run scripts create it on demand.

## Running locally

### Linux / macOS

```bash
# From the repo root:
./smoketests/run-smoke-tests.sh

# With an explicit version:
./smoketests/run-smoke-tests.sh 4.0.0-preview
```

Covers **net8.0**, **net9.0**, **net10.0**.

### Windows (PowerShell)

```powershell
# From the repo root:
.\smoketests\run-smoke-tests.ps1

# With an explicit version:
.\smoketests\run-smoke-tests.ps1 -Version 4.0.0-preview
```

Covers **net48** (→ `netstandard2.0` asset), **net8.0**, **net9.0**, **net10.0**.

> **net48 requires Windows.**  The `netstandard2.0` asset is what any .NET Framework
> or legacy .NET Standard consumer receives; net48 is the proxy TFM that validates it.

## What is tested

`Program.cs` exercises the entire public API surface against a real runtime:

| Area | Checks |
|---|---|
| `Subnet` (IPv4) | Parse, construct (address+prefix, two addresses), Netmask, Contains, Overlaps, Touches, Enumerate, ContainsAnyPrivateAddresses, ContainsAllPrivateAddresses, TryParse, Deconstruct |
| `Subnet` (IPv6) | Parse, construct, Contains, Overlaps |
| `IPAddressRange` | Construct, Contains, Overlaps |
| `SubnetUtilities` | FewestConsecutiveSubnetsFor, PrivateIPAddressRangesList, LargestSubnet, SmallestSubnet |
| `IPAddressMath` | Increment, TryIncrement, IsGreaterThan/LessThan/EqualTo, IsBetween, Max, Min, IsAtMin/Max |
| `IPAddressUtilities` | IsIPv4, IsIPv6, IsPrivate, IPv4Min/MaxAddress, IsValidNetMask, ParseFromHexString |
| `IPAddressConverters` | NetmaskToCidrRoutePrefix, ToHexString, ToNumericString |
| `Comparers` | DefaultIPAddressComparer, DefaultIIPAddressRangeComparer, DefaultAddressFamilyComparer |

## CI

The `smoke-test` job in `.github/workflows/build.yml` runs automatically on every PR and push
to `main`, after the main `build` job succeeds.  It uses a matrix strategy:

- **ubuntu-latest** → net8.0, net9.0, net10.0
- **windows-latest** → net48 (netstandard2.0 asset validation)

Exit code 1 from any framework fails the job.

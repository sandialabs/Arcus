# Changelog

All notable changes to Arcus are documented here.
This project adheres to [Semantic Versioning](https://semver.org/).

## [5.0.0] - TBD

### Added

- .NET 10 (`net10.0`) target framework
- `IIPAddressRange.ToIPAddresses()` -- safe enumeration method capped at `MaxEnumerationExponent`
- `MaxEnumerationExponent` property on all range types (default 12 = 4096 addresses)
- `SubnetUtilities.FewestConsecutiveSubnetsFor` overload with `maxEnumerationExponent` parameter
- Regex source generators for improved parsing performance
- Partial class split for maintainability (`Subnet`, `IPAddressRange`, `AbstractIPAddressRange`, `BigEndianBitWrapper`)

### Changed

- `SubnetUtilities.PrivateIPAddressRangesList` and `LinkLocalIPAddressRangesList` are now `readonly` (lazily initialized)
- `AbstractIPAddressRange.Overlaps` -- symmetry restored (both directions now return `true`)
- `IPAddressRange.TryExcludeAll` -- boundary guards at family min/max (no longer throws)
- `Subnet` parsing and `IPAddressUtilities` parsing use `[GeneratedRegex]` where available
- Package dependencies updated and centralized via `Directory.Packages.props`
- Deterministic builds enabled
- Language version set to C# 14
- XML documentation improved throughout all public APIs with RFC references
- Exception types corrected (`InvalidOperationException` to `ArgumentException` where appropriate)

### Removed

- `MacAddress` type (was `[Obsolete]` in v3.x) - replacement coming soon-ish in a new package
- `DefaultIPAddressRangeComparer` (use `DefaultIIPAddressRangeComparer`)
- Drops [Gulliver](https://github.com/sandialabs/gulliver) dependency

### Fixed

- `AbstractIPAddressRange.Overlaps` asymmetry (inner.Overlaps(outer) returned `false`)
- `ContainsAnyPrivateAddresses`/`ContainsAllPublicAddresses` endpoint heuristic producing incorrect results for ranges spanning private subnets
- `IPAddressRange.TryExcludeAll` throwing `InvalidOperationException` at family max address
- IComparable consistency across range types

### Deprecated (will be removed in future major version)

- `GetEnumerator()` on any range type -- use `ToIPAddresses()` instead
- `IIPAddressRange` implementing `IEnumerable<IPAddress>` -- use `ToIPAddresses()` explicitly
- `Subnet.TryIPv6FromPartial(string, out IEnumerable<Subnet>)` -- to be replaced / improved by more explicit methods

### Testing

- Comprehensive test suite expansion across all range types, comparers, converters, math, and utilities
- `Subnet` tests split into partial files: factory, `IComparable`, `IEquatable`, `ISerializable`, operators
- `IPAddressRange` tests split into partial files: factory, `IComparable`, `IEquatable`, `ISerializable`, operators
- `AbstractIPAddressRange` tests split into: `IEnumerable`, `IFormattable`, `IIPAddressRange`
- Smoke test project (`smoketests/`) validating the packed NuGet package on real runtimes

### Infrastructure

- Benchmark project (`src/Arcus.Benchmarks/`) with BenchmarkDotNet for all major types
- CI/CD pipeline rework: build, CodeQL, dependency review, OpenSSF Scorecard, and smoke-test workflows
- `.slnx` solution format (replaces legacy `.sln`)
- `CONTRIBUTING.md` with build, test, and PR guidance
- EditorConfig consolidated to repository root

## [4.0.0] - 2026-01-15

## [3.0.0] - 2025-06-15

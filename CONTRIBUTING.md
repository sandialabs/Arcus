# Contributing to Arcus

## Prerequisites

- .NET SDK (see `src/global.json` for the pinned version)
- (optional) the dev container in `.devcontainer/` if you'd rather not install the SDK locally

## Build & test

```bash
cd src
dotnet restore
dotnet build
dotnet test
```

## Project layout

This repo follows a standard layout: `src/` for the library and its unit
tests, `smoketests/` for package-consumption tests, `docs/` for design and
published documentation. See `docs/design/adr/` for the reasoning behind
non-obvious structural choices.

## Pull requests

1. Fork and branch from `main`.
2. Keep changes focused -- one logical change per PR.
3. Add or update tests for behavior changes.
4. Run `dotnet format` before pushing (or let the pre-commit hook do it).
5. Fill out the PR template; link any related issue.

## Code style

Formatting and analyzer rules are enforced via `.editorconfig` and
`Directory.Build.props`. CI will fail the build on analyzer warnings treated
as errors -- run `dotnet build` locally to catch these before pushing.

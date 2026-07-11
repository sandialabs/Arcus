#!/usr/bin/env bash

set -e

# Ensure workspace is trusted by git.
git config --global --add safe.directory "$(pwd)"

# Standardize line ending handling.
git config --global core.autocrlf false

# Avoid detached head warnings in some container scenarios.
git config --global advice.detachedHead false
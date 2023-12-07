#!/usr/bin/env bash
set -euo pipefail
dotnet run --project build --framework "net8.0" -- "$@"

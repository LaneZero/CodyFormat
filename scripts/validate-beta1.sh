#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "== CodyFormat v0.7.0-beta.1 validation =="
dotnet --version
dotnet restore CodyFormat.sln
dotnet build CodyFormat.sln -c Release --no-restore
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release --no-build

version="$(dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --version)"
[[ "$version" == "0.7.0-beta.1" ]]

status="$(dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --native-status)"
grep -F 'Black' <<<"$status" >/dev/null
grep -F 'Prettier' <<<"$status" >/dev/null

native_output="$(printf 'def f(a,b):\n  return a+b\n' | dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --stdin --language Python --native --quiet)"
grep -F 'def f(a, b):' <<<"$native_output" >/dev/null

# SmokeTests contain the cumulative Alpha.1/Alpha.2/Alpha.3 regression gates.
echo "CodyFormat v0.7.0-beta.1 validation PASS"

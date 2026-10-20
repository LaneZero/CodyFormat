#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "== CodyFormat v0.7.0-alpha.2 validation =="
dotnet --version
dotnet restore CodyFormat.sln
dotnet build CodyFormat.sln -c Release --no-restore
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release --no-build

version="$(dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --version)"
[[ "$version" == "0.7.0-alpha.2" ]]
dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --help >/dev/null

tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT
mkdir -p "$tmp/src" "$tmp/bin" "$tmp/Generated"
printf 'def f(a,b):\n  x=1\n  return a+b\n' > "$tmp/src/sample.py"
printf 'def f(a,b):\n  return a+b\n' > "$tmp/bin/ignored.py"
printf 'def f(a,b):\n  return a+b\n' > "$tmp/Generated/generated.py"

set +e
dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- "$tmp" --recursive --check --exclude '**/Generated/**' --quiet
status=$?
set -e
[[ $status -eq 1 ]]
grep -F 'def f(a,b):' "$tmp/src/sample.py" >/dev/null

dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- "$tmp" --recursive --write --backup --include '**/*.py' --exclude '**/Generated/**' --quiet
grep -F 'def f(a, b):' "$tmp/src/sample.py" >/dev/null
grep -F '    x = 1' "$tmp/src/sample.py" >/dev/null
test -f "$tmp/src/sample.py.codyformat.bak"
grep -F 'def f(a,b):' "$tmp/bin/ignored.py" >/dev/null
grep -F 'def f(a,b):' "$tmp/Generated/generated.py" >/dev/null

dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- "$tmp" --recursive --check --include '**/*.py' --exclude '**/Generated/**' --quiet

echo "CodyFormat v0.7.0-alpha.2 validation PASS"

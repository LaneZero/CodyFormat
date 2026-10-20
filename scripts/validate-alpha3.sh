#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
cd "$ROOT"

echo "== CodyFormat v0.7.0-alpha.3 validation =="
dotnet --version
dotnet restore CodyFormat.sln
dotnet build CodyFormat.sln -c Release --no-restore
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release --no-build

version="$(dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --version)"
[[ "$version" == "0.7.0-alpha.3" ]]

tmp="$(mktemp -d)"
trap 'rm -rf "$tmp"' EXIT
mkdir -p "$tmp/src"
cat > "$tmp/.editorconfig" <<'EOF'
root = true

[*.py]
indent_style = space
indent_size = 2
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true
EOF
printf 'def f(a,b):\r\n    x=1   ' > "$tmp/src/sample.py"

set +e
dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- "$tmp/src/sample.py" --check --quiet
status=$?
set -e
[[ $status -eq 1 ]]

dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- "$tmp/src/sample.py" --write --quiet
grep -F '  x = 1' "$tmp/src/sample.py" >/dev/null
if grep -q $'\r' "$tmp/src/sample.py"; then echo "CR found after end_of_line=lf" >&2; exit 1; fi
if grep -Eq '[[:blank:]]+$' "$tmp/src/sample.py"; then echo "Trailing whitespace remained" >&2; exit 1; fi
last_byte="$(tail -c 1 "$tmp/src/sample.py" | od -An -t u1 | tr -d '[:space:]')"
[[ "$last_byte" == "10" ]]

cat > "$tmp/team.codyprofile.json" <<'EOF'
{
  "schemaVersion": 2,
  "name": "Team Two Space",
  "description": "Validation profile",
  "formatting": { "indentStyle": "space", "indentSize": 2 },
  "appearance": { "theme": "Dark", "background": "Theme Default", "noBackground": false, "lineNumbers": true, "border": true, "fontFamily": "Cascadia Mono", "fontSize": 14 },
  "output": { "formatBeforeOutput": false, "target": "General" }
}
EOF
printf 'def f(a,b):\n    x=1\n' > "$tmp/src/sample.py"
dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- "$tmp/src/sample.py" --write --profile "$tmp/team.codyprofile.json" --no-editorconfig --quiet
grep -F '  x = 1' "$tmp/src/sample.py" >/dev/null

echo "CodyFormat v0.7.0-alpha.3 validation PASS"

#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
ARTIFACTS="$ROOT/artifacts"
OUTPUT="$ARTIFACTS/SHA256SUMS.txt"

if [[ ! -d "$ARTIFACTS" ]]; then
  echo "Artifacts directory does not exist. Build release packages first." >&2
  exit 1
fi

# Keep this script compatible with the older Bash shipped by macOS.
shopt -s nullglob
FILES=(
  "$ARTIFACTS"/CodyFormat-v*-win-x64.zip
  "$ARTIFACTS"/CodyFormat-v*-win-arm64.zip
  "$ARTIFACTS"/CodyFormat-v*-linux-x64.tar.gz
  "$ARTIFACTS"/CodyFormat-v*-linux-arm64.tar.gz
  "$ARTIFACTS"/CodyFormat-v*-macos-arm64.zip
  "$ARTIFACTS"/CodyFormat-v*-macos-x64.zip
)
shopt -u nullglob

if [[ ${#FILES[@]} -eq 0 ]]; then
  echo "No CodyFormat release archives were found in artifacts/." >&2
  exit 1
fi

TMP="${OUTPUT}.tmp"
: > "$TMP"

for FILE in "${FILES[@]}"; do
  BASENAME="$(basename "$FILE")"
  if command -v sha256sum >/dev/null 2>&1; then
    HASH="$(sha256sum "$FILE" | awk '{print $1}')"
  elif command -v shasum >/dev/null 2>&1; then
    HASH="$(shasum -a 256 "$FILE" | awk '{print $1}')"
  else
    echo "Neither sha256sum nor shasum is available." >&2
    rm -f "$TMP"
    exit 1
  fi
  printf '%s  %s\n' "$HASH" "$BASENAME" >> "$TMP"
done

LC_ALL=C sort "$TMP" > "$OUTPUT"
rm -f "$TMP"

echo "Created $OUTPUT"
cat "$OUTPUT"

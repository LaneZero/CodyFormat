#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
for RID in linux-x64 linux-arm64; do
  dotnet publish "$ROOT/src/CodyFormat.Desktop/CodyFormat.Desktop.csproj" -c Release -r "$RID" --self-contained true -p:UseAppHost=true -o "$ROOT/artifacts/$RID"
done
echo "Linux artifacts created under artifacts/"

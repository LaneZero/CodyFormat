#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VERSION="$(tr -d '\r\n' < "$ROOT/VERSION.txt")"
DESKTOP_PROJECT="$ROOT/src/CodyFormat.Desktop/CodyFormat.Desktop.csproj"
CLI_PROJECT="$ROOT/src/CodyFormat.Cli/CodyFormat.Cli.csproj"

for RID in linux-x64 linux-arm64; do
  OUT="$ROOT/artifacts/$RID"
  CLI_OUT="$ROOT/artifacts/publish-cli/$RID"
  ARCHIVE="$ROOT/artifacts/CodyFormat-v${VERSION}-${RID}.tar.gz"

  echo "Publishing desktop $RID..."
  rm -rf "$OUT" "$CLI_OUT"
  rm -f "$ARCHIVE"
  mkdir -p "$OUT" "$CLI_OUT"

  dotnet publish "$DESKTOP_PROJECT" -c Release -r "$RID" --self-contained true -p:UseAppHost=true -o "$OUT"
  dotnet publish "$CLI_PROJECT" -c Release -r "$RID" --self-contained true -p:UseAppHost=true -o "$CLI_OUT"

  chmod +x "$OUT/CodyFormat" "$CLI_OUT/codyformat"

  cp "$ROOT/packaging/linux/install-user.sh" "$OUT/install-user.sh"
  cp "$ROOT/packaging/linux/uninstall-user.sh" "$OUT/uninstall-user.sh"
  chmod +x "$OUT/install-user.sh" "$OUT/uninstall-user.sh"
  mkdir -p "$OUT/desktop-integration"
  cp "$ROOT/packaging/linux/codyformat.desktop" "$OUT/desktop-integration/codyformat.desktop"
  cp -a "$ROOT/packaging/linux/icons" "$OUT/desktop-integration/"
  cp -a "$ROOT/packaging/linux/pixmaps" "$OUT/desktop-integration/"

  mkdir -p "$OUT/cli"
  cp -a "$CLI_OUT/." "$OUT/cli/"
  chmod +x "$OUT/cli/codyformat"

  tar -C "$ROOT/artifacts" -czf "$ARCHIVE" "$RID"
  echo "Created $ARCHIVE"
done

echo "Linux artifacts created. Desktop is at package root; CLI is under cli/."

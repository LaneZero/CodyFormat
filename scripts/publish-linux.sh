#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
VERSION="$(tr -d '\r\n' < "$ROOT/VERSION.txt")"

for RID in linux-x64 linux-arm64; do
  OUT="$ROOT/artifacts/$RID"
  echo "Publishing $RID..."
  rm -rf "$OUT"
  mkdir -p "$OUT"

  dotnet publish "$ROOT/src/CodyFormat.Desktop/CodyFormat.Desktop.csproj" \
    -c Release -r "$RID" --self-contained true -p:UseAppHost=true -o "$OUT"

  chmod +x "$OUT/CodyFormat"

  # Ship current-user desktop integration. This does not require sudo.
  cp "$ROOT/packaging/linux/install-user.sh" "$OUT/install-user.sh"
  cp "$ROOT/packaging/linux/uninstall-user.sh" "$OUT/uninstall-user.sh"
  chmod +x "$OUT/install-user.sh" "$OUT/uninstall-user.sh"
  mkdir -p "$OUT/desktop-integration"
  cp "$ROOT/packaging/linux/codyformat.desktop" "$OUT/desktop-integration/codyformat.desktop"
  cp -a "$ROOT/packaging/linux/icons" "$OUT/desktop-integration/"
  cp -a "$ROOT/packaging/linux/pixmaps" "$OUT/desktop-integration/"

  ARCHIVE="$ROOT/artifacts/CodyFormat-v${VERSION}-${RID}.tar.gz"
  rm -f "$ARCHIVE"
  tar -C "$ROOT/artifacts" -czf "$ARCHIVE" "$RID"
  echo "Created $ARCHIVE"
done

echo "Linux artifacts created successfully under artifacts/."
echo "Run ./artifacts/linux-x64/CodyFormat for a portable test."
echo "Run ./artifacts/linux-x64/install-user.sh for GNOME/KDE launcher + icon integration (no sudo)."

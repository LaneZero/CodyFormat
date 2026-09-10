#!/usr/bin/env bash
set -euo pipefail

# Removes the current-user CodyFormat installation created by install-user.sh.
rm -rf "$HOME/.local/share/codyformat"
rm -f "$HOME/.local/bin/codyformat"
rm -f "$HOME/.local/share/applications/codyformat.desktop"
rm -f "$HOME/.local/share/pixmaps/codyformat.png"
for size in 16 24 32 48 64 128 256 512; do
  rm -f "$HOME/.local/share/icons/hicolor/${size}x${size}/apps/codyformat.png"
done

if command -v update-desktop-database >/dev/null 2>&1; then
  update-desktop-database "$HOME/.local/share/applications" >/dev/null 2>&1 || true
fi

echo "CodyFormat user installation removed."

#!/usr/bin/env bash
set -euo pipefail

# Installs CodyFormat for the current Linux user only. No sudo/root access is required.
SOURCE_DIR="$(cd "$(dirname "$0")" && pwd)"
APP_DIR="$HOME/.local/share/codyformat"
DESKTOP_DIR="$HOME/.local/share/applications"
ICON_ROOT="$HOME/.local/share/icons/hicolor"

mkdir -p "$DESKTOP_DIR" "$HOME/.local/bin"
if [[ "$SOURCE_DIR" != "$APP_DIR" ]]; then
  rm -rf "$APP_DIR"
  mkdir -p "$APP_DIR"
  cp -a "$SOURCE_DIR"/. "$APP_DIR"/
else
  mkdir -p "$APP_DIR"
fi
chmod +x "$APP_DIR/CodyFormat"

# Create a command-line launcher without requiring /usr/local/bin access.
cat > "$HOME/.local/bin/codyformat" <<LAUNCHER
#!/usr/bin/env bash
exec "$APP_DIR/CodyFormat" "\$@"
LAUNCHER
chmod +x "$HOME/.local/bin/codyformat"

# Install freedesktop hicolor icons so GNOME/KDE can resolve Icon=codyformat.
for size in 16 24 32 48 64 128 256 512; do
  src="$SOURCE_DIR/desktop-integration/icons/hicolor/${size}x${size}/apps/codyformat.png"
  dst="$ICON_ROOT/${size}x${size}/apps"
  if [[ -f "$src" ]]; then
    mkdir -p "$dst"
    cp "$src" "$dst/codyformat.png"
  fi
done

mkdir -p "$HOME/.local/share/pixmaps"
if [[ -f "$SOURCE_DIR/desktop-integration/pixmaps/codyformat.png" ]]; then
  cp "$SOURCE_DIR/desktop-integration/pixmaps/codyformat.png" "$HOME/.local/share/pixmaps/codyformat.png"
fi

cat > "$DESKTOP_DIR/codyformat.desktop" <<DESKTOP
[Desktop Entry]
Name=CodyFormat
GenericName=Code Formatter
Comment=Offline code formatter and syntax highlighter
Exec=$APP_DIR/CodyFormat
Icon=codyformat
StartupWMClass=CodyFormat
Terminal=false
Type=Application
Categories=Development;Utility;TextEditor;
Keywords=code;formatter;syntax;sql;developer;
StartupNotify=true
DESKTOP
chmod 644 "$DESKTOP_DIR/codyformat.desktop"

if command -v update-desktop-database >/dev/null 2>&1; then
  update-desktop-database "$DESKTOP_DIR" >/dev/null 2>&1 || true
fi

# Refresh the current user's icon cache when GTK utilities are available.
# This is best-effort and deliberately does not require sudo.
if command -v gtk-update-icon-cache >/dev/null 2>&1; then
  gtk-update-icon-cache -f -t "$ICON_ROOT" >/dev/null 2>&1 || true
fi

printf '\nCodyFormat was installed for the current user.\n'
printf 'Application: %s\n' "$APP_DIR/CodyFormat"
printf 'Desktop entry: %s\n' "$DESKTOP_DIR/codyformat.desktop"
printf 'Run from terminal: %s\n\n' "$HOME/.local/bin/codyformat"
printf 'If GNOME already had CodyFormat pinned, remove and re-pin it so the new icon is used.\n'

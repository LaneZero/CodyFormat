#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
DESKTOP_PROJECT="$ROOT/src/CodyFormat.Desktop/CodyFormat.Desktop.csproj"
CLI_PROJECT="$ROOT/src/CodyFormat.Cli/CodyFormat.Cli.csproj"
VERSION="$(tr -d '\r\n' < "$ROOT/VERSION.txt")"
ARTIFACTS="$ROOT/artifacts"

mkdir -p "$ARTIFACTS"

for RID in osx-arm64 osx-x64; do
  PUBLISH="$ARTIFACTS/publish/$RID"
  CLI_PUBLISH="$ARTIFACTS/publish-cli/$RID"
  APP_DIR="$ARTIFACTS/$RID"
  APP="$APP_DIR/CodyFormat.app"
  PACKAGE_DIR="$ARTIFACTS/package/$RID"

  case "$RID" in
    osx-arm64) PUBLIC_ARCH="arm64" ;;
    osx-x64) PUBLIC_ARCH="x64" ;;
    *) echo "Unsupported RID: $RID" >&2; exit 1 ;;
  esac

  ARCHIVE="$ARTIFACTS/CodyFormat-v${VERSION}-macos-${PUBLIC_ARCH}.zip"

  echo "Publishing desktop $RID..."
  rm -rf "$PUBLISH" "$CLI_PUBLISH" "$APP_DIR" "$PACKAGE_DIR"
  rm -f "$ARCHIVE"

  dotnet publish "$DESKTOP_PROJECT" -c Release -r "$RID" --self-contained true -p:UseAppHost=true -o "$PUBLISH"
  dotnet publish "$CLI_PROJECT" -c Release -r "$RID" --self-contained true -p:UseAppHost=true -o "$CLI_PUBLISH"

  mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
  cp -a "$PUBLISH/." "$APP/Contents/MacOS/"
  cp "$ROOT/packaging/macos/Info.plist" "$APP/Contents/Info.plist"
  cp "$ROOT/packaging/macos/CodyFormat.icns" "$APP/Contents/Resources/CodyFormat.icns"
  chmod +x "$APP/Contents/MacOS/CodyFormat" "$CLI_PUBLISH/codyformat"

  mkdir -p "$PACKAGE_DIR/cli"
  cp -a "$APP" "$PACKAGE_DIR/CodyFormat.app"
  cp -a "$CLI_PUBLISH/." "$PACKAGE_DIR/cli/"
  chmod +x "$PACKAGE_DIR/cli/codyformat"

  echo "Packaging macOS $PUBLIC_ARCH..."
  if command -v ditto >/dev/null 2>&1; then
    ditto -c -k --sequesterRsrc "$PACKAGE_DIR" "$ARCHIVE"
  else
    (cd "$PACKAGE_DIR" && zip -qry "$ARCHIVE" "CodyFormat.app" "cli")
  fi

  test -f "$ARCHIVE" || { echo "Failed to create $ARCHIVE" >&2; exit 1; }
  echo "Created $ARCHIVE"
done

echo "macOS packages created. CodyFormat.app and the self-contained CLI under cli/ are included."

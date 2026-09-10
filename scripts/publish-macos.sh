#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJECT="$ROOT/src/CodyFormat.Desktop/CodyFormat.Desktop.csproj"
VERSION="$(tr -d '\r\n' < "$ROOT/VERSION.txt")"
ARTIFACTS="$ROOT/artifacts"

mkdir -p "$ARTIFACTS"

for RID in osx-arm64 osx-x64; do
  PUBLISH="$ARTIFACTS/publish/$RID"
  APP_DIR="$ARTIFACTS/$RID"
  APP="$APP_DIR/CodyFormat.app"

  case "$RID" in
    osx-arm64) PUBLIC_ARCH="arm64" ;;
    osx-x64) PUBLIC_ARCH="x64" ;;
    *) echo "Unsupported RID: $RID" >&2; exit 1 ;;
  esac

  ARCHIVE="$ARTIFACTS/CodyFormat-v${VERSION}-macos-${PUBLIC_ARCH}.zip"

  echo "Publishing $RID..."
  rm -rf "$PUBLISH" "$APP_DIR"
  rm -f "$ARCHIVE"

  dotnet publish "$PROJECT" \
    -c Release -r "$RID" --self-contained true -p:UseAppHost=true -o "$PUBLISH"

  mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
  cp -a "$PUBLISH/." "$APP/Contents/MacOS/"
  cp "$ROOT/packaging/macos/Info.plist" "$APP/Contents/Info.plist"
  cp "$ROOT/packaging/macos/CodyFormat.icns" "$APP/Contents/Resources/CodyFormat.icns"
  chmod +x "$APP/Contents/MacOS/CodyFormat"

  echo "Packaging macOS $PUBLIC_ARCH..."
  if command -v ditto >/dev/null 2>&1; then
    ditto -c -k --sequesterRsrc --keepParent "$APP" "$ARCHIVE"
  else
    # Fallback for non-macOS build hosts. Public Mac releases should normally be packaged on macOS.
    (cd "$APP_DIR" && zip -qry "$ARCHIVE" "CodyFormat.app")
  fi

  test -f "$ARCHIVE" || { echo "Failed to create $ARCHIVE" >&2; exit 1; }
  echo "Created $ARCHIVE"
done

echo "macOS .app bundles and release ZIPs created under artifacts/."
echo "Apple Silicon output: artifacts/CodyFormat-v${VERSION}-macos-arm64.zip"
echo "Intel output: artifacts/CodyFormat-v${VERSION}-macos-x64.zip"

#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PROJECT="$ROOT/src/CodyFormat.Desktop/CodyFormat.Desktop.csproj"
for RID in osx-arm64 osx-x64; do
  PUBLISH="$ROOT/artifacts/publish/$RID"
  APP="$ROOT/artifacts/$RID/CodyFormat.app"
  rm -rf "$PUBLISH" "$APP"
  dotnet publish "$PROJECT" -c Release -r "$RID" --self-contained true -p:UseAppHost=true -o "$PUBLISH"
  mkdir -p "$APP/Contents/MacOS" "$APP/Contents/Resources"
  cp -a "$PUBLISH/." "$APP/Contents/MacOS/"
  cp "$ROOT/packaging/macos/Info.plist" "$APP/Contents/Info.plist"
  cp "$ROOT/packaging/macos/CodyFormat.icns" "$APP/Contents/Resources/CodyFormat.icns"
  chmod +x "$APP/Contents/MacOS/CodyFormat"
done
echo "macOS .app bundles created under artifacts/osx-*/"

# CodyFormat Release Guide

This guide describes the maintainer workflow for the **v0.6.0 Stable** cross-platform release line and later releases.

> For the one-time promotion from the old GitHub `main` branch to the stable Avalonia codebase, follow [`docs/GIT-FINALIZE-v0.6.0.md`](docs/GIT-FINALIZE-v0.6.0.md) before using the normal release workflow below.

## 1. Confirm the release version

The canonical version is stored in:

```text
VERSION.txt
```

Current stable version:

```text
0.6.0
```

Before tagging, keep these values consistent:

- `VERSION.txt`
- `src/CodyFormat.Core/AppInfo.cs`
- `src/CodyFormat.Desktop/CodyFormat.Desktop.csproj`
- visible version text in the main window
- macOS `Info.plist`
- `README.md`
- `CHANGELOG.md`
- `RELEASE_NOTES.md`
- release asset filenames

---

## 2. Start from a clean worktree

```bash
git status
```

Do not release from a worktree containing unexplained local changes.

---

## 3. Restore and run smoke tests

```bash
dotnet restore CodyFormat.sln
```

```bash
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release
```

Required result:

```text
CodyFormat smoke tests PASS
```

---

## 4. Build Windows packages

From Windows PowerShell:

```powershell
.\scripts\publish-windows.ps1
```

Expected assets:

```text
artifacts/CodyFormat-v0.6.0-win-x64.zip
artifacts/CodyFormat-v0.6.0-win-arm64.zip
```

Validation status for the current Stable baseline:

- Windows x64: tested
- Windows ARM64: build available; broader hardware validation pending

---

## 5. Build Linux packages

From Linux:

```bash
chmod +x scripts/*.sh
./scripts/publish-linux.sh
```

Expected assets:

```text
artifacts/CodyFormat-v0.6.0-linux-x64.tar.gz
artifacts/CodyFormat-v0.6.0-linux-arm64.tar.gz
```

The archives include portable execution plus current-user desktop integration scripts.

Validation status:

- Ubuntu Linux x64: tested
- Linux ARM64: build available; broader hardware validation pending

---

## 6. Build macOS packages

Run on a Mac with the .NET 10 SDK and Xcode Command Line Tools installed:

```bash
chmod +x scripts/*.sh
./scripts/publish-macos.sh
```

Expected application bundles:

```text
artifacts/osx-arm64/CodyFormat.app
artifacts/osx-x64/CodyFormat.app
```

Expected release archives:

```text
artifacts/CodyFormat-v0.6.0-macos-arm64.zip
artifacts/CodyFormat-v0.6.0-macos-x64.zip
```

Validation status:

- **macOS Apple Silicon ARM64: tested successfully on a real Apple M1 Mac**
- macOS Intel x64: build available; real Intel hardware validation pending

### Recommended public macOS release hardening

For broad public distribution, sign the final `.app` with a **Developer ID Application** certificate, enable the Hardened Runtime, submit it to Apple's notarization service, staple the accepted ticket, and only then create the final GitHub ZIP.

Typical verification commands:

```bash
codesign --verify --deep --strict --verbose=2 artifacts/osx-arm64/CodyFormat.app
```

```bash
spctl --assess --type execute --verbose=4 artifacts/osx-arm64/CodyFormat.app
```

If notarizing manually, use `xcrun notarytool`, not the retired `altool` workflow.

---

## 7. Final runtime test matrix

### Windows x64

- extract the ZIP into a clean folder
- launch `CodyFormat.exe`
- test formatting
- test Copy for Word
- test Copy Telegram
- export DOCX
- check icon and theme persistence

### Ubuntu Linux x64

- extract the TAR.GZ into a clean folder
- run `./CodyFormat`
- run `./install-user.sh`
- verify launcher/icon
- test formatting
- test Copy for Word
- test Telegram copy
- export DOCX

### macOS Apple Silicon

Use the ARM64 build on the real test machine.

Current reference hardware:

```text
Apple M1 Mac
Architecture: arm64
```

Test:

- launch from `.app`
- icon in Finder/Dock
- Dark/Light theme
- language detection
- formatting
- format selection/preview/undo
- Open File and Drag & Drop
- Copy Plain
- Copy Telegram
- Copy for Word in an office editor
- DOCX export
- Help / Donate / GitHub
- settings persistence

The current Stable baseline passed functional validation on Apple M1.

See `TESTING.md` for the complete matrix.

---

## 8. Collect release archives

For a complete Stable release, the `artifacts/` directory can contain:

```text
artifacts/
├── CodyFormat-v0.6.0-win-x64.zip
├── CodyFormat-v0.6.0-win-arm64.zip
├── CodyFormat-v0.6.0-linux-x64.tar.gz
├── CodyFormat-v0.6.0-linux-arm64.tar.gz
├── CodyFormat-v0.6.0-macos-arm64.zip
└── CodyFormat-v0.6.0-macos-x64.zip
```

Do not upload raw publish directories to GitHub Releases.

---

## 9. Generate SHA-256 checksums

### Windows

```powershell
.\scripts\create-checksums.ps1
```

### Linux / macOS

```bash
./scripts/create-checksums.sh
```

Expected output:

```text
artifacts/SHA256SUMS.txt
```

Confirm each release archive is listed once.

---

## 10. Commit the Stable release

```bash
git add .
git commit -m "release: CodyFormat v0.6.0 stable"
git push
```

---

## 11. Create and push the tag

```bash
git tag -a v0.6.0 -m "CodyFormat v0.6.0 Stable"
git push origin v0.6.0
```

---

## 12. Create the GitHub Release

In GitHub:

```text
Repository
→ Releases
→ Draft a new release
```

Use:

```text
Tag: v0.6.0
Title: CodyFormat v0.6.0
```

For this Stable baseline:

```text
Pre-release: OFF
Set as latest release: ON
```

---

## 13. Upload release assets

Upload:

```text
CodyFormat-v0.6.0-win-x64.zip
CodyFormat-v0.6.0-win-arm64.zip
CodyFormat-v0.6.0-linux-x64.tar.gz
CodyFormat-v0.6.0-linux-arm64.tar.gz
CodyFormat-v0.6.0-macos-arm64.zip
CodyFormat-v0.6.0-macos-x64.zip
SHA256SUMS.txt
```

If an architecture has not received full real-hardware validation, keep that status explicit in the Release Notes even when the build is published.

GitHub automatically adds:

```text
Source code (zip)
Source code (tar.gz)
```

Do not upload a duplicate source ZIP unless there is a specific distribution reason.

---

## 14. Use RELEASE_NOTES.md

Copy `RELEASE_NOTES.md` into the GitHub Release description and update only details that changed during the final release run.

---

## 15. Post-release verification

Download the assets from GitHub itself and verify:

- filenames are correct
- checksums match
- archives open successfully
- tested-platform packages launch
- README download names match the actual Release assets

For macOS ARM64, verify the final downloaded package again on the Apple M1 test machine if the package was changed by signing/notarization after functional testing.

---

## 16. Starting the next version

Do not develop new features directly on the Stable baseline.

Recommended next branch:

```text
feature/v0.7.0
```

or:

```text
develop/v0.7.0
```

The planned scope is documented in [`ROADMAP.md`](ROADMAP.md).

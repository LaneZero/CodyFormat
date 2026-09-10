# CodyFormat v0.6.0 Stable

CodyFormat `v0.6.0` is the first stable cross-platform Avalonia release for Windows, Linux, and macOS.

## Validation status

- ✅ Windows x64: tested successfully
- ✅ Ubuntu Linux x64: tested successfully
- ✅ macOS Apple Silicon ARM64: tested successfully on a real **Apple M1 Mac**
- 🧪 Windows ARM64: build available, broader hardware validation pending
- 🧪 Linux ARM64: build available, broader hardware validation pending
- 🧪 macOS Intel x64: build available, real Intel hardware validation pending

This release is the new Stable baseline for CodyFormat.

## Release assets

Upload these files to the GitHub Release:

```text
CodyFormat-v0.6.0-win-x64.zip
CodyFormat-v0.6.0-win-arm64.zip
CodyFormat-v0.6.0-linux-x64.tar.gz
CodyFormat-v0.6.0-linux-arm64.tar.gz
CodyFormat-v0.6.0-macos-arm64.zip
CodyFormat-v0.6.0-macos-x64.zip
SHA256SUMS.txt
```

GitHub automatically adds:

```text
Source code (zip)
Source code (tar.gz)
```

## Highlights

- One Avalonia codebase targeting Windows, Linux, and macOS
- Fully offline source-code formatting and highlighting
- Language-aware formatting with protected-syntax safety guards
- Automatic language detection
- Dark and Light themes
- Format document / selection / preview / undo workflow
- Native Windows rich clipboard support for Microsoft Word
- Native Linux X11/XWayland rich clipboard support with fallback
- macOS Apple Silicon runtime validation on Apple M1
- Telegram-compatible fenced-code copy
- Editable DOCX export
- No Background mode
- Formatting profiles and typography controls
- Cross-platform CodyFormat application icon
- Linux user-local desktop integration without sudo
- Release-ready Windows/Linux/macOS packaging scripts
- SHA-256 release verification
- No telemetry, account, or cloud formatting service

## Major fixes completed during the preview cycle

### Windows rich clipboard

CodyFormat uses a native Win32 clipboard backend and registers:

- `HTML Format`
- `Rich Text Format`
- `CF_UNICODETEXT`

This fixed the regression where Word could paste visible raw RTF control data instead of formatted code.

### Linux rich clipboard

CodyFormat provides native rich clipboard targets on X11/XWayland and retains a cross-platform fallback.

### macOS

The Apple Silicon build was produced and functionally tested on a real M1 Mac. The release process supports `.app` packaging and release ZIP generation.

For broad public macOS distribution, Developer ID signing and Apple notarization are recommended before publishing the final Mac archive.

## Windows installation

Most Windows computers should use:

```text
CodyFormat-v0.6.0-win-x64.zip
```

Extract and run:

```text
CodyFormat.exe
```

The package is self-contained.

## Linux installation

Most Intel/AMD Linux computers should use:

```text
CodyFormat-v0.6.0-linux-x64.tar.gz
```

```bash
tar -xzf CodyFormat-v0.6.0-linux-x64.tar.gz
cd linux-x64
chmod +x CodyFormat
./CodyFormat
```

Optional current-user desktop integration:

```bash
chmod +x install-user.sh uninstall-user.sh
./install-user.sh
```

No sudo access is required for the user-local installer.

## macOS Apple Silicon installation

For Apple Silicon Macs, including the tested M1 platform, use:

```text
CodyFormat-v0.6.0-macos-arm64.zip
```

Extract and open:

```text
CodyFormat.app
```

Or:

```bash
open CodyFormat.app
```

## Verify downloads

Windows example:

```powershell
Get-FileHash .\CodyFormat-v0.6.0-win-x64.zip -Algorithm SHA256
```

Linux example:

```bash
sha256sum CodyFormat-v0.6.0-linux-x64.tar.gz
```

macOS example:

```bash
shasum -a 256 CodyFormat-v0.6.0-macos-arm64.zip
```

Compare with `SHA256SUMS.txt`.

## Next versions

Development after v0.6.0 Stable follows the versioned roadmap:

- `v0.7.0` — CLI, batch formatting, `.editorconfig`, native formatter providers, editor workflow
- `v0.8.0` — PNG/SVG, Markdown/HTML output, themes, diff, safety report, utility tools
- `v0.9.0` — command palette, OS integration, portable/session/large-file workflows
- `v1.0.0` — stable public contracts, accessibility, full regression and release automation hardening

See `ROADMAP.md` for the full scope.

## Privacy

CodyFormat formats and highlights code locally. It does not upload source code, require an account, or send telemetry.

## Developer

**LaneZeroO**
https://github.com/LaneZero/CodyFormat

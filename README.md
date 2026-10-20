# CodyFormat

**CodyFormat** is a lightweight, privacy-first, offline-first code formatter, syntax highlighter, and export tool for developers who need clean code in documentation, Microsoft Word, Telegram, reports, presentations, and day-to-day development work.

CodyFormat processes source code locally. It does **not** upload your code, require an account, use telemetry, or depend on an online formatting service.

> **Current stable release:** `v0.6.0`
>
> **Current development build:** `v0.7.0-beta.1` adds opt-in native formatter providers on top of the validated CLI, batch, `.editorconfig`, and Profiles v2 work. It does **not** replace the v0.6.0 Stable release.
>
> Runtime validation completed on **Windows x64**, **Ubuntu Linux x64**, and **macOS Apple Silicon on a real Apple M1 Mac**. Windows ARM64, Linux ARM64, and macOS Intel packages can also be published, but remain architecture-specific validation targets until they receive equivalent real-hardware coverage.

---

## Download the right release

All ready-to-run builds are published on the **GitHub Releases** page.

| Platform | Architecture | Release asset | Validation status |
|---|---|---|---|
| Windows | x64 / Intel & AMD | `CodyFormat-v0.6.0-win-x64.zip` | ✅ Tested |
| Windows | ARM64 | `CodyFormat-v0.6.0-win-arm64.zip` | 🧪 Build available / broader hardware validation pending |
| Linux | x64 / Intel & AMD | `CodyFormat-v0.6.0-linux-x64.tar.gz` | ✅ Tested on Ubuntu |
| Linux | ARM64 | `CodyFormat-v0.6.0-linux-arm64.tar.gz` | 🧪 Build available / broader hardware validation pending |
| macOS | Apple Silicon / ARM64 | `CodyFormat-v0.6.0-macos-arm64.zip` | ✅ Tested on Apple M1 |
| macOS | Intel x64 | `CodyFormat-v0.6.0-macos-x64.zip` | 🧪 Build available / Intel hardware validation pending |

The release also includes `SHA256SUMS.txt` so downloaded packages can be verified before use.

GitHub automatically provides **Source code (zip)** and **Source code (tar.gz)** for every tag. Those source archives are intended for developers. Normal users should download the platform package from the table above.

---

## Install and run a GitHub Release

### Windows x64

Download:

```text
CodyFormat-v0.6.0-win-x64.zip
```

Extract it to a normal writable folder, for example:

```text
C:\Tools\CodyFormat\
```

Run:

```text
CodyFormat.exe
```

The Windows package is **self-contained**. A separate .NET runtime is not required.

If Windows marks downloaded files as blocked, open the ZIP properties before extraction and use **Unblock** when that option is available.

### Windows ARM64

Use this package on Windows ARM64 devices:

```text
CodyFormat-v0.6.0-win-arm64.zip
```

Installation is otherwise identical to Windows x64. This package is available in the stable release set, but broader ARM64 hardware validation is still encouraged.

### Linux x64

Download:

```text
CodyFormat-v0.6.0-linux-x64.tar.gz
```

Extract it:

```bash
tar -xzf CodyFormat-v0.6.0-linux-x64.tar.gz
cd linux-x64
```

Run the portable application:

```bash
chmod +x CodyFormat
./CodyFormat
```

For application-menu, launcher, and icon integration without `sudo`:

```bash
chmod +x install-user.sh uninstall-user.sh
./install-user.sh
```

To remove the user-local integration later:

```bash
./uninstall-user.sh
```

### Linux ARM64

On ARM64 Linux systems use:

```text
CodyFormat-v0.6.0-linux-arm64.tar.gz
```

Then follow the same extraction and installation steps as Linux x64.

### macOS Apple Silicon

The Apple Silicon build has been **tested successfully on a real Apple M1 Mac**.

Download:

```text
CodyFormat-v0.6.0-macos-arm64.zip
```

Extract the archive and open:

```text
CodyFormat.app
```

From Terminal you can also run:

```bash
open CodyFormat.app
```

The public macOS package should be distributed with Developer ID signing and Apple notarization when those release credentials are available. Functional runtime validation, formatting, clipboard/export workflows, and application behavior were tested on Apple M1 hardware.

### macOS Intel

Intel Macs use:

```text
CodyFormat-v0.6.0-macos-x64.zip
```

The x64 target is built from the same codebase. Until it receives equivalent real Intel Mac validation, treat it as an architecture-specific compatibility build.

---

## Verify a downloaded release

Download `SHA256SUMS.txt` from the same GitHub Release.

### Windows PowerShell

```powershell
Get-FileHash .\CodyFormat-v0.6.0-win-x64.zip -Algorithm SHA256
```

Compare the returned hash with the matching line in `SHA256SUMS.txt`.

### Linux

```bash
sha256sum CodyFormat-v0.6.0-linux-x64.tar.gz
```

Or:

```bash
sha256sum -c SHA256SUMS.txt --ignore-missing
```

### macOS

```bash
shasum -a 256 CodyFormat-v0.6.0-macos-arm64.zip
```

Compare the result with `SHA256SUMS.txt`.

---

## Features

- Fully offline formatting and syntax highlighting
- No account, telemetry, or cloud processing
- Automatic language detection
- Language-aware formatter families instead of one generic whitespace formatter
- Format the entire document or only selected code
- Review formatting before applying changes
- Undo support
- Dark and Light themes
- 2-space and 4-space indentation
- Custom code-block backgrounds
- True **No Background** mode
- Optional line numbers and border
- Font and font-size controls
- Open File and Drag & Drop
- Copy Plain Text
- Copy formatted code for Microsoft Word / compatible office editors
- Native Windows rich clipboard support
- Native Linux X11/XWayland rich clipboard support with fallback
- Cross-platform rich clipboard support on macOS
- Copy Telegram-compatible fenced code
- Editable `.docx` export
- Formatting profiles for documentation and presentation use
- Built-in Help and Donate pages
- Cross-platform application icon and desktop integration
- Manual **Check Updates** action using GitHub Releases (no automatic/background checks)

---

## Supported languages

CodyFormat currently supports these language selections:

- C#
- SQL
- JavaScript
- TypeScript
- Python
- JSON
- XML
- HTML
- CSS
- PowerShell
- Bash
- Rust
- Go
- PHP
- Java
- C
- C++
- Kotlin
- Swift
- Dart
- Ruby
- Lua
- R
- Perl
- YAML
- Markdown
- Objective-C
- Scala
- Groovy
- Plain Text

Formatting is intentionally conservative where syntax is whitespace-sensitive or ambiguous. CodyFormat prefers preserving valid source code over aggressively rewriting it for appearance alone.

---

## Formatting engine

CodyFormat does not apply one generic whitespace algorithm to every language.

Different formatter families are used for:

- JSON and XML structural formatting
- SQL clause-aware formatting
- C-style and brace-based languages
- HTML and CSS
- Bash, Ruby, and Lua block structures
- Python and YAML whitespace-sensitive input
- Raw strings, heredocs, template literals, multiline strings, and other protected syntax

Safety rules protect syntax such as Rust lifetimes, JavaScript template literals, C# raw strings, PHP/Perl heredocs, PowerShell here-strings, YAML block scalars, and similar constructs.

---

## Copy for Word

CodyFormat creates multiple representations of the same highlighted code so office applications can choose the richest supported clipboard format.

### Windows

The native Windows clipboard backend publishes:

- `HTML Format` / CF_HTML
- `Rich Text Format` / RTF
- `CF_UNICODETEXT` fallback

This prevents RTF control text from being pasted visibly into Word and improves compatibility with Microsoft Word and clipboard helpers.

### Linux

On X11/XWayland CodyFormat provides native rich clipboard targets including HTML, RTF, UTF-8 text, and plain-text fallbacks. On other Linux clipboard backends, the cross-platform clipboard implementation is used as a fallback.

### macOS

The Apple Silicon build has been tested on an Apple M1 Mac. Rich-copy behavior should still be rechecked whenever clipboard/export code changes because destination applications can choose or sanitize clipboard formats differently.

`Export DOCX` remains the deterministic output path when an office application strips clipboard styling.

---

## Copy for Telegram

CodyFormat can copy source code using Telegram-compatible fenced code blocks.

Example SQL output:

````text
```sql
SELECT
    id,
    username
FROM users;
```
````

The Markdown language identifier is mapped automatically, for example:

```text
C#         -> csharp
C++        -> cpp
JavaScript -> javascript
TypeScript -> typescript
PowerShell -> powershell
Rust       -> rust
Go         -> go
SQL        -> sql
```

When **Format before output** is enabled, Telegram output uses formatted code without modifying the original editor content.

---

## DOCX export

CodyFormat can generate an editable `.docx` document while preserving:

- syntax colors
- indentation
- selected background
- no-background mode
- line numbering preferences
- font settings
- border behavior

DOCX generation is independent of the operating-system clipboard and is the recommended fallback when a destination application strips rich clipboard formatting.

---

## Keyboard shortcuts

| Action | Windows / Linux | macOS |
|---|---|---|
| Open file | `Ctrl+O` | `Cmd+O` |
| Format document | `Shift+Alt+F` | `Shift+Option+F` |
| Alternative format | `Ctrl+Enter` | `Cmd+Enter` |
| Format selection | `Ctrl+Shift+F` | `Cmd+Shift+F` |
| Undo | `Ctrl+Z` | `Cmd+Z` |

---

## Development CLI — v0.7.0-beta.1

The v0.7 development line introduces `CodyFormat.Cli`, which reuses the same `CodyFormat.Core` formatter used by the desktop application.

Initial commands:

```bash
codyformat file.cs
codyformat file.cs --check
codyformat file.cs --write
codyformat query.sql --language sql --indent 4
cat script.py | codyformat --stdin --language python
```

`--check` never writes. `--write` uses a same-directory temporary file and replacement path so formatter failures do not truncate the original source. Beta.1 also adds `--native` and `--native-status`; external formatters are opt-in only and always have a built-in fallback. See [`docs/CLI.md`](docs/CLI.md) and [`docs/NATIVE-FORMATTERS.md`](docs/NATIVE-FORMATTERS.md).

The same development build also introduces a formatter regression matrix for all selectable languages. Python is the priority regression target. See [`docs/FORMATTER-QUALITY.md`](docs/FORMATTER-QUALITY.md).

---

## Build from source

### Requirements

- .NET 10 SDK
- Git
- Internet access for the initial NuGet restore
- Xcode Command Line Tools on macOS

The built application itself does not require an internet connection.

Clone the repository:

```bash
git clone https://github.com/LaneZero/CodyFormat.git
cd CodyFormat
```

Restore:

```bash
dotnet restore CodyFormat.sln
```

Run smoke tests:

```bash
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release
```

Expected result:

```text
CodyFormat smoke tests PASS
```

Run from source:

```bash
dotnet run --project src/CodyFormat.Desktop/CodyFormat.Desktop.csproj
```

---


> Starting with the v0.7 development line, platform release packages also include the self-contained CLI under `cli/`, so the same GitHub-downloaded package can be used to validate both the desktop application and CLI behavior.

## Create release builds

### Windows

From PowerShell:

```powershell
.\scripts\publish-windows.ps1
```

Outputs:

```text
artifacts/CodyFormat-v0.6.0-win-x64.zip
artifacts/CodyFormat-v0.6.0-win-arm64.zip
```

### Linux

```bash
chmod +x scripts/*.sh
./scripts/publish-linux.sh
```

Outputs:

```text
artifacts/CodyFormat-v0.6.0-linux-x64.tar.gz
artifacts/CodyFormat-v0.6.0-linux-arm64.tar.gz
```

### macOS

On macOS:

```bash
chmod +x scripts/*.sh
./scripts/publish-macos.sh
```

Outputs:

```text
artifacts/CodyFormat-v0.6.0-macos-arm64.zip
artifacts/CodyFormat-v0.6.0-macos-x64.zip
```

The `osx-arm64` application bundle has been runtime-tested on Apple M1 hardware.

### Create checksums

Windows:

```powershell
.\scripts\create-checksums.ps1
```

Linux/macOS:

```bash
./scripts/create-checksums.sh
```

Both scripts create:

```text
artifacts/SHA256SUMS.txt
```

For the complete maintainer workflow, see [`RELEASE_GUIDE.md`](RELEASE_GUIDE.md). For the Apple Silicon build/test/sign/notarize workflow, see [`docs/macOS-M1-Build-Test-Release-Guide.md`](docs/macOS-M1-Build-Test-Release-Guide.md).

---

## Stable platform status

| Release | Purpose | Status |
|---|---|---|
| `v0.5.0` | Original WPF Windows baseline | Legacy stable rollback point |
| `v0.6.0-preview.1` → `preview.6` | Avalonia migration and cross-platform hardening | Completed |
| `v0.6.0` | First stable cross-platform baseline | **Current Stable** |

Current validation:

```text
Windows x64      PASS
Ubuntu Linux x64 PASS
macOS ARM64 M1   PASS
Windows ARM64    build available / broader hardware validation pending
Linux ARM64      build available / broader hardware validation pending
macOS x64 Intel  build available / real Intel hardware validation pending
```

The Stable baseline must not be modified directly for experimental features. New development belongs in a new branch/version line.

---

## Roadmap

`v0.6.0` is the current Stable baseline. Feature development continues on separate version branches; the Stable line is reserved for maintenance fixes.

### v0.7.0 — Developer Workflow

- CLI with file, stdin/stdout, `--check`, and safe `--write`
- Batch/folder formatting with dry-run and include/exclude rules
- `.editorconfig` resolution shared by GUI and CLI
- Profiles v2 with import/export and migration
- Optional local formatter providers with built-in offline fallback
- SQL dialect-aware formatting
- Find / Replace

Development is staged through `alpha → beta → rc → stable` with explicit safety and cross-platform release gates.

### v0.8.0 — Export & Presentation

- PNG/SVG export
- Markdown/HTML output
- Advanced Diff
- Formatting Safety Report
- Theme Editor and compatible VS Code theme import
- Explicit Beautify/Minify utilities
- Expanded JSON/XML tools

### v0.9.0 — Desktop Productivity & Scale

- Command Palette
- OS integration
- Portable Mode
- privacy-aware Session Recovery
- Large File Mode
- performance statistics
- Project Mode
- structure-aware formatting where reliable parsing is available

### v1.0.0 — Stable Toolchain Contract

- stable CLI/provider/profile/theme contracts
- full cross-platform regression suite
- accessibility completion
- hardened packaging/signing automation
- performance and memory gates
- compatibility and upgrade policy

See [`ROADMAP.md`](ROADMAP.md) for the complete release train and release gates.

For the detailed implementation sequence of the next version, see [`docs/v0.7.0-DEVELOPMENT-PLAN.md`](docs/v0.7.0-DEVELOPMENT-PLAN.md).

---

### Batch / folder formatting — alpha.2

Batch mode is non-destructive unless `--write` is explicitly supplied.

```bash
codyformat ./src --recursive --check
codyformat ./src --recursive --write
codyformat ./src --recursive --write --backup
codyformat . --recursive --check --include "**/*.cs" --exclude "**/Generated/**"
```

Default excluded directories:

```text
.git
bin
obj
artifacts
TestResults
node_modules
```

`--include` and `--exclude` can be repeated. Recursive directory symlinks/reparse points are not followed.

For every Alpha/Beta/RC/Stable milestone, final acceptance requires downloading the published GitHub Release asset again, verifying SHA-256, extracting it into a fresh location, and running the runtime smoke test on those downloaded bytes.

See [`docs/RELEASE-ASSET-VERIFICATION.md`](docs/RELEASE-ASSET-VERIFICATION.md).



### Project configuration — alpha.3

CodyFormat now resolves `.editorconfig` locally for file and batch formatting. Supported rules include indentation style/size, tab width, line endings, final newline, and trailing whitespace. Use `--no-editorconfig` for an explicit one-operation opt-out.

Profiles v2 add versioned JSON import/export and project profiles at `.codyformat/profile.json`. See [`docs/EDITORCONFIG.md`](docs/EDITORCONFIG.md) and [`docs/PROFILES-V2.md`](docs/PROFILES-V2.md).

## Project structure

```text
CodyFormat.sln

src/
├── CodyFormat.Core/
├── CodyFormat.Desktop/
├── CodyFormat.Cli/
└── CodyFormat.SmokeTests/

packaging/
├── linux/
└── macos/

scripts/
├── publish-windows.ps1
├── publish-linux.sh
├── publish-macos.sh
├── smoke-test.sh
├── validate-alpha2.ps1
├── validate-alpha2.sh
├── create-checksums.ps1
└── create-checksums.sh
```

`CodyFormat.Core` intentionally remains independent of WPF and platform-specific UI code.

---

## Privacy and security

CodyFormat treats pasted or opened source code as local text.

It does not intentionally:

- upload source code
- execute pasted source code
- run SQL queries
- execute shell or PowerShell commands
- call a cloud formatter
- send analytics or telemetry
- require an account


### Network behavior

Normal formatting, highlighting, clipboard/export, CLI, and batch operations do not require an internet connection.

CodyFormat does **not** perform automatic or background update checks. The only update request is made when the user explicitly clicks **Check Updates**. That action queries the public GitHub Releases API for `LaneZero/CodyFormat` and does not upload source code or editor contents.

Source code is not stored in the application settings file.

Please remove passwords, API keys, tokens, private URLs, and confidential information before posting bug examples publicly.

See [`SECURITY.md`](SECURITY.md) for security-reporting guidance.

---

## Reporting formatter bugs

A useful formatter bug report should include:

```text
CodyFormat version:
Operating system:
Architecture:
Language:
Original code:
Actual formatted output:
Expected output:
```

Please use minimal, non-confidential code samples.

See [`CONTRIBUTING.md`](CONTRIBUTING.md) for contribution guidelines.

---

## Migration status

The Avalonia migration reached the **v0.6.0 Stable** baseline after successful functional validation on Windows x64, Ubuntu Linux x64, and macOS Apple Silicon on an Apple M1 Mac.

The WPF `v0.5.0` release remains preserved as a historical rollback baseline, but new development should target the cross-platform codebase.

See:

- [`MIGRATION.md`](MIGRATION.md)
- [`TESTING.md`](TESTING.md)
- [`CHANGELOG.md`](CHANGELOG.md)
- [`ROADMAP.md`](ROADMAP.md)

---

## Developer

**LaneZeroO**

GitHub: https://github.com/LaneZero  
Project: https://github.com/LaneZero/CodyFormat

---

## Support CodyFormat

CodyFormat is free software. If it is useful to you, optional donations help continued development, bug fixes, testing, and documentation.

### Bitcoin

**Network:** Bitcoin

```text
bc1q5tl36qpk27hf7upl8l753xa0gcm57adrvmwgkz
```

### Tether

**Network:** TRON / TRC20

```text
TA5pibChqS7CeHiDvfP7V3uQVMynk6SxLq
```

### EVM Networks

**Supported networks:** Ethereum, BNB Smart Chain, Polygon

```text
0x6634E26BA0e323182B7A9a89278E9a7BbCa9aF70
```

### Solana

**Network:** Solana

```text
91kuBFEZAnRk8ixsuzAQnAVvtzLAsQpPFRsfe285ZKUC
```

> **Important:** Always verify the complete wallet address and blockchain network before transferring cryptocurrency. Cryptocurrency transactions generally cannot be reversed.

---

## License

CodyFormat is distributed under the [MIT License](LICENSE).

If CodyFormat saves you time, starring the repository helps other developers discover the project.

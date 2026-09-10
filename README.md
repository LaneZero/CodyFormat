# CodyFormat

**CodyFormat** is a lightweight, privacy-first, fully offline code formatter, syntax highlighter, and export tool for developers who need clean code in documentation, Microsoft Word, Telegram, reports, presentations, and day-to-day development work.

CodyFormat processes source code locally. It does **not** upload your code, require an account, use telemetry, or depend on an online formatting service.

> **Current stable release:** `v0.6.0`
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

For the complete maintainer workflow, see [`RELEASE_GUIDE.md`](RELEASE_GUIDE.md). For the one-time GitHub promotion of the stable `v0.6.0` codebase, see [`docs/GIT-FINALIZE-v0.6.0.md`](docs/GIT-FINALIZE-v0.6.0.md). For the Apple Silicon build/test/sign/notarize workflow, see [`docs/macOS-M1-Build-Test-Release-Guide.md`](docs/macOS-M1-Build-Test-Release-Guide.md).

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

Future development is intentionally split into versioned milestones so Stable does not become a moving target.

### v0.7.0 — Developer workflow

- CLI mode
- Batch / folder formatting
- `.editorconfig` support
- Native formatter-provider integration with safe built-in fallback
- Expanded project/profile workflow
- SQL dialect-aware formatting improvements
- Find / Replace and practical editor improvements

### v0.8.0 — Export and presentation

- Export code as PNG
- Export code as SVG
- Copy as Markdown
- Copy as HTML
- Advanced before/after diff
- Formatting safety report
- Custom theme editor
- Import compatible VS Code color themes
- Beautify / Minify tools where syntax permits
- Expanded JSON / XML utilities

### v0.9.0 — Desktop productivity and scale

- Command Palette
- OS file-context integration
- Portable mode
- Session recovery with privacy controls
- Large File Mode
- Performance statistics
- Current block/function/class formatting where safe parsing is available
- Further project-mode improvements

### v1.0.0 — Long-term stable toolchain

- Stable Windows, Linux, and macOS desktop release line
- Stable CLI behavior
- Stable formatter/provider contracts
- Stable profiles and themes
- Full regression suite across supported platforms
- Accessibility and keyboard-navigation completion
- Packaging/signing/release automation hardening
- Compatibility policy and documented upgrade guarantees

See [`ROADMAP.md`](ROADMAP.md) for scope, non-goals, and release gates.

---

## Project structure

```text
CodyFormat.sln

src/
├── CodyFormat.Core/
├── CodyFormat.Desktop/
└── CodyFormat.SmokeTests/

packaging/
├── linux/
└── macos/

scripts/
├── publish-windows.ps1
├── publish-linux.sh
├── publish-macos.sh
├── smoke-test.sh
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

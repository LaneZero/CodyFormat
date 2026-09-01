# CodyFormat

CodyFormat is a lightweight, privacy-first code formatter, syntax highlighter and export tool that works locally without sending source code to a server.

> **Cross-platform migration preview:** `v0.6.0-preview.3`
>
> The known-stable Windows release remains **CodyFormat v0.5.0 (WPF)**. This preview starts the move to a single Avalonia codebase for Windows, macOS and Linux. Do not replace the v0.5.0 Windows release until this preview passes platform testing.

## Target platforms

- Windows x64 / ARM64
- macOS Apple Silicon (ARM64)
- macOS Intel (x64)
- Linux x64 / ARM64

## What is already shared across all desktop platforms

- Code formatting engine
- Language detection
- Syntax highlighting rules
- Dark / Light code themes
- Formatting profiles
- Source-file validation and extension detection
- Telegram fenced-code output
- Rich HTML generation for Word-compatible clipboard output
- DOCX generation
- Local settings model

## Supported languages

C#, SQL, JavaScript, TypeScript, Python, JSON, XML, HTML, CSS, PowerShell, Bash, Rust, Go, PHP, Java, C, C++, Kotlin, Swift, Dart, Ruby, Lua, R, Perl, YAML, Markdown, Objective-C, Scala, Groovy and Plain Text.

## Main features

- 100% local formatting and highlighting
- Auto language detection
- Open file and drag & drop
- Format entire document
- Format selected code
- Review formatting before applying it
- Native TextBox undo support
- 2 / 4-space indentation
- Dark / Light UI
- Custom code backgrounds and true no-background mode
- Line numbers and border control
- Font and font-size options
- Copy Plain
- Copy for Telegram (` ```language ` fenced output)
- Rich HTML clipboard output for Word-style paste
- Editable DOCX export
- Help and Donate windows
- No telemetry, account or cloud API

## Architecture

```text
CodyFormat.sln

src/
├── CodyFormat.Core/          # OS-independent formatter/export engine
├── CodyFormat.Desktop/       # Avalonia desktop UI
└── CodyFormat.SmokeTests/    # dependency-free core regression checks

packaging/
├── macos/
└── linux/

scripts/
├── publish-windows.ps1
├── publish-macos.sh
├── publish-linux.sh
└── smoke-test.sh
```

The Core project intentionally has no WPF, Win32, Avalonia or platform-specific UI dependency.

## Requirements for building

- .NET 10 SDK (`global.json` accepts current .NET 10 feature bands)
- Avalonia `12.1.1` (restored from NuGet)
- Internet access during the first restore of Avalonia packages
- No internet connection is required by the built application at runtime

## Run from source

```bash
dotnet restore CodyFormat.sln
dotnet run --project src/CodyFormat.Desktop/CodyFormat.Desktop.csproj
```

## Run smoke tests

```bash
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release
```

The smoke suite checks language detection, Telegram output, JSON/SQL formatting, UTF-16 source decoding, binary-file rejection, settings-path resolution, no-background HTML output and DOCX structure.

## Publish for Windows

PowerShell:

```powershell
./scripts/publish-windows.ps1
```

Outputs:

```text
artifacts/win-x64/
artifacts/win-arm64/
```

## Publish for macOS

Run from macOS or Linux/WSL so executable permissions are preserved:

```bash
./scripts/publish-macos.sh
```

Outputs:

```text
artifacts/osx-arm64/CodyFormat.app
artifacts/osx-x64/CodyFormat.app
```

The script creates the `.app` bundle with `Info.plist`, the CodyFormat `.icns` icon and the self-contained .NET runtime.

For public distribution on macOS, code signing and Apple notarization should be added before calling a release stable.

## Publish for Linux

```bash
./scripts/publish-linux.sh
```

Outputs:

```text
artifacts/linux-x64/
artifacts/linux-arm64/
```

A desktop-entry template is included under `packaging/linux/codyformat.desktop`.

## Clipboard behavior

### Plain / Telegram

Uses Avalonia's native clipboard API on Windows, macOS and Linux.

### Rich copy for Word

CodyFormat generates the same syntax-colored HTML in the shared Core project, then exposes it through the platform's native HTML clipboard format:

- Windows: `HTML Format` / CF_HTML
- macOS: `public.html`
- Linux: `text/html`

This must be compatibility-tested with Microsoft Word on both Windows and macOS before the cross-platform build is promoted to stable.

## Keyboard shortcuts

| Action | Windows / Linux | macOS |
|---|---|---|
| Open file | `Ctrl+O` | `Cmd+O` |
| Format document | `Shift+Alt+F` | `Shift+Option+F` |
| Alternative format | `Ctrl+Enter` | `Cmd+Enter` |
| Format selection | `Ctrl+Shift+F` | `Cmd+Shift+F` |
| Undo | `Ctrl+Z` | `Cmd+Z` |

## Settings and privacy

CodyFormat stores UI preferences locally through `.NET`'s `LocalApplicationData` location. Source code is not written to the settings file.

Typical locations include:

- Windows: `%LOCALAPPDATA%/CodyFormat/`
- macOS: `~/Library/Application Support/CodyFormat/`
- Linux: the user-local application-data directory

The application does not execute pasted code, upload source, use telemetry or call an online formatter.

## GitHub Actions

`.github/workflows/cross-platform-build.yml` runs:

1. Core smoke tests
2. Windows x64 / ARM64 publish
3. Linux x64 / ARM64 publish
4. macOS x64 / ARM64 publish
5. Uploads every build artifact for inspection

This is the first build gate for the migration branch.

## Migration policy

The Windows WPF `v0.5.0` release is frozen as the stable fallback.

The Avalonia branch can become stable only after:

- Windows feature-parity test passes
- Windows rich Word clipboard test passes
- macOS Apple Silicon smoke test passes
- macOS Word clipboard test passes
- macOS `.app` launch/Gatekeeper behavior is verified
- Linux Ubuntu launch and clipboard tests pass
- Formatter regression samples match the stable Windows behavior

See `MIGRATION.md` and `TESTING.md`.

## Developer

**LaneZeroO**

Project: https://github.com/LaneZero/CodyFormat

## Donate

CodyFormat is free. If it is useful to you, an optional donation helps free tools continue to receive maintenance, testing and documentation.

### Bitcoin
Network: Bitcoin

```text
bc1q5tl36qpk27hf7upl8l753xa0gcm57adrvmwgkz
```

### Tether
Network: TRON / TRC20

```text
TA5pibChqS7CeHiDvfP7V3uQVMynk6SxLq
```

### EVM Networks
Ethereum, BNB Smart Chain, Polygon

```text
0x6634E26BA0e323182B7A9a89278E9a7BbCa9aF70
```

### Solana
Network: Solana

```text
91kuBFEZAnRk8ixsuzAQnAVvtzLAsQpPFRsfe285ZKUC
```

> Always verify the complete wallet address and blockchain network before transferring cryptocurrency. Cryptocurrency transactions generally cannot be reversed.

## License

MIT License. See `LICENSE`.

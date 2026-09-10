# Contributing to CodyFormat

Contributions are welcome, especially reproducible formatter fixes, cross-platform compatibility improvements, tests, and documentation corrections.

## Before opening an issue

Search existing issues first and confirm the problem still exists in the latest applicable release.

Do not post confidential company source code, passwords, API keys, access tokens, internal URLs, or customer data.

## Bug reports

Include:

```text
CodyFormat version:
Operating system:
Architecture:
Language:
Original code or minimal reproduction:
Actual behavior:
Expected behavior:
```

For clipboard issues also include the destination application, for example Microsoft Word, LibreOffice Writer, ONLYOFFICE, Telegram Desktop, or another editor.

## Formatter changes

Formatter changes must prioritize correctness over aggressive visual rewriting.

A formatter contribution should:

1. preserve language semantics
2. avoid modifying protected strings/comments/heredocs unnecessarily
3. include a minimal regression sample
4. pass the existing smoke suite
5. avoid network dependencies

## Build

Requirements:

- .NET 10 SDK
- Git

Run:

```bash
dotnet restore CodyFormat.sln
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release
```

## Branches

Create a focused branch:

```bash
git checkout -b fix/sql-indentation
```

Use clear commits, for example:

```text
fix: preserve Rust lifetimes during formatting
feat: add language-specific formatting option
 docs: clarify Linux installation
```

## Pull requests

A pull request should explain:

- what changed
- why it changed
- platforms affected
- formatter languages affected
- manual testing completed

Cross-platform changes should not silently regress the known-good Windows, Ubuntu Linux, or macOS Apple Silicon behavior. The current macOS reference hardware is an Apple M1 Mac.

## Roadmap alignment

The current Stable baseline is `v0.6.0`. New feature work should align with [`ROADMAP.md`](ROADMAP.md) and target the appropriate future version branch rather than modifying Stable directly.

Examples:

```text
feature/v0.7.0-cli
feature/v0.8.0-svg-export
fix/v0.6.1-word-clipboard
```

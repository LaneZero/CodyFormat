# CodyFormat Cross-Platform Migration Status

## Status: Completed for v0.6.0 Stable

The migration from the Windows-only WPF baseline to the shared Avalonia desktop codebase reached its first stable milestone in **CodyFormat v0.6.0**.

## Stable baseline

The cross-platform implementation is now the primary development base.

Validated environments:

- Windows x64 — functional validation passed
- Ubuntu Linux x64 — functional validation passed
- macOS Apple Silicon ARM64 — functional validation passed on a real Apple M1 Mac

Compatibility build targets also exist for:

- Windows ARM64
- Linux ARM64
- macOS Intel x64

These remain architecture-specific validation targets until equivalent hardware testing is completed.

## Architecture

```text
CodyFormat.sln
src/
  CodyFormat.Core
  CodyFormat.Desktop
  CodyFormat.SmokeTests
```

`CodyFormat.Core` contains OS-independent formatting, highlighting, export, profile, and related logic.

`CodyFormat.Desktop` provides the Avalonia UI and platform integrations.

## What was resolved during the migration

The preview cycle hardened several platform-specific areas:

- Avalonia XAML compatibility
- generated `InitializeComponent` wiring
- Windows native rich clipboard behavior
- Linux X11/XWayland rich clipboard behavior
- cross-platform icons
- Linux desktop integration
- macOS `.app` packaging
- formatter safety protections
- release packaging and checksum scripts

## Legacy WPF baseline

`v0.5.0` remains preserved as a historical Windows rollback point.

It should not receive normal feature development now that `v0.6.0` is the accepted cross-platform Stable baseline.

## Development after migration

New work starts from the cross-platform codebase and follows the versioned roadmap:

```text
v0.7.0  Developer workflow
v0.8.0  Export and presentation
v0.9.0  Desktop productivity and scale
v1.0.0  Long-term stable toolchain
```

See [`ROADMAP.md`](ROADMAP.md).

# Changelog

All notable changes to CodyFormat are documented in this file.

CodyFormat v0.6.0 is the first stable cross-platform Avalonia baseline for Windows, Linux, and macOS. The earlier preview entries are preserved below as migration history.

## [0.6.0] - 2026-09-03

### Stable cross-platform baseline

- Promoted the Avalonia codebase to the first stable cross-platform CodyFormat release.
- Validated Windows x64 runtime and feature workflows.
- Validated Ubuntu Linux x64 runtime, desktop integration, rich clipboard, and export workflows.
- Validated macOS Apple Silicon ARM64 on a real Apple M1 Mac.
- Promoted the application version from `0.6.0-preview.6` to `0.6.0`.
- Added stable Windows, Linux, and macOS release asset naming.
- Added macOS release ZIP generation and cross-platform checksum coverage.
- Updated release, testing, migration, security, and installation documentation for Stable.
- Added a versioned roadmap for v0.7.0, v0.8.0, v0.9.0, and v1.0.0.

### Compatibility status

- Windows ARM64 remains a buildable compatibility target pending broader real-hardware validation.
- Linux ARM64 remains a buildable compatibility target pending broader real-hardware validation.
- macOS Intel x64 remains a buildable compatibility target pending real Intel Mac validation.


## [0.6.0-preview.6] - 2026-09-02

### Fixed

- Fixed Windows **Copy for Word** pasting raw RTF control text instead of formatted code.
- Added a native Win32 clipboard backend using the registered `Rich Text Format` and `HTML Format` clipboard formats plus `CF_UNICODETEXT`.
- Added bounded clipboard-open retries to handle temporary clipboard contention from Word, Remote Desktop, clipboard history, and clipboard helper processes.
- Changed the Windows fallback path so it never publishes RTF as an Avalonia custom string format. The fallback now prefers HTML plus plain text, preventing raw `\\rtf1` control text from appearing in documents.

### Preserved

- Native Linux X11/XWayland rich clipboard backend introduced in preview.5.
- Improved cross-platform formatter engine and safety guards.
- New CodyFormat application icon across Windows, Linux, and macOS assets.
- Telegram, DOCX, formatting profiles, No Background mode, Help, Donate, and offline/privacy behavior.

## [0.6.0-preview.5] - 2026-09-02

### Linux clipboard

- Added a native X11/XWayland rich clipboard owner for office-compatible copy on Linux.
- Advertises HTML, RTF/rich-text, UTF-8 text, and plain-text targets directly to destination applications.
- Retains Avalonia DataTransfer as a fallback when an X11 display is unavailable.
- Added clearer clipboard-backend information to the application status bar for troubleshooting.

### Formatter

- Added safer assignment, comparison, and logical-operator spacing for brace-based languages.
- Improved indentation behavior across C#, Java, JavaScript, TypeScript, C/C++, Rust, Go, PHP, Kotlin, Swift, Dart, Objective-C, Scala, and Groovy families.
- Improved HTML nesting and formatting of embedded `<script>` and `<style>` blocks.
- Improved CSS property/value spacing.
- Improved YAML established-indentation and safe key/list spacing while preserving block scalars.
- Added or tightened protection for raw strings, template literals, heredocs, multiline strings, Rust lifetimes, PowerShell here-strings, Lua long strings, and similar syntax.
- Expanded smoke-test coverage to a representative formatting path for every selectable language.

### Branding and Linux integration

- Replaced the previous artwork with the new CodyFormat master icon.
- Regenerated Windows `.ico`, macOS `.icns`, Avalonia PNG, and Linux hicolor/pixmap assets.
- Set the Linux X11 `WmClass` to match the desktop launcher identity.

## [0.6.0-preview.4] - 2026-09-02

### Changed

- Expanded rich clipboard output to HTML + RTF + plain-text representations.
- Added clearer Telegram action styling.
- Improved Linux desktop integration with user-local `.desktop` installation and hicolor icon registration.
- Added user-local Linux install/uninstall scripts requiring no `sudo` privileges.
- Rebuilt application icon assets for improved Windows/Linux scaling.

## [0.6.0-preview.3] - 2026-09-01

### Fixed

- Fixed Avalonia startup `NullReferenceException` caused by manually implementing `InitializeComponent()`.
- Restored Avalonia source-generated named-control initialization in Main, Format Preview, Help, and Donate windows.

## [0.6.0-preview.2] - 2026-08-31

### Fixed

- Replaced WPF-style `TextBox` scrollbar properties with Avalonia `ScrollViewer` attached properties.
- Replaced unsupported WPF-style `Checked` / `Unchecked` handlers with Avalonia-compatible checked-state handling.
- Updated Windows publish script error handling so failed builds no longer print a misleading success message.

## [0.6.0-preview.1] - 2026-08-31

### Added

- Began migration from WPF to Avalonia.
- Split OS-independent code into `CodyFormat.Core`.
- Added Avalonia desktop UI project.
- Added Windows x64/ARM64, Linux x64/ARM64, and macOS x64/ARM64 runtime targets.
- Added cross-platform source-file handling, settings, clipboard abstraction, and packaging structure.
- Added GitHub Actions cross-platform build gate.
- Added dependency-free smoke-test project.

## [0.5.0] - 2026-08-31

### Windows stable baseline

- Last known-stable WPF release before the Avalonia migration.
- Added Telegram-compatible fenced-code copy.
- Included formatting, syntax highlighting, Word copy, DOCX export, themes, backgrounds, Help, Donate, and offline/privacy functionality.

The WPF `v0.5.0` release remains preserved as a historical rollback baseline; `v0.6.0` is now the primary stable cross-platform baseline.

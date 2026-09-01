# Changelog

## 0.6.0-preview.3

### Fixed
- Fixed startup `NullReferenceException` in `MainWindow` caused by manually overriding Avalonia-generated `InitializeComponent()`.
- Removed manual `InitializeComponent()` implementations from Main, Help, Donate, and Format Preview windows.
- Restored Avalonia source-generator wiring for all `x:Name` controls.

## 0.6.0-preview.2

### Fixed
- Fixed Avalonia 12 AXAML compilation errors caused by WPF-style `Checked` / `Unchecked` events.
- Replaced those handlers with Avalonia `IsCheckedChanged`.
- Fixed `TextBox` scrollbar declarations by using the `ScrollViewer.HorizontalScrollBarVisibility` and `ScrollViewer.VerticalScrollBarVisibility` attached properties.
- Windows publish script now stops immediately when `dotnet publish` returns a non-zero exit code instead of printing a misleading success message.
- Windows publish output for each RID is cleaned before publishing to avoid stale artifacts.

## 0.6.0-preview.1

### Cross-platform architecture
- Added Avalonia 12 desktop UI targeting Windows, macOS and Linux.
- Split formatter, highlighting, language detection, Telegram, DOCX and HTML export logic into an OS-independent `CodyFormat.Core` project.
- Removed WPF/System.Windows dependencies from the shared Core.
- Added runtime targets for Windows x64/ARM64, macOS x64/ARM64 and Linux x64/ARM64.

### Platform integration
- Added Avalonia native file-picker integration.
- Added cross-platform drag-and-drop source-file loading.
- Added platform-aware text and rich HTML clipboard bridge.
- Added stream-based DOCX export for platform storage providers.
- Added macOS application-bundle metadata and ICNS icon.
- Added Linux desktop-entry template.

### Quality gates
- Added dependency-free Core smoke tests.
- Added GitHub Actions cross-platform build workflow.
- Added migration and platform test documentation.

### Stability policy
- Windows WPF v0.5.0 remains the stable release and rollback baseline.
- v0.6.0-preview.1 is a migration preview and must not replace the stable Windows release yet.

### Hardening
- Added explicit Windows/macOS/Linux settings-directory resolution.
- Added atomic settings-file replacement.
- Fixed source-file binary detection so BOM-marked UTF-16/UTF-32 source files are accepted correctly.
- Clipboard output now requests persistence through Avalonia `FlushAsync()` on supported backends.
- Window chrome uses the cross-platform PNG asset while the Windows executable keeps the multi-size ICO.

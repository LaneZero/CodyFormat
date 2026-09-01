# CodyFormat Cross-Platform Migration

## Stable baseline

**Do not modify or replace the Windows stable release:** `CodyFormat v0.5.0` (WPF).

The cross-platform line begins at `v0.6.0-preview.1` and uses Avalonia.

## Migration goals

1. One shared formatting/export engine.
2. One desktop UI codebase for Windows, macOS and Linux.
3. Preserve the offline/privacy model.
4. Preserve Word, DOCX and Telegram workflows.
5. Preserve Windows behavior before retiring WPF.
6. Use native platform file pickers, drag/drop and clipboard APIs.

## Completed in preview.1

- Split OS-independent code into `CodyFormat.Core`.
- Removed WPF/System.Windows dependencies from Core.
- Ported the primary desktop layout to Avalonia.
- Added platform-aware clipboard bridge.
- Added cross-platform storage provider for open/save.
- Added stream-based source-file import and DOCX export.
- Added Windows/macOS/Linux runtime IDs.
- Added macOS `.app` bundle template and `.icns` icon.
- Added Linux desktop-entry template.
- Added GitHub Actions cross-platform build gate.
- Added dependency-free smoke-test executable.

## Still required before v0.6.0 stable

- Compile gate on all three OS families.
- Pixel/UX pass on Windows and macOS.
- Word rich-paste compatibility test on Windows.
- Word rich-paste compatibility test on macOS.
- Apple code signing and notarization workflow.
- macOS Intel runtime test if Intel distribution remains supported.
- Linux GNOME/KDE clipboard test.
- Large-file responsiveness test.
- Accessibility/keyboard navigation pass.

## Branch recommendation

```text
main                         -> Windows stable / release history
cross-platform/avalonia      -> v0.6.x migration
```

Do not delete the WPF source/history until the cross-platform build has been stable through at least one public release cycle.

# CodyFormat v0.6.0-preview.3 Test Plan

## Automated gate

Run:

```bash
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release
```

Expected: `CodyFormat smoke tests PASS`.

The automated core gate also covers UTF-16 decoding, binary-file rejection and the platform settings path.

## Windows regression

- Launch win-x64 self-contained build.
- Verify Dark/Light ComboBox text visibility.
- Test all main controls against v0.5.0 stable.
- Open/drop `.sql`, `.cs`, `.rs`, `.go`, `.php` files.
- Format document and selection.
- Undo with Ctrl+Z.
- Verify None background removes fill in Preview, Word clipboard and DOCX.
- Paste Copy for Word into current Microsoft Word.
- Paste Copy Telegram into Telegram Desktop.
- Export DOCX and open it in Word.

## macOS Apple Silicon

- Build/package `osx-arm64`.
- Launch `CodyFormat.app` from Finder.
- Verify menu/window identity and icon.
- Test Cmd+O, Cmd+Enter, Cmd+Shift+F, Cmd+Z.
- Test file picker and drag/drop.
- Test Telegram clipboard.
- Test rich Word clipboard in Microsoft Word for Mac.
- Test DOCX export and reopen in Word.
- Test settings persistence under Application Support.

## macOS Intel

Repeat the Apple Silicon functional checklist with the `osx-x64` build if Intel support is retained.

## Linux

Test at least Ubuntu GNOME. Prefer a second pass on KDE.

- Launch self-contained `linux-x64` build.
- Verify file picker.
- Verify drag/drop.
- Verify text clipboard.
- Verify `text/html` rich clipboard in a compatible target.
- Export DOCX.
- Confirm no required network access after install.

## Promotion to stable

Do not tag `v0.6.0` stable until Windows feature parity and macOS Word clipboard are both confirmed on real machines.

## Startup regression check

After publishing, launch the application from PowerShell once before GUI testing:

```powershell
dotnet .\artifacts\win-x64\CodyFormat.dll
```

The application must open without a `NullReferenceException`. Then launch `CodyFormat.exe` normally and verify Main, Help, Donate, and Format Preview windows.


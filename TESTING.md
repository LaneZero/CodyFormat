# CodyFormat v0.6.0 Stable Test Plan and Release Status

This document records the validation state of the first stable cross-platform Avalonia baseline.

## Current validation status

| Platform | Architecture | Build | Runtime / functional validation | Stable release status |
|---|---|---|---|---|
| Windows | x64 | PASS | PASS | ✅ Primary tested platform |
| Windows | ARM64 | PASS target | Broader hardware validation pending | 🧪 Compatibility build |
| Ubuntu Linux | x64 | PASS | PASS | ✅ Primary tested platform |
| Linux | ARM64 | PASS target | Broader hardware validation pending | 🧪 Compatibility build |
| macOS | Apple Silicon ARM64 | PASS | **PASS on real Apple M1 Mac** | ✅ Primary tested platform |
| macOS | Intel x64 | PASS target | Real Intel hardware validation pending | 🧪 Compatibility build |

## Stable release gate

The v0.6.0 Stable baseline is accepted because the main desktop implementation has passed functional testing on three operating-system families:

```text
Windows x64
Ubuntu Linux x64
macOS Apple Silicon ARM64 (Apple M1)
```

Compatibility builds for other architectures remain available but must not be described as equivalently hardware-tested until that validation exists.

---

## Core smoke tests

Run:

```bash
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release
```

Required result:

```text
CodyFormat smoke tests PASS
```

Smoke coverage should include formatter paths, protected syntax, clipboard/export builders, Telegram output, DOCX structure, encoding handling, and binary-file rejection.

---

## Functional regression checklist

Run on each primary tested platform after changes that affect UI, formatter, clipboard, file handling, export, settings, or packaging.

- [ ] Application launches
- [ ] Correct application icon
- [ ] Dark theme
- [ ] Light theme
- [ ] Language selection
- [ ] Auto Detect
- [ ] Format Document
- [ ] Format Selection
- [ ] Format Preview
- [ ] Apply / Cancel
- [ ] Undo
- [ ] Open File
- [ ] Drag & Drop
- [ ] Copy Plain
- [ ] Copy Telegram
- [ ] Copy for Word / rich office copy
- [ ] None Background
- [ ] Line numbers
- [ ] Border
- [ ] Font and size
- [ ] DOCX export
- [ ] Help
- [ ] Donate
- [ ] GitHub action/link
- [ ] Settings persistence

---

## Windows x64 validation

Specific checks:

- native Win32 rich clipboard path activates
- Word does not receive visible raw RTF control text
- CF_HTML / RTF / Unicode fallback behaves correctly
- self-contained ZIP launches after extraction
- application icon is correct in window/taskbar/Alt-Tab

Expected rich-copy status should indicate the native Windows clipboard path when available.

---

## Ubuntu Linux x64 validation

Specific checks:

- portable executable launches
- `install-user.sh` works without sudo
- application launcher and icon appear correctly
- X11/XWayland rich clipboard path works where available
- office copy retains formatting on the validated environment
- DOCX remains the deterministic rich-output fallback

---

## macOS Apple Silicon validation

Reference real hardware:

```text
Apple M1 Mac
Architecture: arm64
```

The v0.6.0 Stable codebase has been tested successfully on this platform.

Specific checks:

- `file .../CodyFormat` reports `arm64`
- `.app` launches normally
- Finder/Dock application icon is correct
- Dark/Light themes work
- Open File and Drag & Drop work
- formatting and preview work
- Copy Plain works
- Copy Telegram works
- rich office copy is functionally checked
- DOCX export opens and remains editable
- Help / Donate / GitHub work
- settings persist across restart

If code signing or notarization changes the final distributable after testing, perform a short post-signing regression run on the final package.

---

## Architecture compatibility builds

The following targets are buildable but are not yet treated as equal real-hardware validation claims:

```text
win-arm64
linux-arm64
osx-x64
```

Publishing them is allowed when their status is clearly labeled.

---

## Formatter regression policy

For formatter changes:

1. add or update a minimal representative smoke test,
2. verify protected strings/comments/template syntax,
3. verify no unexpected token removal,
4. test at least one realistic sample for the affected language,
5. avoid expanding a formatter rule to unrelated language families without evidence.

Whitespace-sensitive languages such as Python and YAML require conservative handling.

---

## Clipboard regression policy

Any clipboard change must be tested separately on Windows, Linux, and macOS because clipboard formats and destination-app behavior differ by platform.

Minimum outputs:

```text
Plain Text
Telegram fenced code
Rich office copy
DOCX export
```

DOCX must remain independent of clipboard implementation.

---

## Release archive verification

Before upload:

- extract each primary-platform archive into a clean location
- launch the packaged app, not the development build
- verify package filename/version
- generate SHA-256

After upload:

- download the GitHub asset again
- compare checksum
- confirm archive opens

---

## Stable baseline rule

`v0.6.0` is the current stable baseline.

New roadmap capabilities belong in later version branches. Do not retrofit experimental v0.7+ features directly into the v0.6.0 Stable branch.

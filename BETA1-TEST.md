# CodyFormat v0.7.0-beta.1 — Test Instructions

## 1. Automated gate

Windows PowerShell:

```powershell
Get-ChildItem -Recurse -Filter *.ps1 | Unblock-File
.\scripts\validate-beta1.ps1
```

Expected final line:

```text
CodyFormat v0.7.0-beta.1 validation PASS
```

Linux/macOS:

```bash
./scripts/validate-beta1.sh
```

## 2. Native provider diagnostics

```powershell
.\src\CodyFormat.Cli\bin\Release\net10.0\codyformat.exe --native-status
```

Missing providers are valid. CodyFormat must report them as missing rather than downloading or installing anything.

## 3. CLI opt-in / fallback

```powershell
"def f(a,b):`n  return a+b" | .\src\CodyFormat.Cli\bin\Release\net10.0\codyformat.exe --stdin --language Python --native
```

If Black is installed in a safe PATH entry it may be used. Otherwise CodyFormat must use the built-in formatter and still produce formatted output.

## 4. GUI

- Native formatter is OFF after a clean settings state.
- Native Status lists Black, Prettier, clang-format, rustfmt, and gofmt.
- Enabling Native formatter does not execute anything by itself.
- Native tools run only on explicit Format Code / Format Selection actions.
- Live preview and Copy/Export must not spawn native tools.
- Missing/failed native formatter falls back to built-in formatting.
- Preview-before-apply still works.

## 5. Release gate

Publish as a GitHub pre-release, download the actual platform asset back from GitHub, verify SHA-256, fresh-extract, verify `--version`, run the GUI checks, and confirm `Check Updates` reports the published beta correctly.

# CodyFormat v0.7.0-alpha.3 — Test Instructions

## 1. Automated gate

Windows PowerShell:

```powershell
Get-ChildItem -Recurse -Filter *.ps1 | Unblock-File
.\scripts\validate-alpha3.ps1
```

Expected:

```text
CodyFormat v0.7.0-alpha.3 validation PASS
```

## 2. GUI

Run the Desktop project. Open a local source file under a project containing `.editorconfig`. Confirm the `.editorconfig` checkbox is enabled and formatting follows the project indentation/EOL rules.

Use **Import** / **Export** next to Profile to test Profile v2 JSON. A `.codyformat/profile.json` file in a parent project directory should be detected when opening a local source file.

## 3. CLI

```powershell
dotnet run --project .\src\CodyFormat.Cli\CodyFormat.Cli.csproj -c Release --no-build -- --version
```

Then test a project file with and without `.editorconfig`:

```powershell
dotnet run --project .\src\CodyFormat.Cli\CodyFormat.Cli.csproj -c Release --no-build -- .\test.py --check
dotnet run --project .\src\CodyFormat.Cli\CodyFormat.Cli.csproj -c Release --no-build -- .\test.py --check --no-editorconfig
```

## 4. GitHub Release acceptance

After local PASS, publish `v0.7.0-alpha.3` as a **Pre-release**, download the actual platform package back from GitHub, verify SHA-256, extract it into a fresh folder, and repeat GUI/CLI checks. The downloaded GitHub asset is the final acceptance artifact.

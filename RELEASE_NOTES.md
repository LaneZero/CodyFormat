# CodyFormat v0.7.0-alpha.3

Development pre-release. `v0.6.0` remains the Stable baseline.

## What changed

- Adds shared `.editorconfig` support to GUI and CLI.
- Adds Profiles v2 JSON import/export and project profile discovery.
- Keeps all Python/all-language formatter and Batch Formatting regression tests from earlier Alpha builds.

## Local validation

Windows:

```powershell
.\scripts\validate-alpha3.ps1
```

Linux/macOS:

```bash
./scripts/validate-alpha3.sh
```

Expected final line:

```text
CodyFormat v0.7.0-alpha.3 validation PASS
```

After publishing a GitHub pre-release, download the actual release asset again, verify SHA-256, extract into a clean location, and repeat the runtime acceptance checks.

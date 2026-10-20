# GitHub Release Asset Verification Gate

This gate applies to every CodyFormat Alpha, Beta, RC, and Stable milestone.

## Rule

A local build is not the final acceptance artifact. The final acceptance test must use the package downloaded back from GitHub Releases.

## Required workflow

1. Run local validation.
2. Build release packages.
3. Generate `SHA256SUMS.txt`.
4. Publish the GitHub Release or Pre-release.
5. Download the published platform asset again.
6. Verify SHA-256.
7. Extract/install into a fresh location.
8. Verify the reported CodyFormat version.
9. Run the milestone smoke tests, including the manual **Check Updates** action when the milestone contains it.
10. Record OS and architecture.
11. Mark the milestone accepted only after these checks pass.

## Checksums

Windows:

```powershell
Get-FileHash .\CodyFormat-<version>-win-x64.zip -Algorithm SHA256
```

Linux:

```bash
sha256sum CodyFormat-<version>-linux-x64.tar.gz
```

macOS:

```bash
shasum -a 256 CodyFormat-<version>-macos-arm64.zip
```

If signing, notarization, repackaging, or release automation modifies the final bytes, repeat validation against the final GitHub-downloaded package.

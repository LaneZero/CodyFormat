# CodyFormat v0.6.0 - GitHub Finalization Guide

This is the one-time repository promotion workflow for moving CodyFormat from the old Windows-only `main` branch to the stable cross-platform `v0.6.0` codebase.

## Repository state before promotion

Expected remote state:

- `main`: legacy Windows/WPF generation
- `cross-platform/avalonia`: migration branch
- existing historical tag: `v0.3.1`
- target stable tag: `v0.6.0`

Do not rewrite or delete the historical `v0.3.1` tag.

## 1. Work from a fresh clone

Using a fresh clone avoids accidentally committing old build output or local experiments.

```powershell
git clone https://github.com/LaneZero/CodyFormat.git CodyFormat-release
cd CodyFormat-release
git fetch --all --tags --prune
git status
git branch -a
git tag --list
```

## 2. Update the migration branch first

```powershell
git switch cross-platform/avalonia
git pull --ff-only origin cross-platform/avalonia
git status --short
```

The worktree must be clean before replacing its contents.

## 3. Replace the branch content with the final v0.6.0 source

Remove the currently tracked migration-preview files:

```powershell
git rm -r .
```

Then copy the complete contents of the final `CodyFormat-v0.6.0` source directory into this repository root. Copy hidden entries too, especially:

```text
.github/
.gitignore
.gitattributes
```

Do **not** copy or replace the `.git/` directory.

A safe PowerShell copy pattern, after setting the source path, is:

```powershell
$Source = "C:\PATH\TO\CodyFormat-v0.6.0"

if (-not (Test-Path "$Source\CodyFormat.sln")) {
    throw "Source path is wrong: CodyFormat.sln was not found."
}

Get-ChildItem -Force $Source | ForEach-Object {
    Copy-Item $_.FullName -Destination . -Recurse -Force
}
```

## 4. Stage everything, including deletions

```powershell
git add -A
```

Preserve executable bits for Unix scripts in Git:

```powershell
git update-index --chmod=+x scripts/create-checksums.sh
git update-index --chmod=+x scripts/publish-linux.sh
git update-index --chmod=+x scripts/publish-macos.sh
git update-index --chmod=+x scripts/smoke-test.sh
git update-index --chmod=+x packaging/linux/install-user.sh
git update-index --chmod=+x packaging/linux/uninstall-user.sh
```

## 5. Inspect what will be committed

```powershell
git status
git diff --cached --stat
git diff --cached --check
```

Confirm that generated folders are not tracked:

```powershell
git ls-files | Select-String '(^|/)(bin|obj|artifacts|TestResults)/'
```

The command above should return no results.

Confirm executable script modes:

```powershell
git ls-files -s -- 'scripts/*.sh' 'packaging/linux/*.sh'
```

Unix scripts should show mode:

```text
100755
```

Confirm that release archives were not accidentally added to Git history:

```powershell
git ls-files | Select-String '(\.zip$|\.tar\.gz$|\.dmg$|\.pkg$)'
```

Normally this should return no generated release archives.

## 6. Validate the source before committing

```powershell
dotnet restore CodyFormat.sln
dotnet build CodyFormat.sln -c Release --no-restore
dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release
```

Required smoke-test result:

```text
CodyFormat smoke tests PASS
```

## 7. Commit the final stable code to the migration branch

```powershell
git commit -m "release: finalize CodyFormat v0.6.0 cross-platform stable"
git push origin cross-platform/avalonia
```

## 8. Open a Pull Request into main

Create a GitHub Pull Request:

```text
base:    main
compare: cross-platform/avalonia
```

Recommended title:

```text
Release CodyFormat v0.6.0 stable cross-platform
```

Review the complete file diff and wait for GitHub Actions to pass before merging.

Use a normal merge commit for this migration so the branch history remains visible.

## 9. Update local main after the PR is merged

```powershell
git switch main
git pull --ff-only origin main
git status
```

## 10. Verify stable version metadata

```powershell
Get-Content VERSION.txt
Select-String -Path src/CodyFormat.Core/AppInfo.cs -Pattern '0.6.0'
Select-String -Path src/CodyFormat.Desktop/CodyFormat.Desktop.csproj -Pattern '0.6.0'
```

The canonical public version must be `0.6.0`.

## 11. Create the stable tag

First verify the tag does not already exist:

```powershell
git tag --list v0.6.0
```

If no result is returned:

```powershell
git tag -a v0.6.0 -m "CodyFormat v0.6.0 Stable"
git show v0.6.0 --no-patch
git push origin v0.6.0
```

Never move or recreate a public release tag after publishing unless there is an exceptional reason.

## 12. Create the GitHub Release

Create a release from tag:

```text
v0.6.0
```

Release title:

```text
CodyFormat v0.6.0
```

This is a Stable release, so do not mark it as a pre-release.

Upload release binaries only, not raw publish directories:

```text
CodyFormat-v0.6.0-win-x64.zip
CodyFormat-v0.6.0-win-arm64.zip
CodyFormat-v0.6.0-linux-x64.tar.gz
CodyFormat-v0.6.0-linux-arm64.tar.gz
CodyFormat-v0.6.0-macos-arm64.zip
CodyFormat-v0.6.0-macos-x64.zip
SHA256SUMS.txt
```

GitHub automatically provides source-code ZIP and TAR.GZ archives for the tag.

Validation labels should remain honest:

```text
Windows x64       Tested
Ubuntu Linux x64  Tested
macOS ARM64       Tested on Apple M1
Windows ARM64     Build available; wider hardware validation pending
Linux ARM64       Build available; wider hardware validation pending
macOS Intel x64   Build available; Intel hardware validation pending
```

## 13. Post-release repository cleanup

After the Release page and tag are verified, the migration branch may be deleted because `main` now contains the stable Avalonia codebase.

Remote branch deletion:

```powershell
git push origin --delete cross-platform/avalonia
```

Local deletion:

```powershell
git branch -d cross-platform/avalonia
```

Do this only after confirming `v0.6.0` exists on `main` and the GitHub Release is correct.

## 14. Start the next development line

Keep `main` stable.

```powershell
git switch main
git pull --ff-only origin main
git switch -c develop/v0.7.0
git push -u origin develop/v0.7.0
```

Future features should branch from `develop/v0.7.0`, for example:

```text
feature/v0.7.0-cli
feature/v0.7.0-batch-format
feature/v0.7.0-editorconfig
feature/v0.7.0-native-formatters
```

Bug fixes for the Stable release should use a `fix/v0.6.1-*` branch and return to `main` through a reviewed pull request.

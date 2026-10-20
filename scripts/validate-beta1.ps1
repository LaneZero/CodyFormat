$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

Write-Host "== CodyFormat v0.7.0-beta.1 validation =="

dotnet --version
if ($LASTEXITCODE -ne 0) { throw "dotnet SDK check failed" }

dotnet restore CodyFormat.sln
if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

dotnet build CodyFormat.sln -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release --no-build
if ($LASTEXITCODE -ne 0) { throw "Smoke tests failed" }

$version = dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --version
if ($LASTEXITCODE -ne 0 -or $version.Trim() -ne "0.7.0-beta.1") { throw "CLI version check failed: $version" }

$status = dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --native-status
if ($LASTEXITCODE -ne 0 -or -not ($status -match "Black") -or -not ($status -match "Prettier")) { throw "Native provider status check failed" }

$nativeOutput = "def f(a,b):`n  return a+b`n" | dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --stdin --language Python --native --quiet
if ($LASTEXITCODE -ne 0 -or -not ($nativeOutput -match "def f\(a, b\):")) { throw "Native/fallback CLI formatting failed" }

# SmokeTests contain the cumulative Alpha.1/Alpha.2/Alpha.3 regression gates.

Write-Host "CodyFormat v0.7.0-beta.1 validation PASS"

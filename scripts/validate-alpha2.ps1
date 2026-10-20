$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

Write-Host "== CodyFormat v0.7.0-alpha.2 validation =="
dotnet --version
dotnet restore CodyFormat.sln
if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

dotnet build CodyFormat.sln -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release --no-build
if ($LASTEXITCODE -ne 0) { throw "Smoke tests failed" }

$version = dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --version
if ($LASTEXITCODE -ne 0 -or $version.Trim() -ne "0.7.0-alpha.2") { throw "CLI version check failed: $version" }

dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --help | Out-Null
if ($LASTEXITCODE -ne 0) { throw "CLI help check failed" }

$tempDir = Join-Path ([IO.Path]::GetTempPath()) ("codyformat-alpha2-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempDir | Out-Null
try {
    New-Item -ItemType Directory -Path (Join-Path $tempDir "src") | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $tempDir "bin") | Out-Null
    New-Item -ItemType Directory -Path (Join-Path $tempDir "Generated") | Out-Null

    $sample = Join-Path $tempDir "src\sample.py"
    $ignored = Join-Path $tempDir "bin\ignored.py"
    $excluded = Join-Path $tempDir "Generated\generated.py"

    [IO.File]::WriteAllText($sample, "def f(a,b):`r`n  x=1`r`n  return a+b`r`n", [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($ignored, "def f(a,b):`r`n  return a+b`r`n", [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText($excluded, "def f(a,b):`r`n  return a+b`r`n", [Text.UTF8Encoding]::new($false))

    & dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- $tempDir --recursive --check --exclude "**/Generated/**" --quiet
    if ($LASTEXITCODE -ne 1) { throw "Batch --check should return 1 before formatting, got $LASTEXITCODE" }
    if (-not ([IO.File]::ReadAllText($sample).Contains("def f(a,b):"))) { throw "Batch --check modified a file" }

    & dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- $tempDir --recursive --write --backup --include "**/*.py" --exclude "**/Generated/**" --quiet
    if ($LASTEXITCODE -ne 0) { throw "Batch --write failed" }

    $text = [IO.File]::ReadAllText($sample)
    if (-not $text.Contains("def f(a, b):") -or -not $text.Contains("    x = 1")) { throw "Batch formatting result is unexpected" }
    if (-not (Test-Path "$sample.codyformat.bak")) { throw "Batch backup was not created" }
    if (-not ([IO.File]::ReadAllText($ignored).Contains("def f(a,b):"))) { throw "Default excluded bin directory was modified" }
    if (-not ([IO.File]::ReadAllText($excluded).Contains("def f(a,b):"))) { throw "Custom excluded directory was modified" }

    & dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- $tempDir --recursive --check --include "**/*.py" --exclude "**/Generated/**" --quiet
    if ($LASTEXITCODE -ne 0) { throw "Batch second --check should be clean" }
}
finally {
    Remove-Item -Recurse -Force $tempDir -ErrorAction SilentlyContinue
}

Write-Host "CodyFormat v0.7.0-alpha.2 validation PASS"

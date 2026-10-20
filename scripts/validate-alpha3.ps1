$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
Set-Location $Root

Write-Host "== CodyFormat v0.7.0-alpha.3 validation =="

dotnet --version
if ($LASTEXITCODE -ne 0) { throw "dotnet SDK check failed" }

dotnet restore CodyFormat.sln
if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

dotnet build CodyFormat.sln -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

dotnet run --project src/CodyFormat.SmokeTests/CodyFormat.SmokeTests.csproj -c Release --no-build
if ($LASTEXITCODE -ne 0) { throw "Smoke tests failed" }

$version = dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- --version
if ($LASTEXITCODE -ne 0 -or $version.Trim() -ne "0.7.0-alpha.3") { throw "CLI version check failed: $version" }

$temp = Join-Path ([IO.Path]::GetTempPath()) ("codyformat-alpha3-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path (Join-Path $temp "src") -Force | Out-Null
try {
    $editorConfigText = @"
root = true

[*.py]
indent_style = space
indent_size = 2
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true
"@
    [IO.File]::WriteAllText((Join-Path $temp ".editorconfig"), $editorConfigText, [Text.UTF8Encoding]::new($false))

    $sample = Join-Path $temp "src\sample.py"
    [IO.File]::WriteAllText($sample, "def f(a,b):`r`n    x=1   ", [Text.UTF8Encoding]::new($false))

    & dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- $sample --check --quiet
    if ($LASTEXITCODE -ne 1) { throw "EditorConfig --check should report changes" }

    & dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- $sample --write --quiet
    if ($LASTEXITCODE -ne 0) { throw "EditorConfig --write failed" }

    $bytes = [IO.File]::ReadAllBytes($sample)
    $text = [Text.UTF8Encoding]::new($false,$true).GetString($bytes)
    if (-not $text.Contains("  x = 1")) { throw "indent_size=2 was not applied" }
    if ($text.Contains("`r`n")) { throw "end_of_line=lf was not applied" }
    if (-not $text.EndsWith("`n")) { throw "insert_final_newline=true was not applied" }
    if ($text.Contains("   `n")) { throw "trim_trailing_whitespace=true was not applied" }

    $profile = Join-Path $temp "team.codyprofile.json"
    $profileText = @'
{
  "schemaVersion": 2,
  "name": "Team Two Space",
  "description": "Validation profile",
  "formatting": { "indentStyle": "space", "indentSize": 2 },
  "appearance": {
    "theme": "Dark", "background": "Theme Default", "noBackground": false,
    "lineNumbers": true, "border": true, "fontFamily": "Cascadia Mono", "fontSize": 14
  },
  "output": { "formatBeforeOutput": false, "target": "General" }
}
'@
    [IO.File]::WriteAllText($profile, $profileText, [Text.UTF8Encoding]::new($false))

    [IO.File]::WriteAllText($sample, "def f(a,b):`n    x=1`n", [Text.UTF8Encoding]::new($false))
    & dotnet run --project src/CodyFormat.Cli/CodyFormat.Cli.csproj -c Release --no-build -- $sample --write --profile $profile --no-editorconfig --quiet
    if ($LASTEXITCODE -ne 0) { throw "Profile v2 CLI test failed" }
    $profileText = [IO.File]::ReadAllText($sample)
    if (-not $profileText.Contains("  x = 1")) { throw "Profile v2 indent was not applied" }
}
finally {
    Remove-Item -Recurse -Force $temp -ErrorAction SilentlyContinue
}

Write-Host "CodyFormat v0.7.0-alpha.3 validation PASS"

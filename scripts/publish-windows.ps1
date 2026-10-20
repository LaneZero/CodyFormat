$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$DesktopProject = Join-Path $Root "src\CodyFormat.Desktop\CodyFormat.Desktop.csproj"
$CliProject = Join-Path $Root "src\CodyFormat.Cli\CodyFormat.Cli.csproj"
$Version = (Get-Content (Join-Path $Root "VERSION.txt") -Raw).Trim()
$Artifacts = Join-Path $Root "artifacts"

New-Item -ItemType Directory -Force -Path $Artifacts | Out-Null

foreach ($rid in @("win-x64", "win-arm64")) {
    $out = Join-Path $Artifacts $rid
    $cliOut = Join-Path $Artifacts "publish-cli\$rid"
    $archive = Join-Path $Artifacts "CodyFormat-v$Version-$rid.zip"

    Remove-Item $out -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $cliOut -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $archive -Force -ErrorAction SilentlyContinue

    Write-Host "Publishing desktop $rid..." -ForegroundColor Cyan
    & dotnet publish $DesktopProject -c Release -r $rid --self-contained true -p:UseAppHost=true -o $out
    if ($LASTEXITCODE -ne 0) { throw "Desktop publish failed for $rid." }

    Write-Host "Publishing CLI $rid..." -ForegroundColor Cyan
    & dotnet publish $CliProject -c Release -r $rid --self-contained true -p:UseAppHost=true -o $cliOut
    if ($LASTEXITCODE -ne 0) { throw "CLI publish failed for $rid." }

    $cliPackage = Join-Path $out "cli"
    New-Item -ItemType Directory -Force -Path $cliPackage | Out-Null
    Copy-Item (Join-Path $cliOut "*") $cliPackage -Recurse -Force

    Write-Host "Packaging $rid..." -ForegroundColor Cyan
    Compress-Archive -Path (Join-Path $out "*") -DestinationPath $archive -CompressionLevel Optimal
    if (-not (Test-Path $archive)) { throw "Failed to create release archive: $archive" }

    Write-Host "Created $archive" -ForegroundColor Green
}

Write-Host "Windows release packages created. Desktop is at package root; CLI is under cli/." -ForegroundColor Green

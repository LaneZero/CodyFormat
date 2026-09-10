$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$Project = Join-Path $Root "src\CodyFormat.Desktop\CodyFormat.Desktop.csproj"
$Version = (Get-Content (Join-Path $Root "VERSION.txt") -Raw).Trim()
$Artifacts = Join-Path $Root "artifacts"

New-Item -ItemType Directory -Force -Path $Artifacts | Out-Null

foreach ($rid in @("win-x64", "win-arm64")) {
    $out = Join-Path $Artifacts $rid
    $archive = Join-Path $Artifacts "CodyFormat-v$Version-$rid.zip"

    if (Test-Path $out) {
        Remove-Item $out -Recurse -Force
    }

    if (Test-Path $archive) {
        Remove-Item $archive -Force
    }

    Write-Host "Publishing $rid..." -ForegroundColor Cyan
    & dotnet publish $Project -c Release -r $rid --self-contained true -p:UseAppHost=true -o $out

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $rid with exit code $LASTEXITCODE."
    }

    Write-Host "Packaging $rid..." -ForegroundColor Cyan
    Compress-Archive -Path (Join-Path $out "*") -DestinationPath $archive -CompressionLevel Optimal

    if (-not (Test-Path $archive)) {
        throw "Failed to create release archive: $archive"
    }

    Write-Host "Created $archive" -ForegroundColor Green
}

Write-Host "Windows release packages created successfully under artifacts/." -ForegroundColor Green

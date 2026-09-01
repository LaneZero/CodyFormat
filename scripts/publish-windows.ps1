$ErrorActionPreference = "Stop"
$Project = Join-Path $PSScriptRoot "..\src\CodyFormat.Desktop\CodyFormat.Desktop.csproj"

foreach ($rid in @("win-x64", "win-arm64")) {
    $out = Join-Path $PSScriptRoot "..\artifacts\$rid"
    if (Test-Path $out) { Remove-Item $out -Recurse -Force }

    Write-Host "Publishing $rid..." -ForegroundColor Cyan
    & dotnet publish $Project -c Release -r $rid --self-contained true -p:UseAppHost=true -o $out
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed for $rid with exit code $LASTEXITCODE."
    }
}

Write-Host "Windows artifacts created successfully under artifacts/" -ForegroundColor Green

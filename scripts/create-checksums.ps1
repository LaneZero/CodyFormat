$ErrorActionPreference = "Stop"

$Root = Resolve-Path (Join-Path $PSScriptRoot "..")
$Artifacts = Join-Path $Root "artifacts"
$Output = Join-Path $Artifacts "SHA256SUMS.txt"

if (-not (Test-Path $Artifacts)) {
    throw "Artifacts directory does not exist. Build release packages first."
}

$Files = Get-ChildItem $Artifacts -File | Where-Object {
    $_.Name -match '^CodyFormat-v.+-(win-(x64|arm64)\.zip|linux-(x64|arm64)\.tar\.gz|macos-(arm64|x64)\.zip)$'
} | Sort-Object Name

if (-not $Files) {
    throw "No CodyFormat release archives were found in artifacts/."
}

$Lines = foreach ($File in $Files) {
    $Hash = (Get-FileHash $File.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    "$Hash  $($File.Name)"
}

Set-Content -Path $Output -Value $Lines -Encoding ascii
Write-Host "Created $Output" -ForegroundColor Green
$Lines | ForEach-Object { Write-Host $_ }

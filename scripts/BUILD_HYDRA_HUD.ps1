param(
    [string]$RepoRoot = "C:\Users\cezar\Desktop\HYDRA"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$Project = Join-Path $RepoRoot "desktop\wpf\src\Hydra.Desktop.Hud\Hydra.Desktop.Hud.csproj"
$Publish = Join-Path $RepoRoot "artifacts\hud\win-x64"
$Zip = Join-Path $RepoRoot "artifacts\hud\HYDRA_HUD_WIN_X64.zip"

if (-not (Test-Path $Project)) {
    throw "HUD project not found: $Project"
}

Get-Process Hydra.Desktop.Hud -ErrorAction SilentlyContinue |
    Stop-Process -Force -ErrorAction SilentlyContinue

Remove-Item $Publish -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $Zip -Force -ErrorAction SilentlyContinue
New-Item $Publish -ItemType Directory -Force | Out-Null

dotnet restore $Project
if ($LASTEXITCODE -ne 0) { throw "HUD restore failed." }

dotnet publish $Project `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -o $Publish

if ($LASTEXITCODE -ne 0) { throw "HUD publish failed." }

Compress-Archive `
    -Path "$Publish\*" `
    -DestinationPath $Zip `
    -CompressionLevel Optimal

$Hash = (Get-FileHash $Zip -Algorithm SHA256).Hash

Write-Host "HYDRA HUD BUILD: PASS" -ForegroundColor Green
Write-Host "EXE: $(Join-Path $Publish 'Hydra.Desktop.Hud.exe')" -ForegroundColor Green
Write-Host "ZIP: $Zip" -ForegroundColor Green
Write-Host "SHA256: $Hash" -ForegroundColor Cyan

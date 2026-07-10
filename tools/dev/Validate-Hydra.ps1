$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Push-Location $root
try {
    cargo check --manifest-path core\rust\Cargo.toml
    dotnet build bridge\dotnet\Hydra.Bridge.sln -v:minimal
    dotnet build desktop\wpf\Hydra.Desktop.sln -v:minimal
    & (Join-Path $PSScriptRoot "Publish-Hydra.ps1")
    dotnet build installer\wix\Hydra.Installer.wixproj -v:minimal

    $forbidden = Get-ChildItem -Force -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object {
            $_.FullName -notmatch '\\(bin|obj|target)\\' -and
            $_.Name -match '\.py$|\.iss$|requirements\.txt|pyproject\.toml'
        }

    if ($forbidden) {
        $forbidden | Format-Table FullName -AutoSize
        throw "Forbidden active source files detected."
    }
}
finally {
    Pop-Location
}

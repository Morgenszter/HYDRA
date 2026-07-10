$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Push-Location $root
try {
    Remove-Item -Recurse -Force artifacts\publish -ErrorAction SilentlyContinue

    dotnet publish bridge\dotnet\src\Hydra.Bridge.Api\Hydra.Bridge.Api.csproj `
        -c Release `
        -r win-x64 `
        --self-contained false `
        -p:PublishSingleFile=true `
        -o artifacts\publish\bridge `
        -v:minimal

    dotnet publish desktop\wpf\src\Hydra.Desktop.App\Hydra.Desktop.App.csproj `
        -c Release `
        -r win-x64 `
        --self-contained false `
        -p:PublishSingleFile=true `
        -o artifacts\publish\desktop `
        -v:minimal
}
finally {
    Pop-Location
}
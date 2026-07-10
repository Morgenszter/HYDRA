param(
    [string]$Urls = "http://127.0.0.1:5025"
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$project = Join-Path $root "bridge\dotnet\src\Hydra.Bridge.Api\Hydra.Bridge.Api.csproj"

dotnet run --project $project --urls $Urls

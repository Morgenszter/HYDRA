param(
    [string]$Urls = "https://localhost:5001;http://localhost:5000"
)

$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$project = Join-Path $root "bridge\dotnet\src\Hydra.Bridge.Api\Hydra.Bridge.Api.csproj"

dotnet run --project $project --urls $Urls

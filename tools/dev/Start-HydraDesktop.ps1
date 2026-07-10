$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$project = Join-Path $root "desktop\wpf\src\Hydra.Desktop.App\Hydra.Desktop.App.csproj"

dotnet run --project $project

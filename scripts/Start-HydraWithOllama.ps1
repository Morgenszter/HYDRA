param(
    [string]$Model = "qwen3:8b",
    [string]$SolutionPath = ".\bridge\dotnet\Hydra.Bridge.slnx",
    [string]$ApiProject = ".\bridge\dotnet\src\Hydra.Bridge.Api\Hydra.Bridge.Api.csproj"
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command ollama -ErrorAction SilentlyContinue)) {
    throw "Ollama CLI was not found in PATH."
}

Write-Host "Ensuring model is available: $Model"
ollama pull $Model

try {
    Invoke-RestMethod `
        -Method Get `
        -Uri "http://127.0.0.1:11434/api/version" `
        | Out-Null
}
catch {
    Write-Host "Starting Ollama server..."
    Start-Process -FilePath "ollama" -ArgumentList "serve"
    Start-Sleep -Seconds 3
}

Write-Host "Restoring .NET solution..."
dotnet restore $SolutionPath

Write-Host "Building .NET solution..."
dotnet build $SolutionPath --configuration Debug

Write-Host "Starting Hydra Bridge API..."
dotnet run --project $ApiProject

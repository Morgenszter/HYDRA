param(
    [string]$Model = "qwen3:8b",
    [string]$Prompt = "Odpowiedz po polsku: HYDRA AI ONLINE"
)

$ErrorActionPreference = "Stop"

Write-Host "Checking Ollama version..."
$version = Invoke-RestMethod `
    -Method Get `
    -Uri "http://127.0.0.1:11434/api/version"

Write-Host "Ollama version: $($version.version)"

$body = @{
    model = $Model
    prompt = $Prompt
    stream = $false
} | ConvertTo-Json

Write-Host "Sending generation request to model $Model..."
$response = Invoke-RestMethod `
    -Method Post `
    -Uri "http://127.0.0.1:11434/api/generate" `
    -ContentType "application/json" `
    -Body $body

Write-Host ""
Write-Host "Response:"
Write-Host $response.response

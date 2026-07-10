$ErrorActionPreference = "Stop"

Write-Host "Checking Ollama..."
try {
  $models = Invoke-RestMethod -Uri "http://127.0.0.1:11434/v1/models" -Method Get
  $models | ConvertTo-Json -Depth 8
} catch {
  Write-Error "Ollama is unavailable on localhost:11434. Start Ollama and pull a model first. $($_.Exception.Message)"
}

if (-not $env:OPENAI_API_KEY) {
  Write-Warning "OPENAI_API_KEY is not set. Cloud routing will remain unavailable."
} else {
  Write-Host "OPENAI_API_KEY is present (value not printed)."
}

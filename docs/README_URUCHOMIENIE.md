# HYDRA ↔ Ollama — uruchomienie

## 1. Wymagania

- Windows 11
- .NET 8 SDK
- Ollama
- model `qwen3:8b`
- opcjonalnie `grpcurl`

## 2. Instalacja modelu

```powershell
ollama pull qwen3:8b
```

## 3. Uruchomienie Ollamy

```powershell
ollama serve
```

Sprawdzenie:

```powershell
Invoke-RestMethod http://127.0.0.1:11434/api/version
```

## 4. Włączenie plików w Bridge

1. Skopiuj katalogi z paczki do repo.
2. Zastosuj:
   - `Program.AI.patch.txt`
   - `Hydra.Bridge.Api.csproj.patch.txt`
3. Upewnij się, że `appsettings.AI.json` jest kopiowany do outputu.

## 5. Build

```powershell
dotnet restore .\bridge\dotnet\Hydra.Bridge.slnx
dotnet build .\bridge\dotnet\Hydra.Bridge.slnx --configuration Debug
```

## 6. Start

```powershell
dotnet run --project .\bridge\dotnet\src\Hydra.Bridge.Api\Hydra.Bridge.Api.csproj
```

## 7. Test Ollamy

```powershell
.\scripts\Test-Ollama.ps1
```

## 8. Test gRPC

Dostosuj port do `launchSettings.json`:

```powershell
grpcurl -plaintext `
  -d '{
    "input": "Podaj status systemu HYDRA po polsku.",
    "conversationId": "local-test",
    "taskType": "AI_TASK_TYPE_GENERAL_CHAT",
    "allowCloudFallback": false
  }' `
  localhost:5000 `
  hydra.ai.v1.HydraAiService/Generate
```

## Zasady bezpieczeństwa

- Model nie wykonuje komend urządzeń bez walidacji.
- Device control przechodzi przez Rust Core.
- Sekrety i local keys nie trafiają do promptu.
- Ollama działa lokalnie na `127.0.0.1`.

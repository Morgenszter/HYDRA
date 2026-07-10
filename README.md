# HYDRA Bridge Full Code v1

## Zawartość

- pełny `Program.cs`
- kompletne projekty:
  - Hydra.Bridge.Api
  - Hydra.Bridge.Application
  - Hydra.Bridge.Infrastructure
- serwisy gRPC:
  - Runtime
  - Devices
  - Voice
  - AI/Ollama
- repozytoria in-memory
- konfiguracja Ollama
- wszystkie pliki `.proto`
- solution `.slnx`

## Uruchomienie

```powershell
ollama pull qwen3:8b
ollama serve

dotnet restore .\bridge\dotnet\Hydra.Bridge.slnx
dotnet build .\bridge\dotnet\Hydra.Bridge.slnx
dotnet run --project .\bridge\dotnet\src\Hydra.Bridge.Api\Hydra.Bridge.Api.csproj
```

## Port

Domyślnie:
`http://localhost:5000`

## Ważne

Repozytoria Runtime/Devices/Voice są implementacjami in-memory.
To jest pełny, działający Bridge v1, ale jeszcze bez prawdziwego połączenia z Rust Core, BLE, Tapo i Tuya.

# HYDRA.zip — pełny audyt forensyczny v1

Data audytu: 2026-07-10

## 1. Werdykt wykonawczy

Repozytorium nie jest pustym szkieletem. Zawiera działający zalążek pionowego przepływu:

```text
WPF Desktop
  -> gRPC client
.NET 8 Hydra.Bridge.Api
  -> in-memory runtime repository
RuntimeSnapshot
```

Jednocześnie aktualny kod **nie realizuje jeszcze deklarowanej architektury Rust Core jako źródła prawdy**. Rust ma modele i klienta `tonic` do .NET Bridge, natomiast `.NET` hostuje usługę runtime i zwraca dane z implementacji in-memory.

Stan do przyjęcia jako faktyczny baseline:

```text
Public gRPC host: .NET 8 Bridge
Desktop client: WPF .NET 8
Runtime source: .NET in-memory stub
Rust: domain skeleton + gRPC client
Devices: contract only
Voice: rich contract + plans/config, no runtime implementation
Installer: basic WiX MSI skeleton
Mobile: package.json placeholder
Assets: empty registry
```

## 2. Inwentaryzacja

- Pliki źródłowe bez build outputów: **81**
- Pliki C#: **19**
- Pliki Rust: **3**
- Pliki Protobuf: **3**
- Pliki build output (`bin/obj/target`) obecne w ZIP: **1626**

## 3. Moduły — EXISTS / WORKS / STUB / MISSING

| Moduł | Stan | Uzasadnienie |
|---|---|---|
| `Hydra.Bridge.Api` | PARTIAL-WORKING | Host ASP.NET Core gRPC, `GetRuntimeSnapshot`, pojedynczy event stream, command stub. |
| `Hydra.Bridge.Application` | MINIMAL | Model snapshotu i `IHydraRuntimeRepository`; brak devices/voice use-cases. |
| `Hydra.Bridge.Infrastructure` | STUB | `InMemoryHydraRuntimeRepository`; brak klienta Rust Core. |
| `hydra-core` | SKELETON | Modele runtime i dwa błędy; brak state store, command bus, service hosta. |
| `hydra-grpc-client` | PARTIAL | Klient runtime do .NET Bridge; mapowanie `Timestamp -> SystemTime`. |
| `Hydra.Desktop.App` | PARTIAL-WORKING | DI, ViewModel, prosty runtime status UI i lifecycle start Bridge. |
| `Hydra.Desktop.Infrastructure` | PARTIAL | gRPC runtime client działa koncepcyjnie; Windows Service mode jawnie niezaimplementowany. |
| `hydra.runtime.v1.proto` | USABLE-V0 | Snapshot, stream, command; brakuje auth, correlation, typed payloadów i version policy. |
| `hydra.devices.v1.proto` | CONTRACT-ONLY | `value` jako string i capabilities jako stringi są zbyt słabe dla urządzeń produkcyjnych. |
| `hydra.voice.v1.proto` | ADVANCED-CONTRACT | Duży zakres profiles/routes/playback/health; brak implementacji usług. |
| WiX v4 | SKELETON | MSI kopiuje exe/config i skróty; brak Windows Service, prerequisites, recovery, firewall, upgrade discipline. |
| Expo mobile | PLACEHOLDER | Tylko `package.json` z `latest`; brak aplikacji, configu Expo i źródeł. |
| Asset registry | EMPTY | `assets: []`. |
| Voice dataset | SPECIFICATION | Manifest jest dobry jako spec, lecz brak segmentów, transkrypcji i legalnego corpus. |
| Tests | MISSING | Brak projektów testowych i realnej automatycznej walidacji. |
| CI/CD | MISSING | Brak workflow build/test/sign/release. |

## 4. Krytyczne znaleziska

### F-001 — Odwrócona topologia względem deklarowanego „Rust Core”

Aktualnie:

```text
Rust hydra-grpc-client -> .NET HydraRuntimeService
```

Deklarowany kierunek mówi, że Rust ma być Core. Repo nie zawiera serwera `tonic` ani klienta .NET do Rust Core. Należy zamrozić topologię przez ADR.

**Rekomendacja:** zachować .NET jako publiczny gateway, ale uruchomić Rust jako prywatny gRPC service:

```text
WPF -> .NET Public Gateway -> Rust Core private gRPC
```

Wtedy `InMemoryHydraRuntimeRepository` zostaje zastąpione `RustHydraRuntimeRepository`.

### F-002 — `ExecuteCommand` potwierdza komendę bez wykonania

Zwraca `ACCEPTED` oraz tekst „stub”. Nie ma command handlera, idempotency, deadline, ACK, completion event ani audytu.

### F-003 — Event streaming nie jest streamingiem

`StreamRuntimeEvents` wysyła jeden event snapshot-ready i kończy wywołanie. Brakuje channel/broadcast, backpressure, reconnect cursor i heartbeat.

### F-004 — Kontrakty urządzeń są zbyt stringowe

`capability`, `value`, `attributes` jako stringi nie zapewniają walidacji temperatury, jasności, RGB, scen ani trybów. Potrzebne `oneof`/typed values.

### F-005 — Voice contract wyprzedza implementację

Kontrakt jest znacznie dojrzalszy niż kod. Nie ma `HydraVoiceGrpcService`, repository/facade, queue, synthesizer ani output adapterów.

### F-006 — WPF ma poprawne boundary, ale minimalny UI

MVVM i DI są sensowne, lecz ekran jest technicznym shellem, nie docelowym HUD. `MainWindow.xaml.cs` powinien pozostać techniczny; logika musi zostać w usługach/ViewModelach.

### F-007 — Lifecycle Windows Service jest jawnie niedokończony

`EnsureWindowsService()` zwraca błąd. WiX nie instaluje żadnej usługi. Obecny model to proces użytkownika uruchamiany z WPF.

### F-008 — Instalator ma testowe GUID-y

GUID-y typu `11111111...`, `22222222...` nie mogą zostać w wydaniu. Brakuje też `ServiceInstall`, `ServiceControl`, `util:ServiceConfig`, Burn i .NET Desktop Runtime prerequisite.

### F-009 — Repo ZIP zawiera 1626 plików build output

`.gitignore` jest poprawny, ale paczka zawiera `.git`, `bin`, `obj`, `target` i publikacje. Źródłowe archiwum powinno je wykluczać.

### F-010 — Zależności są przypięte częściowo, mobile używa `latest`

`expo/react/react-native/typescript = latest` uniemożliwia reprodukowalny build. Należy pinować wersje kompatybilne z wybranym Expo SDK.

### F-011 — Brak modelu bezpieczeństwa gRPC

Brakuje TLS policy, local transport policy, tokenów, ACL, rate limits, validation i authorization dla komend urządzeń.

### F-012 — Voice dataset jest tylko specyfikacją

Materiały referencyjne z komercyjnych źródeł nie powinny być corpus produkcyjnym. Potrzebne nagrania własne/licencjonowane z transkrypcjami.

## 5. Ocena jakości

| Obszar | Ocena /10 |
|---|---:|
| Kierunek architektoniczny | 8 |
| Kontrakty proto | 6 |
| .NET Bridge | 5 |
| Rust Core | 2 |
| WPF shell | 5 |
| Voice runtime | 2 |
| Asset pipeline | 2 |
| WiX | 3 |
| Mobile | 1 |
| Testy/CI/Security | 1 |
| Reprodukowalność repo | 3 |

## 6. Docelowa topologia po korekcie

```text
Hydra.Desktop (WPF)
       |
       | gRPC localhost/public gateway
       v
Hydra.Bridge.Api (.NET 8)
       |
       | private gRPC
       v
hydra-core-service (Rust tonic)
       |-- device runtime
       |-- voice orchestration
       |-- state/event bus
       |-- Ollama/OpenAI hybrid AI router (policy controlled)
```

## 7. Priorytety naprawy

1. ADR topologii `.NET Gateway -> Rust Core`.
2. Dodać `hydra-core-service` jako serwer tonic.
3. Zastąpić `InMemoryHydraRuntimeRepository` klientem Rust.
4. Utworzyć `Hydra.Contracts`/`hydra-proto` jako kontrolowane generation boundaries.
5. Implementować `GetRuntimeSnapshot` end-to-end.
6. Dodać typed device values i mock device vertical slice.
7. Implementować Voice fallback TTS przed custom modelem.
8. Dodać `hydra.ai.v1.proto` i router Ollama/OpenAI.
9. Dodać testy kontraktowe i integracyjne.
10. Dopiero potem WiX Service/Burn, HUD i mobile.

## 8. Walidacja kompilacji

W środowisku audytowym nie było dostępnych `dotnet`, `cargo` ani WiX CLI, więc nie wykonano świeżej kompilacji. Repo zawiera wcześniejsze `bin/obj/target`, które świadczą o wcześniejszych buildach, ale nie są dowodem reprodukowalnego aktualnego builda.

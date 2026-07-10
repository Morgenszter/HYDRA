# HYDRA_HOME — jak połączyć Ollamę z OpenAI/ChatGPT

## 1. Co znaczy „połączyć Ollamę ze mną”

Nie istnieje bezpieczne połączenie, w którym aplikacja ChatGPT bezpośrednio steruje lokalnym `localhost:11434`. Są dwa poprawne warianty:

### Wariant A — HYDRA jako orchestrator modeli (rekomendowany)
HYDRA wysyła część zadań do lokalnej Ollamy, a część do OpenAI API. Użytkownik widzi jeden interfejs, ale decyzję o providerze podejmuje `HybridModelRouter`.

### Wariant B — ChatGPT -> zdalny MCP -> HYDRA
Tworzysz zdalny MCP server dostępny przez HTTPS. ChatGPT/OpenAI wywołuje jego narzędzia. MCP server komunikuje się z HYDRA przez gRPC. Ollama pozostaje lokalna i nie jest wystawiana do Internetu.

## 2. Minimalny wariant hybrydowy

```text
Hydra.Bridge.Api
  /AI/Chat lub HydraAiService gRPC
       |
       +-- Ollama: http://127.0.0.1:11434/v1/responses
       +-- OpenAI: https://api.openai.com/v1/responses
```

Oba endpointy używają podobnego formatu Responses API, ale nie zakładaj pełnej równoważności funkcji. Przechowuj własny stan rozmowy i normalizuj odpowiedzi.

## 3. Reguły routingu

- `LOCAL_ONLY`: prywatne dane, sterowanie domem, offline.
- `LOCAL_FIRST`: streszczenia logów, klasyfikacja, proste planowanie; cloud fallback po błędzie/timeout.
- `CLOUD_FIRST`: trudne projektowanie, kod, złożone rozumowanie; local fallback.
- `CLOUD_ONLY`: zadania wymagające konkretnych modeli OpenAI.
- `CLASSIFIED`: polityka wybiera provider według klasy danych.

Domyślna polityka HYDRA:

```text
DEVICE_CONTROL       -> LOCAL_ONLY + deterministic tool policy
SECRETS/LOCAL_KEYS   -> NEVER_SEND_TO_MODEL
HOME_TELEMETRY       -> LOCAL_ONLY
CODE_ARCHITECTURE    -> CLOUD_FIRST
GENERAL_CHAT         -> LOCAL_FIRST
VOICE_INTENT         -> LOCAL_ONLY
```

## 4. Instalacja Ollamy — wymagania operacyjne

1. Zainstaluj Ollamę na Windows.
2. Uruchom usługę Ollama.
3. Pobierz wybrany model poleceniem `ollama pull <model>`.
4. Zweryfikuj endpoint `http://127.0.0.1:11434/v1/models`.
5. Nie wystawiaj portu 11434 publicznie.
6. HYDRA powinna łączyć się tylko przez loopback lub prywatny host z firewall ACL.

## 5. Konfiguracja

Użyj `appsettings.AI.json` oraz sekretu środowiskowego `OPENAI_API_KEY`.

- Klucz OpenAI nigdy nie trafia do repo.
- `Ollama.ApiKey` może być wartością techniczną, ponieważ lokalna kompatybilność wymaga pola, ale Ollama je ignoruje.
- Ustaw timeouty osobno dla providerów.

## 6. Narzędzia i sterowanie urządzeniami

Model nigdy nie generuje komendy wykonywanej bezpośrednio. Poprawny przepływ:

```text
model tool_call
 -> schema validation
 -> allowlist
 -> user/automation authorization
 -> Rust Core command
 -> device ACK
 -> tool result
 -> model final response
```

Dla komend mutujących wymagaj jawnej autoryzacji lub policy engine. Ogranicz zakres temperatury, jasności, device IDs i częstotliwość wywołań.

## 7. Połączenie z ChatGPT przez MCP

ChatGPT/OpenAI może używać zdalnego MCP servera dostępnego przez publiczny/trusted HTTPS URL. MCP powinien oferować wąskie narzędzia, np.:

- `hydra_get_runtime_snapshot`
- `hydra_list_devices`
- `hydra_get_device_state`
- `hydra_set_device_state` — approval required
- `hydra_speak_text` — approval/policy controlled

Nie wystawiaj narzędzia typu `execute_arbitrary_command`. Loguj każde wywołanie, argumenty po redakcji sekretów, decyzję policy i wynik.

## 8. Czego nie robić

- Nie otwieraj Ollamy bez uwierzytelnienia na publiczny Internet.
- Nie przekazuj OpenAI kluczy Tuya/Tapo, MAC-ów i pełnych logów lokalnych bez redakcji.
- Nie pozwalaj modelowi wybierać dowolnej metody gRPC.
- Nie używaj odpowiedzi tekstowej jako komendy urządzenia.
- Nie mieszaj kontekstu prywatnego i cloud bez jawnej klasyfikacji.
- Nie uzależniaj podstawowej automatyki domu od dostępności chmury.

## 9. Kolejność implementacji

1. Dodać `hydra.ai.v1.proto`.
2. Dodać `Hydra.Bridge.Application/AI` abstractions.
3. Dodać Ollama client.
4. Dodać OpenAI Responses client.
5. Dodać router i policy classifier.
6. Dodać structured tool calls i allowlist.
7. Dodać testy z mock HTTP handlers.
8. Dodać telemetrykę bez promptów/sekretów.
9. Opcjonalnie dodać remote MCP server.
10. Dopiero potem integrować z WPF/Voice.

## 10. Kryteria akceptacji

- Lokalny chat działa przy odłączonym Internecie.
- Cloud provider nie otrzymuje danych `LOCAL_ONLY`.
- Fallback nie duplikuje tool calli.
- Każda mutacja urządzenia ma correlation ID i ACK.
- Timeout Ollamy nie blokuje UI.
- Klucze nie pojawiają się w logach.
- MCP nie oferuje arbitralnych komend.

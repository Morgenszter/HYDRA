# HYDRA_OMEGON_DATASET_SPEC_V1

Status: ACTIVE

## Cel

OMEGON model ma dawać czysty głos bazowy. Charakter postaci, hełm, rezonans pancerza, EQ, subtelny pitch/formant shaping, pogłos i ambient blend są nakładane przez runtime DSP Voice Huba, nie przez dataset treningowy.

Kanoniczny manifest:

```text
datasets/omegon/dataset.manifest.v1.json
```

## Krok C — czyszczenie

Usuwaj:

- szum tła
- zbyt głośny room tone
- kliknięcia
- fragmenty bez mowy
- muzykę lub podkład, jeśli jest dominujący

Nie rób z głosu plastikowego radia. Zbyt mocne odszumianie zabija naturalność i nie jest akceptowane.

Zachowuj:

- naturalną fakturę głosu
- czytelną artykulację
- czyste, niedominujące oddechy, jeśli są naturalną częścią wypowiedzi

## Krok D — segmentacja

Cel segmentu:

- `3-12` sekund
- jedno sensowne zdanie albo jedna zwarta wypowiedź

Naming:

```text
omegon_0001.wav
omegon_0002.wav
omegon_0003.wav
```

Segmenty trafiają do:

```text
datasets/omegon/segments/train
datasets/omegon/segments/validation
```

Materiał referencyjny jest osobny:

```text
datasets/omegon/reference
```

## Krok E — transkrypcja

Każdy segment musi mieć dokładny tekst. Format CSV:

```csv
file,text
segments/train/omegon_0001.wav,Rozkaz przyjęty.
segments/train/omegon_0002.wav,Hydra Core pozostaje aktywny.
segments/validation/omegon_0003.wav,Przełączam kanał głosowy na wyjście lokalne.
```

Zasada: transkrypcja odpowiada temu, co faktycznie słychać, a nie temu, co miało być powiedziane.

## Podział datasetu

Docelowo:

| Split | Proporcja |
|---|---:|
| `train` | `80-90%` |
| `validation` | `10-20%` |
| `reference` | osobno |

Przykład dla 60 minut:

- 48 minut `train`
- 8 minut `validation`
- 4 minuty `reference`

## Paczki treści

Dataset nie może składać się wyłącznie z fraz typu „Rozkaz przyjęty”. Musi zawierać różne typy wypowiedzi.

### A. Krótkie komendy

- Rozkaz przyjęty.
- Wykonuję.
- Kanał aktywny.
- Połączenie przerwane.
- Tryb głosowy aktywny.

### B. Komunikaty systemowe

- Hydra Core pozostaje w gotowości.
- Wykryto aktywne urządzenie audio.
- Zmieniono trasę wyjścia na telefon.

### C. Odpowiedzi dłuższe

- Wszystkie podsystemy działają prawidłowo. Nie wykryto konfliktów w aktualnej konfiguracji.

### D. Liczby, temperatury, godziny, nazwy urządzeń

- Ustawiono temperaturę na dwadzieścia dwa stopnie.
- Listwa LED została przełączona do grupy drugiej.

### E. Polskie znaki

Materiał musi pokrywać:

```text
ą ę ś ć ł ź ż ó ń
```

## Czego nie trenować w modelu

Nie wkładaj do modelu:

- pogłosu Alphariusa ani innego pogłosu scenicznego
- metalicznego hełmu
- radia
- glitchy
- ambientu
- alarmów
- FX przejść

To ma siedzieć w DSP presetach Voice Huba, nie w samym modelu.

## Odpowiedzialność runtime

Model zapewnia:

- czysty głos bazowy

Voice Hub dokłada:

- helmet vox
- armoured resonance
- EQ
- subtle pitch/formant shaping
- reverb
- ambient blend

Powiązany preset DSP:

```text
hydra.voice-hub.preset.omegon.v1
```

## Acceptance criteria

- Każdy segment ma `3-12` sekund albo ma udokumentowany wyjątek.
- Każdy segment zawiera jedną zwartą wypowiedź.
- `metadata.csv` zawiera ścieżkę i tekst dla każdego segmentu.
- Tekst odpowiada temu, co słychać.
- `reference` nie miesza się z `train` ani `validation`.
- Audio treningowe jest czyste, suche i bez baked FX.
- Dataset pokrywa komendy, komunikaty systemowe, dłuższe odpowiedzi, liczby, temperatury, godziny, nazwy urządzeń i polskie znaki.

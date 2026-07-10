# HYDRA_DEVELOPER_WORKFLOW_V1

Status: ACTIVE

## Commands

From repository root:

```powershell
.\tools\dev\Validate-Hydra.ps1
.\tools\dev\Publish-Hydra.ps1
.\tools\dev\Start-HydraBridge.ps1
.\tools\dev\Start-HydraDesktop.ps1
```

## OMEGON dataset validation

OMEGON voice datasets are validated with the repository-native PowerShell toolchain:

```powershell
.\tools\dev\Validate-OmegonDataset.ps1 -DatasetRoot C:\path\to\OMEGON -ReportPath artifacts\omegon-validation-report.json
```

Expected dataset layout:

```text
OMEGON/
  train/
    metadata.csv
    omegon_0001.wav
  validation/
    metadata.csv
    omegon_0009.wav
  reference/
    metadata.csv
    omegon_0011.wav
```

Each `metadata.csv` must be UTF-8 and use the exact header `file,text`. The validator checks:

- required `train`, `validation`, and `reference` directories,
- required `metadata.csv` files,
- `omegon_0001.wav`-style file names,
- referenced audio file presence,
- no empty transcriptions,
- PCM WAV segment duration between 3 and 12 seconds,
- no unreferenced WAV files,
- train/validation row distribution: train 80-90%, validation 10-20%,
- text and JSON error reports.

Canonical OMEGON material preparation rules:

1. Cleaning keeps the base voice natural. Remove background noise, overly loud room tone, clicks, non-speech fragments, and music or backing tracks when dominant. Do not over-denoise into a plastic radio-like voice.
2. Segmentation target is 3-12 seconds per segment. Each segment should contain one meaningful sentence or one compact utterance.
3. Transcription must match what is actually audible, not what was intended in the script.
4. Dataset split is based on material volume: `train` 80-90%, `validation` 10-20%, and `reference` kept separate for timbre analysis and DSP preset work.
5. Training material should cover varied phrase packs:
   - short commands: `Rozkaz przyjęty.`, `Wykonuję.`, `Kanał aktywny.`,
   - system messages: `Hydra Core pozostaje w gotowości.`, `Wykryto aktywne urządzenie audio.`,
   - longer responses: complete operational status sentences,
   - numbers, temperatures, times, and device names,
   - Polish diacritics: `ą`, `ę`, `ś`, `ć`, `ł`, `ź`, `ż`, `ó`, `ń`.
6. Do not train Alpharius-style reverb, metallic helmet tone, radio coloration, glitches, ambience, alarms, or transition FX into the base model. Those belong in Voice Hub DSP presets: helmet vox, armoured resonance, EQ, subtle pitch/formant shaping, reverb, and ambient blend.

Run the minimal smoke fixture test with:

```powershell
.\tools\dev\Test-OmegonDatasetValidator.ps1
```

## Runtime flow

1. Start .NET gRPC bridge.
2. Start WPF desktop shell.
3. WPF resolves `IHydraRuntimeService` through DI.
4. Infrastructure uses `Grpc.Net.Client` and generated Protobuf client.
5. Bridge serves `HydraRuntimeGrpcService` through `Grpc.AspNetCore`.

## Publish and installer flow

1. Run `Publish-Hydra.ps1`.
2. Build `installer\wix\Hydra.Installer.wixproj`.
3. WiX packages framework-dependent single-file EXE outputs from `artifacts\publish`.

## Forbidden

Do not add:

- Python
- FastAPI
- Flask
- PyInstaller
- Inno Setup
- `.iss`

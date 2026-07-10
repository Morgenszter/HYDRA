# HYDRA_VOICE_HUB_DSP_PRESETS_V1

Status: ACTIVE

## Principle

Voice Hub presets define runtime DSP identity layers. The trained voice model must output a clean, dry, intelligible base voice. OMEGON character processing is applied after model inference by Voice Hub DSP only.

Do not move these elements into the training dataset:

- reverb
- metallic helmet coloration
- radio band-limiting, radio squelch or radio noise
- glitches
- ambient beds
- alarms
- transition effects
- impact effects

## Registry file

Canonical preset registry:

```text
assets/voice-hub/hydra.voice-hub.dsp-presets.v1.json
```

The file follows the repository's existing JSON registry style:

```json
{
  "id": "hydra.voice-hub.dsp-presets.v1",
  "version": 1,
  "presets": []
}
```

## Preset fields

| Field | Purpose |
|---|---|
| `id` | Stable semantic preset id. Must be unique. |
| `profile` | Runtime voice profile name, e.g. `OMEGON`. |
| `displayName` | Human-readable name for tools/UI. |
| `status` | `active`, `candidate`, or `disabled`. |
| `intent` | Short description of the sonic target and ownership boundary. |
| `datasetPolicy` | Rules for clean training data and forbidden baked effects. |
| `signalChain` | Ordered runtime DSP stages. |
| `validation` | Machine-checkable safety flags and wet-FX ceiling. |

## Dataset policy

| Field | Meaning |
|---|---|
| `baseVoice` | Must be `clean` for OMEGON. |
| `allowedInTrainingDataset` | Dry speech traits that may be present in source data. |
| `forbiddenInTrainingDataset` | Effects and scene layers that must remain outside training clips. |
| `notes` | Human-readable policy note. |

## DSP stage parameters

### `input_conditioning`

Prepares the clean model output before character processing.

| Parameter | Unit/range | Description |
|---|---:|---|
| `inputGainDb` | dB | Input trim before DSP. |
| `highPassHz` | Hz | Removes rumble before armour/helmet layers. |
| `deEssAmount` | `0.0-1.0` | Reduces harsh sibilance. |
| `noiseGateThresholdDb` | dB | Gate floor for quiet gaps. |
| `softClipCeilingDb` | dBFS | Safety ceiling before subsequent stages. |

### `eq`

Shapes clarity and authority without baking identity into the model.

| Parameter | Unit/range | Description |
|---|---:|---|
| `lowShelfDb`, `lowShelfHz` | dB/Hz | Adds controlled body. |
| `lowMidCutDb`, `lowMidCutHz` | dB/Hz | Prevents boxiness. |
| `presenceBoostDb`, `presenceBoostHz` | dB/Hz | Preserves command intelligibility. |
| `airShelfDb`, `airShelfHz` | dB/Hz | Adds slight top-end clarity. |

### `pitch_formant`

Subtle mass and formant shaping.

| Parameter | Unit/range | Description |
|---|---:|---|
| `pitchShiftSemitones` | semitones | Small pitch offset; keep subtle. |
| `formantShiftRatio` | ratio | Slightly lowers apparent vocal tract size. |
| `formantBlend` | `0.0-1.0` | Wet amount for formant shift. |
| `transientPreserve` | `0.0-1.0` | Protects consonants and attack clarity. |

### `armoured_resonance`

Adds armour body/plate resonance as a runtime layer.

| Parameter | Unit/range | Description |
|---|---:|---|
| `resonanceMix` | `0.0-1.0` | Wet amount of armour resonance. |
| `bodyFrequencyHz` | Hz | Low armour/body resonance focus. |
| `plateFrequencyHz` | Hz | Metallic plate resonance focus. |
| `damping` | `0.0-1.0` | Higher values reduce ringing. |
| `width` | `0.0-1.0` | Stereo/spatial width of resonance. |

### `helmet_vox`

Adds helmet enclosure and visor reflections after model inference.

| Parameter | Unit/range | Description |
|---|---:|---|
| `mix` | `0.0-1.0` | Wet amount of helmet coloration. |
| `enclosureSize` | `0.0-1.0` | Apparent helmet cavity size. |
| `metallicReflection` | `0.0-1.0` | Metallic visor/plate reflection amount. |
| `combDepth` | `0.0-1.0` | Comb-filter depth; keep low for intelligibility. |
| `visorDamping` | `0.0-1.0` | High-frequency damping inside the helmet. |
| `radioBleed` | `0.0-1.0` | Must remain `0.0` for OMEGON baseline; radio is not part of this preset. |

### `reverb`

Short runtime room/enclosure reverb.

| Parameter | Unit/range | Description |
|---|---:|---|
| `mix` | `0.0-1.0` | Wet amount. |
| `preDelayMs` | ms | Delay before early reflections. |
| `decayMs` | ms | Reverb tail length. |
| `lowCutHz`, `highCutHz` | Hz | Band limits the reverb return. |
| `earlyReflections` | `0.0-1.0` | Early reflection emphasis. |

### `ambient_blend`

Runtime-only scene bed mixed under speech.

| Parameter | Unit/range | Description |
|---|---:|---|
| `mix` | `0.0-1.0` | Ambient wet amount. |
| `duckingDb` | dB | Speech-sidechain attenuation applied to the bed. |
| `sidechainReleaseMs` | ms | Recovery after speech. |
| `bedId` | string | Semantic id of the runtime ambient bed. |

### `output_limiter`

Final level safety.

| Parameter | Unit/range | Description |
|---|---:|---|
| `ceilingDb` | dBFS | Output ceiling. |
| `releaseMs` | ms | Limiter release time. |
| `targetLufs` | LUFS | Target loudness for normalized output. |

## Validation rules

`tools/dev/Validate-Hydra.ps1` validates the preset registry for:

- registry id/version shape
- unique preset ids
- required OMEGON clean-base dataset policy
- forbidden training dataset terms
- required DSP stages
- normalized mix/blend values in `0.0-1.0`
- no OMEGON radio bleed in the baseline helmet stage
- total wet FX mix not exceeding `validation.maxTotalWetFxMix`

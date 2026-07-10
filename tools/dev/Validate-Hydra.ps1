$ErrorActionPreference = "Stop"
$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Push-Location $root
try {
    $voiceHubPresetPath = "assets\voice-hub\hydra.voice-hub.dsp-presets.v1.json"
    if (-not (Test-Path $voiceHubPresetPath)) {
        throw "Missing Voice Hub DSP preset registry: $voiceHubPresetPath"
    }

    $voiceHubPresetRegistry = Get-Content $voiceHubPresetPath -Raw | ConvertFrom-Json
    if ($voiceHubPresetRegistry.id -ne "hydra.voice-hub.dsp-presets.v1") {
        throw "Invalid Voice Hub DSP preset registry id."
    }

    if ($voiceHubPresetRegistry.version -ne 1) {
        throw "Invalid Voice Hub DSP preset registry version."
    }

    if (-not $voiceHubPresetRegistry.presets -or $voiceHubPresetRegistry.presets.Count -lt 1) {
        throw "Voice Hub DSP preset registry must contain at least one preset."
    }

    $presetIds = @{}
    $requiredOmegonStages = @(
        "input_conditioning",
        "eq",
        "pitch_formant",
        "armoured_resonance",
        "helmet_vox",
        "reverb",
        "ambient_blend",
        "output_limiter"
    )

    $forbiddenDatasetTerms = @(
        "reverb",
        "metallic helmet coloration",
        "radio band-limiting",
        "radio squelch",
        "glitches",
        "ambient beds",
        "alarms",
        "transition effects",
        "impact effects"
    )

    foreach ($preset in $voiceHubPresetRegistry.presets) {
        if ([string]::IsNullOrWhiteSpace($preset.id)) {
            throw "Voice Hub DSP preset id cannot be empty."
        }

        if ($presetIds.ContainsKey($preset.id)) {
            throw "Duplicate Voice Hub DSP preset id: $($preset.id)"
        }

        $presetIds[$preset.id] = $true

        if ($preset.status -notin @("active", "candidate", "disabled")) {
            throw "Invalid Voice Hub DSP preset status for $($preset.id): $($preset.status)"
        }

        if (-not $preset.datasetPolicy) {
            throw "Missing datasetPolicy for Voice Hub DSP preset: $($preset.id)"
        }

        if ($preset.profile -eq "OMEGON") {
            if ($preset.datasetPolicy.baseVoice -ne "clean") {
                throw "OMEGON preset must keep datasetPolicy.baseVoice set to clean."
            }

            if (-not $preset.validation.requiresCleanBaseVoice -or -not $preset.validation.requiresRuntimeOnlyFx -or -not $preset.validation.forbidDatasetFxLeakage) {
                throw "OMEGON preset must require clean base voice, runtime-only FX, and dataset FX leakage prevention."
            }

            foreach ($term in $forbiddenDatasetTerms) {
                if ($preset.datasetPolicy.forbiddenInTrainingDataset -notcontains $term) {
                    throw "OMEGON preset dataset policy must forbid '$term'."
                }
            }

            $stages = @($preset.signalChain | ForEach-Object { $_.stage })
            foreach ($stage in $requiredOmegonStages) {
                if ($stages -notcontains $stage) {
                    throw "OMEGON preset is missing required DSP stage: $stage"
                }
            }

            $totalWetFxMix = 0.0
            foreach ($stage in $preset.signalChain) {
                if (-not $stage.enabled) {
                    continue
                }

                foreach ($property in $stage.parameters.PSObject.Properties) {
                    if ($property.Name -match '^(mix|.*Mix|.*Blend|resonanceMix|formantBlend)$') {
                        $value = [double]$property.Value
                        if ($value -lt 0.0 -or $value -gt 1.0) {
                            throw "OMEGON stage $($stage.stage) parameter $($property.Name) must be between 0.0 and 1.0."
                        }
                    }
                }

                if ($stage.stage -in @("armoured_resonance", "helmet_vox", "reverb", "ambient_blend")) {
                    if ($stage.parameters.PSObject.Properties.Name -contains "mix") {
                        $totalWetFxMix += [double]$stage.parameters.mix
                    }
                    elseif ($stage.parameters.PSObject.Properties.Name -contains "resonanceMix") {
                        $totalWetFxMix += [double]$stage.parameters.resonanceMix
                    }
                }

                if ($stage.stage -eq "helmet_vox" -and [double]$stage.parameters.radioBleed -ne 0.0) {
                    throw "OMEGON baseline helmet_vox.radioBleed must remain 0.0."
                }
            }

            if ($totalWetFxMix -gt [double]$preset.validation.maxTotalWetFxMix) {
                throw "OMEGON total wet FX mix $totalWetFxMix exceeds maxTotalWetFxMix $($preset.validation.maxTotalWetFxMix)."
            }
        }
    }

    $omegonDatasetManifestPath = "datasets\omegon\dataset.manifest.v1.json"
    if (-not (Test-Path $omegonDatasetManifestPath)) {
        throw "Missing OMEGON dataset manifest: $omegonDatasetManifestPath"
    }

    $omegonDatasetManifest = Get-Content $omegonDatasetManifestPath -Raw | ConvertFrom-Json
    if ($omegonDatasetManifest.id -ne "hydra.dataset.omegon.v1") {
        throw "Invalid OMEGON dataset manifest id."
    }

    if ($omegonDatasetManifest.version -ne 1) {
        throw "Invalid OMEGON dataset manifest version."
    }

    if ($omegonDatasetManifest.profile -ne "OMEGON") {
        throw "OMEGON dataset manifest profile must be OMEGON."
    }

    if ($omegonDatasetManifest.metadata.format -ne "csv" -or $omegonDatasetManifest.metadata.path -ne "metadata.csv") {
        throw "OMEGON dataset metadata must use metadata.csv in CSV format."
    }

    foreach ($column in @("file", "text")) {
        if ($omegonDatasetManifest.metadata.columns -notcontains $column) {
            throw "OMEGON dataset metadata is missing required column: $column"
        }
    }

    if ([int]$omegonDatasetManifest.segmentation.targetMinSeconds -ne 3 -or [int]$omegonDatasetManifest.segmentation.targetMaxSeconds -ne 12) {
        throw "OMEGON dataset segmentation target must be 3-12 seconds."
    }

    $trainRange = @($omegonDatasetManifest.split.trainPercentRange)
    $validationRange = @($omegonDatasetManifest.split.validationPercentRange)
    if ([int]$trainRange[0] -ne 80 -or [int]$trainRange[1] -ne 90) {
        throw "OMEGON dataset train split range must be 80-90%."
    }

    if ([int]$validationRange[0] -ne 10 -or [int]$validationRange[1] -ne 20) {
        throw "OMEGON dataset validation split range must be 10-20%."
    }

    foreach ($term in $forbiddenDatasetTerms) {
        if ($omegonDatasetManifest.forbiddenTrainingAudio -notcontains $term) {
            throw "OMEGON dataset manifest must forbid training audio term '$term'."
        }
    }

    foreach ($packId in @("short_commands", "system_messages", "long_responses", "numbers_times_devices", "polish_diacritics")) {
        if (-not ($omegonDatasetManifest.contentPacks | Where-Object { $_.id -eq $packId -and $_.required })) {
            throw "OMEGON dataset manifest is missing required content pack: $packId"
        }
    }

    $diacriticsPack = $omegonDatasetManifest.contentPacks | Where-Object { $_.id -eq "polish_diacritics" }
    foreach ($character in @("ą", "ę", "ś", "ć", "ł", "ź", "ż", "ó", "ń")) {
        if ($diacriticsPack.requiredCharacters -notcontains $character) {
            throw "OMEGON dataset manifest must require Polish character: $character"
        }
    }

    if ($omegonDatasetManifest.runtimeOwnership.dspPresetId -ne "hydra.voice-hub.preset.omegon.v1") {
        throw "OMEGON dataset manifest must reference the canonical OMEGON Voice Hub DSP preset."
    }

    cargo check --manifest-path core\rust\Cargo.toml
    dotnet build bridge\dotnet\Hydra.Bridge.sln -v:minimal
    dotnet build desktop\wpf\Hydra.Desktop.sln -v:minimal
    & (Join-Path $PSScriptRoot "Publish-Hydra.ps1")
    dotnet build installer\wix\Hydra.Installer.wixproj -v:minimal

    $forbidden = Get-ChildItem -Force -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object {
            $_.FullName -notmatch '\\(bin|obj|target)\\' -and
            $_.Name -match '\.py$|\.iss$|requirements\.txt|pyproject\.toml'
        }

    if ($forbidden) {
        $forbidden | Format-Table FullName -AutoSize
        throw "Forbidden active source files detected."
    }
}
finally {
    Pop-Location
}

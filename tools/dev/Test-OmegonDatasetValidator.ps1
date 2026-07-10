$ErrorActionPreference = "Stop"

function New-TestWavFile {
    param(
        [string]$Path,
        [double]$DurationSeconds = 3.0,
        [int]$SampleRate = 16000
    )

    $channelCount = 1
    $bitsPerSample = 16
    $bytesPerSample = [int]($bitsPerSample / 8)
    $sampleCount = [int]($DurationSeconds * $SampleRate)
    $dataSize = $sampleCount * $channelCount * $bytesPerSample
    $byteRate = $SampleRate * $channelCount * $bytesPerSample
    $blockAlign = $channelCount * $bytesPerSample

    $directory = Split-Path -Parent $Path
    if ($directory) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    $stream = [System.IO.File]::Create($Path)
    $writer = [System.IO.BinaryWriter]::new($stream, [System.Text.Encoding]::ASCII)

    try {
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes("RIFF"))
        $writer.Write([uint32](36 + $dataSize))
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes("WAVE"))
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes("fmt "))
        $writer.Write([uint32]16)
        $writer.Write([uint16]1)
        $writer.Write([uint16]$channelCount)
        $writer.Write([uint32]$SampleRate)
        $writer.Write([uint32]$byteRate)
        $writer.Write([uint16]$blockAlign)
        $writer.Write([uint16]$bitsPerSample)
        $writer.Write([System.Text.Encoding]::ASCII.GetBytes("data"))
        $writer.Write([uint32]$dataSize)
        $writer.Write([byte[]]::new($dataSize))
    }
    finally {
        $writer.Dispose()
        $stream.Dispose()
    }
}

function New-MetadataFile {
    param(
        [string]$Path,
        [string[]]$Rows
    )

    $content = @("file,text") + $Rows
    $content | Set-Content -Path $Path -Encoding utf8
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$validator = Join-Path $repoRoot "tools\dev\Validate-OmegonDataset.ps1"
$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("hydra-omegon-fixture-" + [Guid]::NewGuid().ToString("N"))

try {
    foreach ($split in @("train", "validation", "reference")) {
        New-Item -ItemType Directory -Force -Path (Join-Path $fixtureRoot $split) | Out-Null
    }

    for ($index = 1; $index -le 8; $index++) {
        $name = "omegon_{0:D4}.wav" -f $index
        New-TestWavFile -Path (Join-Path $fixtureRoot "train\$name")
    }

    for ($index = 9; $index -le 10; $index++) {
        $name = "omegon_{0:D4}.wav" -f $index
        New-TestWavFile -Path (Join-Path $fixtureRoot "validation\$name")
    }

    New-TestWavFile -Path (Join-Path $fixtureRoot "reference\omegon_0011.wav")

    New-MetadataFile -Path (Join-Path $fixtureRoot "train\metadata.csv") -Rows @(
        "omegon_0001.wav,train sample 1",
        "omegon_0002.wav,train sample 2",
        "omegon_0003.wav,train sample 3",
        "omegon_0004.wav,train sample 4",
        "omegon_0005.wav,train sample 5",
        "omegon_0006.wav,train sample 6",
        "omegon_0007.wav,train sample 7",
        "omegon_0008.wav,train sample 8"
    )

    New-MetadataFile -Path (Join-Path $fixtureRoot "validation\metadata.csv") -Rows @(
        "omegon_0009.wav,validation sample 1",
        "omegon_0010.wav,validation sample 2"
    )

    New-MetadataFile -Path (Join-Path $fixtureRoot "reference\metadata.csv") -Rows @(
        "omegon_0011.wav,reference sample"
    )

    & $validator -DatasetRoot $fixtureRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Expected valid OMEGON fixture to pass, but validator exited with $LASTEXITCODE."
    }

    Write-Host "OMEGON validator smoke test passed."
}
finally {
    Remove-Item -Recurse -Force -Path $fixtureRoot -ErrorAction SilentlyContinue
}
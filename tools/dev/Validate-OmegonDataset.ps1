param(
    [Parameter(Mandatory = $true)]
    [string]$DatasetRoot,

    [string]$ReportPath
)

$ErrorActionPreference = "Stop"

$RequiredSplits = @("train", "validation", "reference")
$RatioSplits = @("train", "validation")
$FileNamePattern = '^omegon_[0-9]{4}\.wav$'
$MinimumDurationSeconds = 3.0
$MaximumDurationSeconds = 12.0

function New-OmegonIssue {
    param(
        [string]$Code,
        [string]$Path,
        [string]$Message
    )

    [pscustomobject]@{
        code = $Code
        path = $Path
        message = $Message
    }
}

function Read-Utf8TextStrict {
    param([string]$Path)

    $utf8Strict = [System.Text.UTF8Encoding]::new($false, $true)
    return [System.IO.File]::ReadAllText($Path, $utf8Strict)
}

function Read-LittleEndianUInt16 {
    param(
        [byte[]]$Bytes,
        [int]$Offset
    )

    return [System.BitConverter]::ToUInt16($Bytes, $Offset)
}

function Read-LittleEndianUInt32 {
    param(
        [byte[]]$Bytes,
        [int]$Offset
    )

    return [System.BitConverter]::ToUInt32($Bytes, $Offset)
}

function Get-WavDurationSeconds {
    param([string]$Path)

    $bytes = [System.IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -lt 44) {
        throw "WAV file is shorter than the RIFF/WAVE header."
    }

    $riff = [System.Text.Encoding]::ASCII.GetString($bytes, 0, 4)
    $wave = [System.Text.Encoding]::ASCII.GetString($bytes, 8, 4)
    if ($riff -ne "RIFF" -or $wave -ne "WAVE") {
        throw "File is not a RIFF/WAVE file."
    }

    $offset = 12
    $byteRate = $null
    $dataSize = $null

    while ($offset + 8 -le $bytes.Length) {
        $chunkId = [System.Text.Encoding]::ASCII.GetString($bytes, $offset, 4)
        $chunkSize = [int](Read-LittleEndianUInt32 -Bytes $bytes -Offset ($offset + 4))
        $chunkDataOffset = $offset + 8

        if ($chunkDataOffset + $chunkSize -gt $bytes.Length) {
            throw "WAV chunk '$chunkId' exceeds file size."
        }

        if ($chunkId -eq "fmt ") {
            if ($chunkSize -lt 16) {
                throw "WAV fmt chunk is too short."
            }

            $audioFormat = Read-LittleEndianUInt16 -Bytes $bytes -Offset $chunkDataOffset
            if ($audioFormat -ne 1) {
                throw "Only PCM WAV files are supported by the validator; detected format $audioFormat."
            }

            $byteRate = [double](Read-LittleEndianUInt32 -Bytes $bytes -Offset ($chunkDataOffset + 8))
        }
        elseif ($chunkId -eq "data") {
            $dataSize = [double]$chunkSize
        }

        $offset = $chunkDataOffset + $chunkSize
        if (($chunkSize % 2) -eq 1) {
            $offset += 1
        }
    }

    if ($null -eq $byteRate -or $byteRate -le 0) {
        throw "WAV fmt chunk with a valid byte rate was not found."
    }

    if ($null -eq $dataSize) {
        throw "WAV data chunk was not found."
    }

    return $dataSize / $byteRate
}

function ConvertFrom-OmegonCsv {
    param(
        [string]$Path,
        [System.Collections.Generic.List[object]]$Issues
    )

    try {
        $content = Read-Utf8TextStrict -Path $Path
    }
    catch {
        $Issues.Add((New-OmegonIssue -Code "metadata.encoding" -Path $Path -Message "metadata.csv must be valid UTF-8: $($_.Exception.Message)"))
        return @()
    }

    $firstLine = ($content -split "`r?`n", 2)[0]
    if ($firstLine -ne "file,text") {
        $Issues.Add((New-OmegonIssue -Code "metadata.header" -Path $Path -Message "metadata.csv header must be exactly 'file,text'."))
        return @()
    }

    try {
        $rows = $content | ConvertFrom-Csv
    }
    catch {
        $Issues.Add((New-OmegonIssue -Code "metadata.csv" -Path $Path -Message "metadata.csv cannot be parsed: $($_.Exception.Message)"))
        return @()
    }

    return @($rows)
}

$issues = [System.Collections.Generic.List[object]]::new()
$datasetPath = $null

try {
    $datasetPath = (Resolve-Path -LiteralPath $DatasetRoot).Path
}
catch {
    $issues.Add((New-OmegonIssue -Code "dataset.root" -Path $DatasetRoot -Message "Dataset root does not exist."))
}

$splitCounts = @{
    train = 0
    validation = 0
}

$seenFiles = @{}

if ($datasetPath) {
    foreach ($split in $RequiredSplits) {
        $splitPath = Join-Path $datasetPath $split
        if (-not (Test-Path -LiteralPath $splitPath -PathType Container)) {
            $issues.Add((New-OmegonIssue -Code "directory.missing" -Path $splitPath -Message "Required '$split' directory is missing."))
            continue
        }

        $metadataPath = Join-Path $splitPath "metadata.csv"
        if (-not (Test-Path -LiteralPath $metadataPath -PathType Leaf)) {
            $issues.Add((New-OmegonIssue -Code "metadata.missing" -Path $metadataPath -Message "Required metadata.csv is missing."))
            continue
        }

        $rows = ConvertFrom-OmegonCsv -Path $metadataPath -Issues $issues
        if ($RatioSplits -contains $split) {
            $splitCounts[$split] = $rows.Count
        }

        for ($index = 0; $index -lt $rows.Count; $index++) {
            $row = $rows[$index]
            $rowNumber = $index + 2
            $file = [string]$row.file
            $text = [string]$row.text
            $rowPath = "$metadataPath#$rowNumber"

            if ([string]::IsNullOrWhiteSpace($file)) {
                $issues.Add((New-OmegonIssue -Code "metadata.file.empty" -Path $rowPath -Message "The file column must not be empty."))
                continue
            }

            if ($file -notmatch $FileNamePattern) {
                $issues.Add((New-OmegonIssue -Code "file.name" -Path $rowPath -Message "File '$file' must match omegon_0001.wav naming pattern."))
            }

            if ([string]::IsNullOrWhiteSpace($text)) {
                $issues.Add((New-OmegonIssue -Code "transcript.empty" -Path $rowPath -Message "Transcript for '$file' must not be empty."))
            }

            $key = $file.ToLowerInvariant()
            if ($seenFiles.ContainsKey($key)) {
                $issues.Add((New-OmegonIssue -Code "file.duplicate" -Path $rowPath -Message "File '$file' is duplicated; first seen at $($seenFiles[$key])."))
            }
            else {
                $seenFiles[$key] = $rowPath
            }

            $audioPath = Join-Path $splitPath $file
            if (-not (Test-Path -LiteralPath $audioPath -PathType Leaf)) {
                $issues.Add((New-OmegonIssue -Code "file.missing" -Path $audioPath -Message "Audio file referenced by metadata.csv is missing."))
                continue
            }

            try {
                $duration = Get-WavDurationSeconds -Path $audioPath
                if ($duration -lt $MinimumDurationSeconds -or $duration -gt $MaximumDurationSeconds) {
                    $roundedDuration = [Math]::Round($duration, 3)
                    $issues.Add((New-OmegonIssue -Code "audio.duration" -Path $audioPath -Message "Segment duration is $roundedDuration seconds; expected 3-12 seconds."))
                }
            }
            catch {
                $issues.Add((New-OmegonIssue -Code "audio.wav" -Path $audioPath -Message "Cannot validate WAV duration: $($_.Exception.Message)"))
            }
        }

        $metadataFiles = @($rows | ForEach-Object { ([string]$_.file).ToLowerInvariant() })
        $unreferencedAudio = Get-ChildItem -LiteralPath $splitPath -File -Filter "*.wav" -ErrorAction SilentlyContinue |
            Where-Object { $metadataFiles -notcontains $_.Name.ToLowerInvariant() }

        foreach ($audio in $unreferencedAudio) {
            $issues.Add((New-OmegonIssue -Code "file.unreferenced" -Path $audio.FullName -Message "Audio file is present but not listed in metadata.csv."))
        }
    }

    $ratioTotal = $splitCounts.train + $splitCounts.validation
    if ($ratioTotal -le 0) {
        $issues.Add((New-OmegonIssue -Code "split.empty" -Path $datasetPath -Message "Train and validation metadata contain no rows."))
    }
    else {
        $trainPercent = ($splitCounts.train / $ratioTotal) * 100.0
        $validationPercent = ($splitCounts.validation / $ratioTotal) * 100.0

        if ($trainPercent -lt 80.0 -or $trainPercent -gt 90.0) {
            $issues.Add((New-OmegonIssue -Code "split.train_ratio" -Path (Join-Path $datasetPath "train") -Message ("Train split is {0:N2}% of train+validation rows; expected 80-90%." -f $trainPercent)))
        }

        if ($validationPercent -lt 10.0 -or $validationPercent -gt 20.0) {
            $issues.Add((New-OmegonIssue -Code "split.validation_ratio" -Path (Join-Path $datasetPath "validation") -Message ("Validation split is {0:N2}% of train+validation rows; expected 10-20%." -f $validationPercent)))
        }
    }
}

$report = [pscustomobject]@{
    datasetRoot = $DatasetRoot
    checkedAtUtc = [DateTime]::UtcNow.ToString("o")
    splitCounts = [pscustomobject]@{
        train = $splitCounts.train
        validation = $splitCounts.validation
    }
    issueCount = $issues.Count
    issues = @($issues)
}

if ($ReportPath) {
    $reportDirectory = Split-Path -Parent $ReportPath
    if ($reportDirectory) {
        New-Item -ItemType Directory -Force -Path $reportDirectory | Out-Null
    }

    $report | ConvertTo-Json -Depth 8 | Set-Content -Path $ReportPath -Encoding utf8
}

if ($issues.Count -eq 0) {
    Write-Host "OMEGON dataset validation passed."
    Write-Host "train=$($splitCounts.train), validation=$($splitCounts.validation)"
    exit 0
}

Write-Host "OMEGON dataset validation failed with $($issues.Count) issue(s)."
$issues | Format-Table code, path, message -AutoSize
exit 1
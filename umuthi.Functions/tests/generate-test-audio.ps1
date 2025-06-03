# Script to generate test audio files for function testing

$outputDir = $PSScriptRoot
$wavOutputPath = Join-Path $outputDir "sample.wav"
$mp3OutputPath = Join-Path $outputDir "sample.mp3"
$mp4OutputPath = Join-Path $outputDir "sample.mp4"

function Download-SampleAudio {
    param (
        [string]$url,
        [string]$outputPath
    )
    
    if (-not (Test-Path $outputPath)) {
        Write-Host "Downloading sample audio to $outputPath..." -ForegroundColor Cyan
        
        try {
            Invoke-WebRequest -Uri $url -OutFile $outputPath
            if (Test-Path $outputPath) {
                Write-Host "Downloaded successfully: $outputPath" -ForegroundColor Green
            }
        }
        catch {
            Write-Host "Failed to download: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "File already exists: $outputPath" -ForegroundColor Yellow
    }
}

# Download sample audio files (these are Creative Commons audio files)
Download-SampleAudio -url "https://www2.cs.uic.edu/~i101/SoundFiles/gettysburg.wav" -outputPath $wavOutputPath
Download-SampleAudio -url "https://www2.cs.uic.edu/~i101/SoundFiles/gettysburg10.mp3" -outputPath $mp3OutputPath

# For MP4, we'll create a simple silent video with ffmpeg if available
$ffmpegPath = (Get-Command ffmpeg -ErrorAction SilentlyContinue).Source

if ($ffmpegPath) {
    if (-not (Test-Path $mp4OutputPath)) {
        Write-Host "Generating sample MP4 using ffmpeg..." -ForegroundColor Cyan
        # Create a 5 second silent MP4 file
        & ffmpeg -f lavfi -i anullsrc=r=44100:cl=mono -t 5 -c:a aac -strict experimental $mp4OutputPath
        if (Test-Path $mp4OutputPath) {
            Write-Host "Created sample MP4: $mp4OutputPath" -ForegroundColor Green
        }
    }
    else {
        Write-Host "MP4 file already exists: $mp4OutputPath" -ForegroundColor Yellow
    }
}
else {
    Write-Host "ffmpeg not found. Cannot generate MP4 sample. Please install ffmpeg or download a sample MP4 manually." -ForegroundColor Yellow
}

Write-Host "`nTest files ready. Use test-audio-conversion.ps1 to test the functions." -ForegroundColor Cyan

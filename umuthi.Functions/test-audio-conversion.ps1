# Test Script for MPEG to Transcript Function
# This script tests the audio conversion and transcription functionality

# API Key for authentication
$apiKey = "umuthi-dev-api-key"
$apiKeyHeader = @{"x-api-key" = $apiKey}

# 1. Test WAV to MP3 conversion
Write-Host "Testing WAV to MP3 conversion..." -ForegroundColor Cyan
$wavFile = "sample.wav"
$mp3OutputFile = "converted_sample.mp3"

if (-not (Test-Path $wavFile)) {
    Write-Host "Sample WAV file not found. Please create or download a sample WAV file first." -ForegroundColor Yellow
}
else {
    Invoke-RestMethod -Uri "http://localhost:7071/api/ConvertWavToMp3" -Method Post -InFile $wavFile -ContentType "multipart/form-data" -Headers $apiKeyHeader -OutFile $mp3OutputFile
    if (Test-Path $mp3OutputFile) {
        Write-Host "WAV to MP3 conversion successful: $mp3OutputFile" -ForegroundColor Green
    }
    else {
        Write-Host "WAV to MP3 conversion failed" -ForegroundColor Red
    }
}

# 2. Test MPEG to MP3 conversion
Write-Host "`nTesting MPEG to MP3 conversion..." -ForegroundColor Cyan
$mpegFile = "sample.mp4" # Change to your sample MPEG file
$mp3OutputFile2 = "converted_mpeg.mp3"

if (-not (Test-Path $mpegFile)) {
    Write-Host "Sample MPEG file not found. Please create or download a sample MPEG file first." -ForegroundColor Yellow
}
else {
    Invoke-RestMethod -Uri "http://localhost:7071/api/ConvertMpegToMp3" -Method Post -InFile $mpegFile -ContentType "multipart/form-data" -Headers $apiKeyHeader -OutFile $mp3OutputFile2
    if (Test-Path $mp3OutputFile2) {
        Write-Host "MPEG to MP3 conversion successful: $mp3OutputFile2" -ForegroundColor Green
    }
    else {
        Write-Host "MPEG to MP3 conversion failed" -ForegroundColor Red
    }
}

# 3. Test MPEG to Transcript conversion
Write-Host "`nTesting MPEG to Transcript conversion..." -ForegroundColor Cyan
$audioFile = "sample.mp3" # Change to your sample audio file
$transcriptFile = "transcript.txt"

if (-not (Test-Path $audioFile)) {
    Write-Host "Sample audio file not found. Please create or download a sample audio file first." -ForegroundColor Yellow
}
else {
    Invoke-RestMethod -Uri "http://localhost:7071/api/ConvertMpegToTranscript?language=en-US" -Method Post -InFile $audioFile -ContentType "multipart/form-data" -Headers $apiKeyHeader -OutFile $transcriptFile
    if (Test-Path $transcriptFile) {
        Write-Host "Audio to Transcript conversion successful:" -ForegroundColor Green
        Get-Content $transcriptFile | Select-Object -First 10 | ForEach-Object { Write-Host "  $_" -ForegroundColor Cyan }
        if ((Get-Content $transcriptFile | Measure-Object -Line).Lines -gt 10) {
            Write-Host "  ..." -ForegroundColor Cyan
        }
    }
    else {
        Write-Host "Audio to Transcript conversion failed" -ForegroundColor Red
    }
}

# 4. Test with timestamps
Write-Host "`nTesting MPEG to Transcript with timestamps..." -ForegroundColor Cyan
$timestampFile = "transcript_with_timestamps.json"

if (-not (Test-Path $audioFile)) {
    Write-Host "Sample audio file not found. Please create or download a sample audio file first." -ForegroundColor Yellow
}
else {
    Invoke-RestMethod -Uri "http://localhost:7071/api/ConvertMpegToTranscript?language=en-US&timestamps=true" -Method Post -InFile $audioFile -ContentType "multipart/form-data" -OutFile $timestampFile
    if (Test-Path $timestampFile) {
        Write-Host "Audio to Transcript with timestamps successful:" -ForegroundColor Green
        Write-Host "  JSON output saved to: $timestampFile" -ForegroundColor Cyan
    }
    else {
        Write-Host "Audio to Transcript with timestamps failed" -ForegroundColor Red
    }
}

Write-Host "`nDone. Remember to configure your Azure Speech Service credentials in local.settings.json!" -ForegroundColor Yellow

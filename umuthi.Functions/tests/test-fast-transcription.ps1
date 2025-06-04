# Test Script for Fast Transcription Function
# This script tests the new Fast Transcription functionality

# API Key for authentication
$apiKey = "umuthi-dev-api-key"
$headers = @{"x-api-key" = $apiKey}

# Test endpoint
$endpoint = "http://localhost:7071/api/FastTranscribeAudio"

Write-Host "Testing Fast Transcription API..." -ForegroundColor Cyan

# First, let's check if we have any sample audio files
$sampleFiles = @(
    "sample.wav",
    "sample.mp3",
    "sample_audio.wav"
)

$testFile = $null
foreach ($file in $sampleFiles) {
    if (Test-Path $file) {
        $testFile = $file
        break
    }
}

if (-not $testFile) {
    Write-Host "No sample audio file found. Please create a sample audio file first." -ForegroundColor Yellow
    Write-Host "Expected files: $($sampleFiles -join ', ')" -ForegroundColor Yellow
    exit 1
}

Write-Host "Using test file: $testFile" -ForegroundColor Green

try {
    # Test with English language (default)
    Write-Host "`nTesting Fast Transcription with English..." -ForegroundColor Cyan
    
    $form = @{
        file = Get-Item $testFile
    }
    
    $response = Invoke-RestMethod -Uri "$endpoint?language=en-US" -Method Post -Form $form -Headers $headers
    
    Write-Host "Response received:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 3 | Write-Host
    
    # Test with different language
    Write-Host "`nTesting Fast Transcription with Spanish..." -ForegroundColor Cyan
    
    $response2 = Invoke-RestMethod -Uri "$endpoint?language=es-ES" -Method Post -Form $form -Headers $headers
    
    Write-Host "Response received:" -ForegroundColor Green
    $response2 | ConvertTo-Json -Depth 3 | Write-Host
    
    Write-Host "`nFast Transcription tests completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Error during Fast Transcription test:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $errorContent = $reader.ReadToEnd()
        Write-Host "Error details: $errorContent" -ForegroundColor Red
    }
}
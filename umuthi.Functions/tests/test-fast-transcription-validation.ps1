# Test Script for Fast Transcription Function Parameter Validation
# This script validates the parameter handling without making real API calls

Write-Host "Testing Fast Transcription Parameter Validation..." -ForegroundColor Cyan

# Test 1: Missing API key
Write-Host "`nTest 1: Missing API key should return 401..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:7071/api/FastTranscribeAudio" -Method Post -ErrorAction Stop
    Write-Host "✗ Expected 401 but got: $($response.StatusCode)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ Correctly returned 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "✗ Expected 401 but got: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 2: Missing file should return 400
Write-Host "`nTest 2: Missing file should return 400..." -ForegroundColor Yellow
try {
    $headers = @{"x-api-key" = "umuthi-dev-api-key"}
    $response = Invoke-WebRequest -Uri "http://localhost:7071/api/FastTranscribeAudio" -Method Post -Headers $headers -ErrorAction Stop
    Write-Host "✗ Expected 400 but got: $($response.StatusCode)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 400) {
        Write-Host "✓ Correctly returned 400 Bad Request for missing file" -ForegroundColor Green
    } else {
        Write-Host "✗ Expected 400 but got: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 3: Multiple files should return 400
Write-Host "`nTest 3: Multiple files should return 400..." -ForegroundColor Yellow
if (Test-Path "../samples/sample.wav") {
    try {
        $headers = @{"x-api-key" = "umuthi-dev-api-key"}
        $form = @{
            file1 = Get-Item "../samples/sample.wav"
            file2 = Get-Item "../samples/sample.wav"
        }
        $response = Invoke-RestMethod -Uri "http://localhost:7071/api/FastTranscribeAudio" -Method Post -Form $form -Headers $headers
        Write-Host "✗ Expected 400 but request succeeded" -ForegroundColor Red
    } catch {
        if ($_.Exception.Response.StatusCode -eq 400) {
            Write-Host "✓ Correctly returned 400 Bad Request for multiple files" -ForegroundColor Green
        } else {
            Write-Host "✗ Expected 400 but got: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "⚠ Skipping multiple files test - sample.wav not found" -ForegroundColor Yellow
}

# Test 4: Test ExtractPlainTextFromTranscript method (using reflection would be complex in PowerShell)
Write-Host "`nTest 4: Code structure validation..." -ForegroundColor Yellow

# Check if the function expects exactly one file
$funcCode = Get-Content "../src/Functions/SpeechTranscriptionFunctions.cs" -Raw
if ($funcCode -match "req\.Form\.Files\.Count > 1") {
    Write-Host "✓ Function properly validates single file requirement" -ForegroundColor Green
} else {
    Write-Host "✗ Function doesn't validate single file requirement" -ForegroundColor Red
}

# Check if the function has proper error handling
if ($funcCode -match "catch.*Exception.*ex") {
    Write-Host "✓ Function has proper exception handling" -ForegroundColor Green
} else {
    Write-Host "✗ Function lacks proper exception handling" -ForegroundColor Red
}

# Check if usage tracking is implemented
if ($funcCode -match "TrackUsageAsync") {
    Write-Host "✓ Function includes usage tracking" -ForegroundColor Green
} else {
    Write-Host "✗ Function lacks usage tracking" -ForegroundColor Red
}

Write-Host "`nFast Transcription validation tests completed!" -ForegroundColor Cyan
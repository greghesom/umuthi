# Test script for Audio Conversion API with authentication

# Configuration
$primaryApiKey = "umuthi-dev-api-key"
$makeApiKey = "make-integration-key"
$testApiKey1 = "test-key-1"
$testApiKey2 = "test-key-2"
$invalidApiKey = "invalid-key-should-fail"
$baseUrl = "http://localhost:7071/api"

Write-Host "Testing Audio Conversion API with authentication..." -ForegroundColor Cyan

# Test 1: Get Supported Formats with primary API key
Write-Host "`nTest 1: Get Supported Formats with primary API key" -ForegroundColor Green
$headers = @{
    "x-api-key" = $primaryApiKey
}
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/GetSupportedFormats" -Headers $headers -Method Get
    Write-Host "Success! API returned supported formats:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Test 2: Get Supported Formats with make.com API key
Write-Host "`nTest 2: Get Supported Formats with make.com API key" -ForegroundColor Green
$headers = @{
    "x-api-key" = $makeApiKey
}
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/GetSupportedFormats" -Headers $headers -Method Get
    Write-Host "Success! API accepted make.com API key" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Test 3: Get Supported Formats with test API key 1
Write-Host "`nTest 3: Get Supported Formats with test API key 1" -ForegroundColor Green
$headers = @{
    "x-api-key" = $testApiKey1
}
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/GetSupportedFormats" -Headers $headers -Method Get
    Write-Host "Success! API accepted test API key 1" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Test 4: Get Supported Formats with test API key 2
Write-Host "`nTest 4: Get Supported Formats with test API key 2" -ForegroundColor Green
$headers = @{
    "x-api-key" = $testApiKey2
}
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/GetSupportedFormats" -Headers $headers -Method Get
    Write-Host "Success! API accepted test API key 2" -ForegroundColor Green
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}

# Test 5: Try to access the API with invalid API key (should fail)
Write-Host "`nTest 5: Try to access the API with invalid API key (should fail)" -ForegroundColor Green
$headers = @{
    "x-api-key" = $invalidApiKey
}
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/GetSupportedFormats" -Headers $headers -Method Get -ErrorAction Stop
    Write-Host "This should have failed with 401 Unauthorized! Got response:" -ForegroundColor Red
    $response | ConvertTo-Json
} catch {
    Write-Host "Expected error received: $_" -ForegroundColor Green
}

# Test 6: Try to access the API without any API key (should fail)
Write-Host "`nTest 6: Try to access the API without any API key (should fail)" -ForegroundColor Green
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/GetSupportedFormats" -Method Get -ErrorAction Stop
    Write-Host "This should have failed with 401 Unauthorized! Got response:" -ForegroundColor Red
    $response | ConvertTo-Json
} catch {
    Write-Host "Expected error received: $_" -ForegroundColor Green
}

# Test 7: Convert WAV file to MP3 with authentication
Write-Host "`nTest 7: Convert WAV file to MP3 with authentication" -ForegroundColor Green
$wavFilePath = "path\to\test.wav"  # Update this with a real file path
if (Test-Path $wavFilePath) {
    $headers = @{
        "x-api-key" = $primaryApiKey
    }
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/ConvertWavToMp3" -Headers $headers -Method Post -InFile $wavFilePath -ContentType "multipart/form-data" -OutFile "converted-output.mp3"
        Write-Host "Success! File converted and saved to converted-output.mp3" -ForegroundColor Green
    } catch {
        Write-Host "Error: $_" -ForegroundColor Red
    }
} else {
    Write-Host "Test file not found at $wavFilePath. Skipping this test." -ForegroundColor Yellow
}

Write-Host "`nTests completed." -ForegroundColor Cyan

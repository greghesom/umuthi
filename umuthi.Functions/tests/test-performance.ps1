# Performance Testing Script for Audio Processing API
# This script measures the performance of the audio conversion and transcription functions

# Enable measurement of execution time
$ErrorActionPreference = "Stop"
$VerbosePreference = "Continue"

function Measure-FunctionPerformance {
    param (
        [string]$TestName,
        [string]$InputFile,
        [string]$Endpoint,
        [string]$OutputFile,
        [hashtable]$AdditionalParams = @{},
        [int]$Iterations = 1
    )
    
    Write-Host "`n===== Testing $TestName =====" -ForegroundColor Cyan
    
    if (-not (Test-Path $InputFile)) {
        Write-Host "Input file not found: $InputFile" -ForegroundColor Red
        return
    }
    
    $totalDuration = 0
    $fileSize = (Get-Item $InputFile).Length / 1MB
    Write-Host "Input file size: $($fileSize.ToString("0.00")) MB" -ForegroundColor Yellow
    
    for ($i = 1; $i -le $Iterations; $i++) {
        Write-Host "Run $i of $Iterations..." -ForegroundColor Gray
        
        $params = @{
            Uri = "http://localhost:7071/api/$Endpoint"
            Method = "POST"
            InFile = $InputFile
            ContentType = "multipart/form-data"
            OutFile = "$OutputFile.$i"
            Headers = @{"x-api-key" = "umuthi-dev-api-key"}
        }
        
        # Add any additional query parameters
        if ($AdditionalParams.Count -gt 0) {
            $queryString = "?" + (($AdditionalParams.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join "&")
            $params.Uri = $params.Uri + $queryString
        }
        
        # Measure execution time
        $sw = [System.Diagnostics.Stopwatch]::StartNew()
        
        try {
            Invoke-RestMethod @params
            $sw.Stop()
            $duration = $sw.Elapsed.TotalSeconds
            $totalDuration += $duration
            
            $throughput = $fileSize / $duration
            Write-Host "  Run completed in $($duration.ToString("0.00")) seconds ($($throughput.ToString("0.00")) MB/s)" -ForegroundColor Green
        }
        catch {
            $sw.Stop()
            Write-Host "  Error: $_" -ForegroundColor Red
            Write-Host "  Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
    
    if ($Iterations -gt 1) {
        $avgDuration = $totalDuration / $Iterations
        $avgThroughput = $fileSize / $avgDuration
        Write-Host "Average performance over $Iterations runs:" -ForegroundColor Cyan
        Write-Host "  Duration: $($avgDuration.ToString("0.00")) seconds" -ForegroundColor Green
        Write-Host "  Throughput: $($avgThroughput.ToString("0.00")) MB/s" -ForegroundColor Green
    }
    
    # Display output file size if created
    if (Test-Path "$OutputFile.1") {
        $outputSize = (Get-Item "$OutputFile.1").Length / 1MB
        Write-Host "Output file size: $($outputSize.ToString("0.00")) MB" -ForegroundColor Yellow
        Write-Host "Compression ratio: $((($fileSize - $outputSize) / $fileSize * 100).ToString("0.00"))%" -ForegroundColor Yellow
    }
    
    return @{
        TestName = $TestName
        InputSize = $fileSize
        AverageDuration = $avgDuration
        AverageThroughput = $avgThroughput
    }
}

# Create results directory
$resultsDir = Join-Path $PSScriptRoot "perf-results"
if (-not (Test-Path $resultsDir)) {
    New-Item -Path $resultsDir -ItemType Directory | Out-Null
}

# Performance test for WAV to MP3 conversion
$wavToMp3Results = Measure-FunctionPerformance -TestName "WAV to MP3 Conversion" `
    -InputFile "sample.wav" `
    -Endpoint "ConvertWavToMp3" `
    -OutputFile (Join-Path $resultsDir "perf-wav-to-mp3.mp3") `
    -Iterations 3

# Performance test for MPEG to MP3 conversion
$mpegToMp3Results = Measure-FunctionPerformance -TestName "MPEG to MP3 Conversion" `
    -InputFile "sample.mp4" `
    -Endpoint "ConvertMpegToMp3" `
    -OutputFile (Join-Path $resultsDir "perf-mpeg-to-mp3.mp3") `
    -Iterations 3

# Performance test for audio to transcript conversion
$audioToTextResults = Measure-FunctionPerformance -TestName "Audio to Transcript Conversion" `
    -InputFile "sample.mp3" `
    -Endpoint "ConvertMpegToTranscript" `
    -OutputFile (Join-Path $resultsDir "perf-transcript.txt") `
    -AdditionalParams @{language = "en-US"} `
    -Iterations 2

# Performance test for audio to transcript with timestamps
$audioToTextTimestampResults = Measure-FunctionPerformance -TestName "Audio to Transcript with Timestamps" `
    -InputFile "sample.mp3" `
    -Endpoint "ConvertMpegToTranscript" `
    -OutputFile (Join-Path $resultsDir "perf-transcript-timestamps.json") `
    -AdditionalParams @{language = "en-US"; timestamps = "true"} `
    -Iterations 1

# Summary
Write-Host "`n===== Performance Summary =====" -ForegroundColor Cyan
$results = @($wavToMp3Results, $mpegToMp3Results, $audioToTextResults, $audioToTextTimestampResults) | 
    Where-Object { $_ -ne $null }

$results | ForEach-Object {
    Write-Host "$($_.TestName):" -ForegroundColor Yellow
    Write-Host "  Average Duration: $($_.AverageDuration.ToString("0.00")) seconds" -ForegroundColor Green
    Write-Host "  Average Throughput: $($_.AverageThroughput.ToString("0.00")) MB/s" -ForegroundColor Green
}

# Export results to CSV
$results | Export-Csv -Path (Join-Path $resultsDir "performance-results.csv") -NoTypeInformation
Write-Host "`nResults saved to: $((Join-Path $resultsDir "performance-results.csv"))" -ForegroundColor Cyan

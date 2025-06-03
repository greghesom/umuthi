# Improved Sample Audio Generator Script
# This script creates sample audio files locally for testing

function Create-SampleWavFile {
    param (
        [string]$outputPath,
        [int]$durationSeconds = 5,
        [int]$sampleRate = 44100
    )
    
    Write-Host "Creating sample WAV file at $outputPath..." -ForegroundColor Cyan
    
    # Create a simple sine wave WAV file using .NET
    Add-Type -TypeDefinition @"
    using System;
    using System.IO;
    
    public class WavGenerator {
        public static void CreateSineWave(string filePath, int durationSeconds, int sampleRate) {
            using (FileStream fs = new FileStream(filePath, FileMode.Create)) {
                using (BinaryWriter bw = new BinaryWriter(fs)) {
                    // WAV header
                    bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                    bw.Write((int)(36 + durationSeconds * sampleRate * 2)); // File size - 8
                    bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
                    bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                    bw.Write((int)16); // Subchunk1Size
                    bw.Write((short)1); // AudioFormat (PCM)
                    bw.Write((short)1); // NumChannels (Mono)
                    bw.Write((int)sampleRate); // SampleRate
                    bw.Write((int)(sampleRate * 2)); // ByteRate
                    bw.Write((short)2); // BlockAlign
                    bw.Write((short)16); // BitsPerSample
                    bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                    bw.Write((int)(durationSeconds * sampleRate * 2)); // Subchunk2Size
                    
                    // Data (sine wave)
                    double frequency = 440.0; // A4 note
                    for (int i = 0; i < durationSeconds * sampleRate; i++) {
                        double t = (double)i / sampleRate;
                        double amplitude = Math.Sin(2 * Math.PI * frequency * t);
                        short sample = (short)(amplitude * 32760);
                        bw.Write(sample);
                    }
                }
            }
        }
    }
"@
    
    [WavGenerator]::CreateSineWave($outputPath, $durationSeconds, $sampleRate)
    
    if (Test-Path $outputPath) {
        Write-Host "Created sample WAV file: $outputPath ($(([System.IO.FileInfo]$outputPath).Length / 1KB) KB)" -ForegroundColor Green
        return $true
    } else {
        Write-Host "Failed to create WAV file" -ForegroundColor Red
        return $false
    }
}

# Create a simple MP3 file from text (for testing only)
function Create-SampleTextFile {
    param (
        [string]$outputPath,
        [string]$text = "This is a sample text file for testing audio conversion functions."
    )
    
    Write-Host "Creating sample text file at $outputPath..." -ForegroundColor Cyan
    
    try {
        $text | Out-File -FilePath $outputPath -Encoding utf8
        Write-Host "Created sample text file: $outputPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "Failed to create text file: $_" -ForegroundColor Red
        return $false
    }
}

# Define output paths
$wavPath = Join-Path $PSScriptRoot "sample.wav"
$mp3Path = Join-Path $PSScriptRoot "sample.mp3"
$mp4Path = Join-Path $PSScriptRoot "sample.mp4"
$textPath = Join-Path $PSScriptRoot "sample.txt"

# Create sample files
$wavCreated = Create-SampleWavFile -outputPath $wavPath -durationSeconds 5
Create-SampleTextFile -outputPath $textPath

# For MP3 and MP4, since we can't easily create them programmatically without external tools,
# we'll create placeholders with instructions
if (-not (Test-Path $mp3Path)) {
    "This is a placeholder file. For real testing, replace with a valid MP3 file." | 
        Out-File -FilePath $mp3Path -Encoding utf8
    Write-Host "Created MP3 placeholder: $mp3Path" -ForegroundColor Yellow
    Write-Host "Warning: This is not a real MP3 file. Replace it with a valid MP3 for actual testing." -ForegroundColor Yellow
}

if (-not (Test-Path $mp4Path)) {
    "This is a placeholder file. For real testing, replace with a valid MP4 file." | 
        Out-File -FilePath $mp4Path -Encoding utf8
    Write-Host "Created MP4 placeholder: $mp4Path" -ForegroundColor Yellow
    Write-Host "Warning: This is not a real MP4 file. Replace it with a valid MP4 for actual testing." -ForegroundColor Yellow
}

Write-Host "`nTest files created. Note that only the WAV file is a valid audio file." -ForegroundColor Cyan
Write-Host "For complete testing with real audio, replace the placeholder MP3 and MP4 files with real ones." -ForegroundColor Cyan

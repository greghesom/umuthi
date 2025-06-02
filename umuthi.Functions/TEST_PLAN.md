# Audio Processing Functions Test Plan

This document outlines the testing strategy for the Umuthi Azure Functions audio processing capabilities.

## Test Environment Setup

Before running the tests, ensure the following prerequisites are met:

1. **Local Azure Functions Runtime**: Make sure the Azure Functions Core Tools are installed
2. **Azure Storage Emulator**: Azurite or the Azure Storage Emulator should be running
3. **Speech Service Configuration**: Configure your `local.settings.json` with valid Speech Service credentials
4. **Sample Audio Files**: Use the `generate-test-audio.ps1` script to create test files

## Test Scenarios

### 1. Authentication Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid API Key in Header | Use a valid API key in the `x-api-key` header | 200 OK response |
| Valid API Key in Query | Use a valid API key in the `code` query parameter | 200 OK response |
| Invalid API Key | Use an incorrect API key | 401 Unauthorized response |
| Missing API Key | No API key provided | 401 Unauthorized response |

### 2. WAV to MP3 Conversion Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid WAV File | Convert a standard WAV file to MP3 | 200 OK with valid MP3 file |
| Invalid File Format | Upload a non-WAV file | 400 Bad Request |
| Oversized File | Upload a WAV file larger than 50MB | 400 Bad Request |
| Empty File | Upload a 0-byte WAV file | 400 Bad Request or appropriate error |
| Corrupted WAV File | Upload a corrupted WAV file | Appropriate error response |

### 3. MPEG to MP3 Conversion Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Valid MP4 File | Convert an MP4 file to MP3 | 200 OK with valid MP3 file |
| Valid MPG File | Convert an MPG file to MP3 | 200 OK with valid MP3 file |
| Valid M4A File | Convert an M4A file to MP3 | 200 OK with valid MP3 file |
| Valid AAC File | Convert an AAC file to MP3 | 200 OK with valid MP3 file |
| Invalid File Format | Upload a non-MPEG file | 400 Bad Request |
| Oversized File | Upload a MPEG file larger than 50MB | 400 Bad Request |
| Empty File | Upload a 0-byte MPEG file | 400 Bad Request or appropriate error |
| Corrupted MPEG File | Upload a corrupted MPEG file | Appropriate error response |

### 4. Audio to Transcript Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Single WAV File | Convert a WAV file to text | 200 OK with transcript text |
| Single MP3 File | Convert an MP3 file to text | 200 OK with transcript text |
| Multiple Audio Files | Convert multiple audio files to a single transcript | 200 OK with combined transcript |
| Different Languages | Test with different language parameters | 200 OK with correct language transcript |
| With Timestamps | Test with timestamps=true parameter | 200 OK with JSON including timestamps |
| Oversized Files | Upload files totaling more than 100MB | 400 Bad Request |
| Silent Audio | Upload audio with no speech | Empty or minimal transcript |
| Low Quality Audio | Upload audio with background noise or low quality | Best-effort transcript |

### 5. API Documentation Tests

| Test Case | Description | Expected Result |
|-----------|-------------|-----------------|
| Get Supported Formats | Call the GetSupportedFormats endpoint | 200 OK with complete API documentation |
| Authentication for Docs | Test API key requirement for docs | Confirm it requires authentication |

## Test Execution Plan

1. **Unit Tests**: Run the test scripts for each function individually
2. **Integration Tests**: Test the full pipeline of converting and transcribing audio
3. **Load Tests**: Test the functions with multiple concurrent requests
4. **Performance Tests**: Measure conversion and transcription times for various file sizes

## Test Scripts

The repository includes the following test scripts:

- `test-api-auth.ps1`: Tests API key authentication
- `test-audio-conversion.ps1`: Tests the audio conversion and transcription functions
- `generate-test-audio.ps1`: Generates sample audio files for testing

## Monitoring and Logging

During test execution, monitor:

1. Function logs for errors and warnings
2. Memory usage during processing of large files
3. CPU usage during transcription
4. Azure Speech Service quota usage

## Post-Deployment Tests

After deployment to Azure, perform the following additional tests:

1. Verify CORS settings if the API will be called from browsers
2. Test with the actual production API keys
3. Verify rate limiting is working correctly
4. Test integration with make.com workflows

## Reporting Issues

For any issues found during testing, document:

1. Steps to reproduce
2. Expected behavior
3. Actual behavior
4. Log output
5. Environment details (OS, Azure Functions version, etc.)

## Approvals

Once all tests pass, the functions are ready for production deployment.

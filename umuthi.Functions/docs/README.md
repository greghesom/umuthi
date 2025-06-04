# Audio Conversion Azure Function

This Azure Functions project includes audio conversion and transcription capabilities using Azure AI services.

## Authentication

This API is secured with API key authentication. You need to provide a valid API key to access the endpoints.

For detailed authentication information, see [API_AUTHENTICATION.md](API_AUTHENTICATION.md).

Quick authentication methods:
- **HTTP Header (Recommended)**: `x-api-key: your-api-key`
- **Query Parameter**: `?code=your-api-key`

## Functions

### ConvertWavToMp3
- **Endpoint**: `POST /api/ConvertWavToMp3`
- **Description**: Converts uploaded WAV files to MP3 format
- **Content-Type**: `multipart/form-data`
- **Max File Size**: 50MB
- **Supported Input**: WAV files
- **Output**: MP3 file download

### ConvertMpegToMp3
- **Endpoint**: `POST /api/ConvertMpegToMp3`
- **Description**: Converts uploaded MPEG files to MP3 format
- **Content-Type**: `multipart/form-data`
- **Max File Size**: 50MB
- **Supported Input**: MPEG, MPG, MP4, M4A, AAC files
- **Output**: MP3 file download

### ConvertMpegToTranscript
- **Endpoint**: `POST /api/ConvertMpegToTranscript`
- **Description**: Converts uploaded audio files to text using Azure AI Speech-to-Text
- **Content-Type**: `multipart/form-data`
- **Max File Size**: 100MB total (combined files)
- **Supported Input**: MPEG, MPG, MP4, M4A, AAC, MP3, WAV files
- **Query Parameters**:
  - `language`: Speech recognition language (e.g., en-US, fr-FR) - defaults to en-US
  - `timestamps`: When set to "true", includes word timestamps in output - defaults to false
- **Output**: Plain text or JSON with timestamps (based on timestamps parameter)

### FastTranscribeAudio
- **Endpoint**: `POST /api/FastTranscribeAudio`
- **Description**: Fast transcription using Azure AI Fast Transcription API (single file only)
- **Content-Type**: `multipart/form-data`
- **Max File Size**: 1GB
- **Supported Input**: MPEG, MPG, MP4, M4A, AAC, MP3, WAV, FLAC, OGG files
- **Query Parameters**:
  - `language`: Speech recognition language (e.g., en-US, fr-FR) - defaults to en-US
- **Output**: JSON with detailed transcription results including timestamps and confidence scores

### GetSupportedFormats
- **Endpoint**: `GET /api/GetSupportedFormats`
- **Description**: Returns information about supported formats and usage

## Usage Examples

### Using curl

```bash
# WAV to MP3
curl -X POST -F "file=@your-audio-file.wav" http://localhost:7071/api/ConvertWavToMp3 -H "x-api-key: umuthi-dev-api-key" --output converted-audio.mp3

# MPEG to MP3
curl -X POST -F "file=@your-video-file.mp4" http://localhost:7071/api/ConvertMpegToMp3 -H "x-api-key: umuthi-dev-api-key" --output converted-audio.mp3

# Audio to Transcript
curl -X POST -F "file=@your-audio-file.mp3" http://localhost:7071/api/ConvertMpegToTranscript?language=en-US -H "x-api-key: umuthi-dev-api-key" --output transcript.txt

# Audio to Transcript with timestamps
curl -X POST -F "file=@your-audio-file.mp3" http://localhost:7071/api/ConvertMpegToTranscript?language=en-US&timestamps=true -H "x-api-key: umuthi-dev-api-key" --output transcript.json

# Fast Transcription
curl -X POST -F "file=@your-audio-file.mp3" http://localhost:7071/api/FastTranscribeAudio?language=en-US -H "x-api-key: umuthi-dev-api-key" --output fast-transcript.json
```

### Using PowerShell

```powershell
# WAV to MP3
Invoke-RestMethod -Uri "http://localhost:7071/api/ConvertWavToMp3" -Method Post -InFile "path\to\your\audio.wav" -ContentType "multipart/form-data" -OutFile "converted.mp3"

# MPEG to MP3
Invoke-RestMethod -Uri "http://localhost:7071/api/ConvertMpegToMp3" -Method Post -InFile "path\to\your\video.mp4" -ContentType "multipart/form-data" -OutFile "converted.mp3"

# Audio to Transcript
Invoke-RestMethod -Uri "http://localhost:7071/api/ConvertMpegToTranscript?language=en-US" -Method Post -InFile "path\to\your\audio.mp3" -ContentType "multipart/form-data" -OutFile "transcript.txt"

# Multiple files to Transcript
$form = @{
    file1 = Get-Item -Path "audio1.mp3"
    file2 = Get-Item -Path "audio2.mp3"
}
Invoke-RestMethod -Uri "http://localhost:7071/api/ConvertMpegToTranscript?language=en-US" -Method Post -Form $form -OutFile "transcript.txt"

# Fast Transcription
$form = @{ file = Get-Item -Path "audio.mp3" }
$headers = @{"x-api-key" = "umuthi-dev-api-key"}
Invoke-RestMethod -Uri "http://localhost:7071/api/FastTranscribeAudio?language=en-US" -Method Post -Form $form -Headers $headers -OutFile "fast-transcript.json"
```

## Configuration

### Azure Speech Services Configuration

To use the transcription functionality, you need to set up Azure Speech Services and configure your credentials:

1. Create an Azure Speech Services resource in the Azure Portal
2. Add the following settings to your `local.settings.json` file:

```json
{
  "Values": {
    "SpeechServiceKey": "your-speech-service-key",
    "SpeechServiceRegion": "your-region"
  }
}
```

## Dependencies

This project uses the following NuGet packages for audio conversion and transcription:
- **NAudio** (v2.2.1): Core audio processing library
- **NAudio.Lame** (v2.1.0): MP3 encoding using LAME encoder
- **Microsoft.CognitiveServices.Speech** (v1.36.0): Azure AI Speech Services SDK
- **Azure.Identity** (v1.12.0): Azure authentication libraries

## Development

### Build the project
```bash
dotnet build
```

### Run locally
```bash
func start
```

## Notes

- The functions use the LAME MP3 encoder for audio conversion
- Audio merging is supported for multiple files in the transcription endpoint
- Audio is automatically resampled to the optimal format for speech recognition (16kHz, mono)
- All operations are performed asynchronously for better performance
- Temporary files are automatically cleaned up after processing
- Error handling includes file size validation and format checking

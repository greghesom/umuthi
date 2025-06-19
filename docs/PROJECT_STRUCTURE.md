# Umuthi Audio Processing Functions - Project Structure

This document describes the reorganized project structure following Azure Functions best practices.

## Folder Structure

```
/
├── src/                          # Source code
│   ├── Functions/               # Function endpoints
│   │   ├── ApiInfoFunctions.cs         # API information and health checks
│   │   ├── AudioConversionFunctions.cs # WAV/MPEG to MP3 conversion
│   │   └── SpeechTranscriptionFunctions.cs # Audio to text transcription
│   ├── Services/               # Business logic services
│   │   ├── IAudioConversionService.cs      # Audio conversion interface
│   │   ├── AudioConversionService.cs       # Audio conversion implementation
│   │   ├── ISpeechTranscriptionService.cs  # Speech transcription interface
│   │   └── SpeechTranscriptionService.cs   # Speech transcription implementation
│   ├── Models/                 # Data models
│   │   └── AudioConversionResult.cs        # Audio conversion result model
│   ├── Middleware/             # Custom middleware/attributes
│   │   ├── ApiKeyAuthenticationAttribute.cs # API key authentication attribute
│   │   └── ApiKeyValidator.cs              # API key validation logic
│   └── Helpers/                # Utility classes
│       └── BufferedAudioHelpers.cs        # Memory-optimized audio processing
├── docs/                       # Documentation
│   ├── README.md                           # This file
│   ├── API_AUTHENTICATION.md               # Authentication guide
│   ├── AZURE_DEPLOYMENT_GUIDE.md          # Azure deployment instructions
│   ├── MAKE_INTEGRATION_GUIDE.md          # Make.com integration guide
│   ├── MONITORING_GUIDE.md                # Monitoring and maintenance
│   ├── PROJECT_STATUS.md                  # Current project status
│   ├── SPEECH_SETUP.md                    # Speech service configuration
│   └── TEST_PLAN.md                       # Testing documentation
├── tests/                      # Test files and scripts
│   ├── test-audio-conversion.ps1           # Audio conversion tests
│   ├── test-api-auth.ps1                  # Authentication tests
│   ├── test-performance.ps1               # Performance tests
│   ├── create-sample-audio.ps1            # Sample file generator
│   └── generate-test-audio.ps1            # Additional test file generator
├── samples/                    # Sample files for testing
│   ├── sample.wav                          # Sample WAV file
│   ├── sample.mp3                          # Sample MP3 file
│   ├── sample.mp4                          # Sample MP4 file
│   └── sample.txt                          # Sample text file
└── deployment/                 # Deployment configurations
    └── (deployment files)
```

## Architecture Overview

### Functions Layer (`src/Functions/`)
- **ApiInfoFunctions**: Provides API information and health checks
- **AudioConversionFunctions**: Handles WAV and MPEG to MP3 conversion
- **SpeechTranscriptionFunctions**: Handles audio to text transcription

### Services Layer (`src/Services/`)
- **IAudioConversionService**: Interface for audio conversion operations
- **AudioConversionService**: Implementation of audio conversion logic
- **ISpeechTranscriptionService**: Interface for speech transcription operations
- **SpeechTranscriptionService**: Implementation using Azure Cognitive Services

### Models Layer (`src/Models/`)
- **AudioConversionResult**: Data model for audio conversion results

### Middleware Layer (`src/Middleware/`)
- **ApiKeyAuthenticationAttribute**: Marker attribute for protected endpoints
- **ApiKeyValidator**: Core API key validation logic

### Helpers Layer (`src/Helpers/`)
- **BufferedAudioHelpers**: Memory-optimized audio processing utilities

## Key Benefits of This Structure

1. **Separation of Concerns**: Each component has a single responsibility
2. **Dependency Injection**: Services are properly registered and injected
3. **Testability**: Business logic is separated from HTTP concerns
4. **Maintainability**: Smaller, focused files are easier to maintain
5. **Scalability**: Easy to add new functions or services
6. **Documentation**: Clear organization makes the project self-documenting

## Available Endpoints

- `GET /api/GetSupportedFormats` - API information and supported formats
- `GET /api/HealthCheck` - Health status check
- `POST /api/ConvertWavToMp3` - Convert WAV files to MP3
- `POST /api/ConvertMpegToMp3` - Convert MPEG files to MP3
- `POST /api/ConvertMpegToTranscript` - Convert audio files to text transcripts

All POST endpoints require API key authentication via `x-api-key` header or `code` query parameter.

## Development Workflow

1. **Build**: `dotnet build`
2. **Run locally**: `func start` (from bin/Debug/net8.0 folder)
3. **Test**: Use scripts in the `tests/` folder
4. **Deploy**: Follow instructions in `docs/AZURE_DEPLOYMENT_GUIDE.md`

## Configuration

Key configuration items in `local.settings.json`:
- `ApiKey`: Primary API key for authentication
- `AdditionalApiKeys`: Comma-separated additional API keys
- `SpeechServiceKey`: Azure Speech Service subscription key
- `SpeechServiceRegion`: Azure Speech Service region

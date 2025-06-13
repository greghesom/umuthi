# Umuthi Functions Multi-Project Architecture

This document describes the refactored multi-project architecture for the umuthi.Functions application.

## Project Structure

```
src/
├── umuthi.Functions/           # Main Functions project
│   ├── Functions/
│   │   ├── Audio/              # Audio processing functions
│   │   │   ├── AudioConversionFunctions.cs
│   │   │   └── SpeechTranscriptionFunctions.cs
│   │   ├── Api/                # API information functions
│   │   │   ├── ApiInfoFunctions.cs
│   │   │   └── OpenApiFunctions.cs
│   │   └── Analytics/          # Usage analytics functions
│   │       └── UsageAnalyticsFunctions.cs
│   ├── Middleware/             # Authentication and middleware
│   │   ├── ApiKeyAuthenticationAttribute.cs
│   │   ├── ApiKeyRateLimiter.cs
│   │   └── ApiKeyValidator.cs
│   ├── Extensions/             # DI configuration extensions
│   │   └── ServiceCollectionExtensions.cs
│   ├── Startup.cs              # Application startup configuration
│   ├── Program.cs              # Entry point
│   └── host.json               # Azure Functions host configuration
├── umuthi.Core/               # Business logic & domain services
│   └── Services/
│       ├── AudioConversionService.cs
│       ├── SpeechTranscriptionService.cs
│       └── UsageTrackingService.cs
├── umuthi.Contracts/          # DTOs, interfaces, contracts
│   ├── Interfaces/
│   │   ├── IAudioConversionService.cs
│   │   ├── ISpeechTranscriptionService.cs
│   │   └── IUsageTrackingService.cs
│   └── Models/
│       ├── ApiModels.cs
│       ├── AudioConversionResult.cs
│       ├── BillingModels.cs
│       ├── OperationTypes.cs
│       └── UsageRecord.cs
└── umuthi.Shared/             # Shared utilities, constants
    └── Helpers/
        └── BufferedAudioHelpers.cs
tests/
├── umuthi.Functions.Tests/    # Function integration tests
├── umuthi.Core.Tests/         # Business logic unit tests
└── umuthi.Infrastructure.Tests/ # Infrastructure tests
```

## Architecture Benefits

1. **Separation of Concerns**: Each project has a single, focused responsibility
2. **Dependency Injection**: Clean service registration using extension methods
3. **Testability**: Business logic isolated from HTTP/Azure Functions concerns
4. **Maintainability**: Smaller, focused projects are easier to maintain
5. **Scalability**: Easy to add new functions or services
6. **Domain Organization**: Functions grouped by business domain (Audio, Api, Analytics)

## Project Dependencies

- **umuthi.Functions** → umuthi.Core, umuthi.Contracts, umuthi.Shared
- **umuthi.Core** → umuthi.Contracts, umuthi.Shared
- **umuthi.Contracts** → (no dependencies)
- **umuthi.Shared** → (minimal dependencies)

## Key Architectural Patterns

### Service Registration
Services are registered using extension methods in `ServiceCollectionExtensions.cs`:
- `AddAudioProcessingServices()` - Audio conversion and transcription
- `AddUsageTrackingServices()` - Usage analytics and billing
- `AddUmuthiFunctionServices()` - All function services

### Domain-Based Function Organization
Functions are organized by business domain:
- **Audio**: Audio processing (conversion, transcription)
- **Api**: API documentation and health checks
- **Analytics**: Usage tracking and billing

### Clean Separation of Concerns
- **umuthi.Functions**: HTTP endpoints and Azure Functions hosting
- **umuthi.Core**: Business logic and service implementations  
- **umuthi.Contracts**: Interfaces and data transfer objects
- **umuthi.Shared**: Utilities and helpers used across projects

## Getting Started

### Build the Solution
```bash
dotnet build
```

### Run Tests
```bash
dotnet test
```

### Run Functions Locally
```bash
cd src/umuthi.Functions
func start
```

## Adding New Functions

1. Create function class in appropriate domain folder under `Functions/`
2. Use dependency injection to access business services from `umuthi.Core`
3. Follow existing patterns for error handling and usage tracking
4. Add corresponding tests in `umuthi.Functions.Tests`

## Adding New Services

1. Create interface in `umuthi.Contracts/Interfaces/`
2. Create implementation in `umuthi.Core/Services/`
3. Register service in `ServiceCollectionExtensions.cs`
4. Add unit tests in `umuthi.Core.Tests`
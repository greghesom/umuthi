# Umuthi - Audio Processing & Workflow Management Platform

[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Azure Functions](https://img.shields.io/badge/Azure-Functions-blue.svg)](https://azure.microsoft.com/en-us/products/functions/)
[![.NET Aspire](https://img.shields.io/badge/.NET-Aspire-orange.svg)](https://learn.microsoft.com/en-us/dotnet/aspire/)

Umuthi is a comprehensive cloud-native platform that combines audio processing capabilities with workflow management. The platform provides powerful audio conversion and speech-to-text transcription services through Azure Functions, alongside a robust workflow management system built with .NET Aspire.

## 🚀 Features

### Audio Processing (Azure Functions)
- **Audio Format Conversion**: Convert WAV and MPEG files to MP3 format
- **Speech-to-Text Transcription**: Convert audio files to text using Azure AI Speech Services
- **Fast Transcription**: High-speed transcription using Azure's Fast Transcription API
- **Multi-language Support**: Support for multiple languages including English, Spanish, French, German, and more
- **Batch Processing**: Process multiple audio files simultaneously
- **API Key Authentication**: Secure API access with key-based authentication

### Workflow Management System
- **Workflow Creation & Management**: Create, edit, and manage complex workflows
- **Workflow Execution Tracking**: Monitor workflow executions with detailed status tracking
- **Clean Architecture**: Domain-driven design with proper separation of concerns
- **Entity Framework Core**: Robust data persistence with SQL Server support
- **Real-time Updates**: SignalR integration for real-time workflow status updates

### Modern Web Application
- **Blazor Server**: Interactive web UI built with Blazor Server components
- **Responsive Design**: Modern, mobile-friendly user interface
- **Real-time Communication**: SignalR hubs for live updates
- **Comprehensive Logging**: Serilog integration for structured logging

## 🏗️ Architecture

The project follows a clean, microservices-oriented architecture using .NET Aspire:

```
├── umuthi.AppHost/          # .NET Aspire orchestration host
├── umuthi.ApiService/       # REST API service for workflows
├── umuthi.Web/              # Blazor Server web application
├── umuthi.Functions/        # Azure Functions for audio processing
├── umuthi.Domain/           # Domain entities and business logic
├── umuthi.Application/      # Application services and DTOs
├── umuthi.Infrastructure/   # Data access and external services
├── umuthi.ServiceDefaults/  # Shared service configurations
└── umuthi.Tests/           # Unit and integration tests
```

### Key Components

- **Domain Layer**: Contains core business entities (`Workflow`, `WorkflowExecution`) and enums
- **Application Layer**: Houses business logic, DTOs, and service interfaces
- **Infrastructure Layer**: Implements data access using Entity Framework Core and repositories
- **API Service**: Provides RESTful endpoints for workflow management
- **Azure Functions**: Serverless functions for audio processing tasks
- **Web Frontend**: Blazor Server application with real-time capabilities

## 🛠️ Technology Stack

- **.NET 8.0**: Latest version of .NET for optimal performance
- **.NET Aspire**: Cloud-native application orchestration
- **Azure Functions v4**: Serverless compute for audio processing
- **Blazor Server**: Interactive web UI framework
- **Entity Framework Core**: Object-relational mapping
- **SignalR**: Real-time web functionality
- **Azure AI Speech Services**: AI-powered speech recognition
- **NAudio & LAME**: Audio processing and MP3 encoding
- **Serilog**: Structured logging framework
- **Swagger/OpenAPI**: API documentation

## 🚦 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Azure Functions Core Tools](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (LocalDB for development)
- [Azure Speech Service](https://azure.microsoft.com/en-us/services/cognitive-services/speech-services/) (for transcription features)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/umuthi.git
   cd umuthi
   ```

2. **Configure Azure Speech Services** (for audio transcription)
   
   Create an Azure Speech Service resource and update `umuthi.Functions/local.settings.json`:
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
       "SpeechServiceKey": "YOUR_SPEECH_SERVICE_KEY",
       "SpeechServiceRegion": "YOUR_SPEECH_SERVICE_REGION"
     }
   }
   ```

3. **Build and run the application**
   ```bash
   dotnet restore
   dotnet build
   dotnet run --project umuthi.AppHost
   ```

   This will start all services using .NET Aspire orchestration.

4. **Run Azure Functions locally** (in a separate terminal)
   ```bash
   cd umuthi.Functions
   func start
   ```

## 📖 Usage

### Audio Processing APIs

The Azure Functions provide the following endpoints:

#### Convert WAV to MP3
```bash
curl -X POST \
  -H "x-api-key: umuthi-dev-api-key" \
  -F "file=@audio.wav" \
  http://localhost:7071/api/ConvertWavToMp3 \
  --output converted.mp3
```

#### Convert Audio to Text
```bash
curl -X POST \
  -H "x-api-key: umuthi-dev-api-key" \
  -F "file=@audio.mp3" \
  "http://localhost:7071/api/ConvertAudioToTranscript?language=en-US&timestamps=true" \
  --output transcript.json
```

#### Fast Transcription
```bash
curl -X POST \
  -H "x-api-key: umuthi-dev-api-key" \
  -F "file=@audio.mp3" \
  "http://localhost:7071/api/FastTranscribeAudio?language=en-US" \
  --output fast-transcript.json
```

### Workflow Management

Access the web application at `https://localhost:7213` to:
- Create and manage workflows
- Monitor workflow executions
- View execution history and status
- Configure workflow parameters

## 🧪 Testing

Run the test suite:

```bash
dotnet test
```

The project includes:
- Unit tests for domain entities
- Integration tests for the web application
- API endpoint testing for Azure Functions

## 📁 Project Structure

### Domain Entities

- **Workflow**: Represents a workflow definition with name, description, and configuration
- **WorkflowExecution**: Tracks individual workflow runs with status and results
- **BaseEntity**: Base class providing common properties (ID, timestamps, audit fields)

### API Endpoints

#### Workflow API (`/api/workflows`)
- `GET /api/workflows` - List all workflows
- `GET /api/workflows/{id}` - Get workflow by ID
- `POST /api/workflows` - Create new workflow
- `PUT /api/workflows/{id}` - Update workflow
- `DELETE /api/workflows/{id}` - Delete workflow

#### Audio Functions (`/api/`)
- `POST /api/ConvertWavToMp3` - Convert WAV to MP3
- `POST /api/ConvertMpegToMp3` - Convert MPEG to MP3
- `POST /api/ConvertAudioToTranscript` - Transcribe audio to text
- `POST /api/FastTranscribeAudio` - Fast audio transcription
- `GET /api/GetSupportedFormats` - Get supported audio formats

## 🔧 Configuration

### Application Settings

Key configuration files:
- `appsettings.json` - Main application configuration
- `local.settings.json` - Azure Functions local settings
- `azure.yaml` - Aspire orchestration configuration

### Environment Variables

Required for Azure Functions:
- `SpeechServiceKey` - Azure Speech Service API key
- `SpeechServiceRegion` - Azure Speech Service region

## 📊 Monitoring & Logging

The application includes comprehensive logging and monitoring:
- **Serilog**: Structured logging with file and console output
- **Application Insights**: Azure monitoring integration
- **Health Checks**: Built-in health monitoring
- **Performance Tracking**: Request/response time monitoring

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## 🆘 Support

For support and questions:
- Check the [documentation](umuthi.Functions/docs/)
- Review [API authentication guide](umuthi.Functions/docs/API_AUTHENTICATION.md)
- See [Azure Speech setup guide](umuthi.Functions/docs/SPEECH_SETUP.md)

## 🔗 Related Documentation

- [Azure Functions Documentation](umuthi.Functions/docs/README.md)
- [Project Structure Guide](umuthi.Functions/docs/PROJECT_STRUCTURE.md)
- [Speech Service Setup](umuthi.Functions/docs/SPEECH_SETUP.md)
- [API Authentication](umuthi.Functions/docs/API_AUTHENTICATION.md)

---

**Umuthi** - Empowering audio processing and workflow automation in the cloud.
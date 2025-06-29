# umuthi Azure Functions - Production Deployment

This repository contains the Azure Functions application for umuthi with automated deployment pipeline.

## 🚀 Quick Start

### Prerequisites
- Azure subscription with Function App resource
- GitHub repository secrets configured (see below)
- .NET 8.0 SDK for local development

### Deployment
Deployment is automated via GitHub Actions and triggers automatically on:
- Push to `main` branch
- Manual workflow dispatch from GitHub Actions tab

## 🔧 Configuration

### Required GitHub Secrets
Configure these secrets in your GitHub repository settings:

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | Azure service principal application ID |
| `AZURE_TENANT_ID` | Azure Active Directory tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

### Azure App Settings
Configure these in your Azure Function App:

| Setting | Description | Example |
|---------|-------------|---------|
| `DefaultConnection` | SQL Server connection string | `Server=...;Database=umuthi;...` |
| `APPLICATIONINSIGHTS_CONNECTION_STRING` | Application Insights connection string | `InstrumentationKey=...` |

## 📋 Project Structure

```
src/umuthi.Functions/           # Main Functions project
├── Functions/                  # Function implementations
│   ├── Api/                   # API and health check functions
│   ├── Analytics/             # Usage analytics functions
│   └── Audio/                 # Audio processing functions
├── Extensions/                # Service configuration extensions
├── Middleware/                # Custom middleware
├── Program.cs                 # Application entry point
├── Startup.cs                 # Application configuration
└── host.json                  # Azure Functions host configuration
```

## 🔍 Health Checks

The application includes built-in health monitoring:

- **Endpoint**: `GET /api/HealthCheck`
- **Response**: JSON with health status and service information
- **Used by**: Deployment verification and monitoring

## 📊 Monitoring

### Application Insights
- Request tracking and performance metrics
- Exception logging and diagnostics
- Custom telemetry for business metrics

### Logging
- Structured logging with Microsoft.Extensions.Logging
- Configurable log levels per category
- Integration with Azure Functions logging

## 🔒 Security

- API key authentication for protected endpoints
- Anonymous access only for health checks
- Secure secret management via Azure App Settings
- HTTPS enforcement in production

## 🛠️ Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/greghesom/umuthi.git
   cd umuthi
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the project**
   ```bash
   dotnet build
   ```

4. **Run functions locally**
   ```bash
   cd src/umuthi.Functions
   func start
   ```

## 📖 API Documentation

### Health Check
```http
GET /api/HealthCheck
```

Returns application health status:
```json
{
  "status": "healthy",
  "timestamp": "2024-06-29T19:07:16Z",
  "version": "1.0.0",
  "services": {
    "audioConversion": "operational",
    "speechTranscription": "operational",
    "usageTracking": "operational"
  }
}
```

### Supported Audio Formats
```http
GET /api/GetSupportedFormats
```

Returns list of supported audio formats and API information.

## 🐛 Troubleshooting

### Common Issues

1. **Deployment Failure**: Check GitHub Actions logs and Azure credentials
2. **Function Not Responding**: Verify Application Insights logs and connection strings
3. **Database Errors**: Check connection string and database availability

### Useful Commands

```bash
# Check Azure Function App status
az functionapp show --name umuthi --resource-group <resource-group>

# View Function App logs
az functionapp log tail --name umuthi --resource-group <resource-group>

# Restart Function App
az functionapp restart --name umuthi --resource-group <resource-group>
```

## 📞 Support

For issues or questions:
1. Check GitHub Issues for similar problems
2. Review Application Insights for error details  
3. Contact the development team with relevant logs
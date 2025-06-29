# Azure Functions Deployment Documentation

## Overview
This document describes the deployment process for the umuthi Azure Functions application.

## Prerequisites

### Azure Resources
- Azure Function App named "umuthi"
- Azure SQL Database (for production)
- Application Insights (for monitoring)

### GitHub Secrets
The following secrets must be configured in your GitHub repository settings:

| Secret Name | Description |
|-------------|-------------|
| `AZURE_CLIENT_ID` | Azure service principal client ID |
| `AZURE_TENANT_ID` | Azure tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

### Setting up Azure Service Principal for OIDC Authentication

1. Create a service principal:
```bash
az ad sp create-for-rbac --name "umuthi-github-actions" --role contributor --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group-name} --json-auth
```

2. Configure federated credentials for GitHub Actions:
```bash
az ad app federated-credential create --id {app-id} --parameters '{
  "name": "umuthi-github-actions",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:greghesom/umuthi:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

## Deployment Workflow

The deployment is automated through GitHub Actions and triggers on:
- Push to `main` branch
- Manual workflow dispatch

### Deployment Steps
1. **Checkout code** - Downloads the repository
2. **Setup .NET 8.0** - Installs the required .NET SDK
3. **Build project** - Compiles the Functions app in Release mode
4. **Azure login** - Authenticates using OIDC
5. **Deploy to Azure** - Deploys the built application to Azure Functions

### Build Configuration
- **Target Framework**: .NET 8.0
- **Build Configuration**: Release
- **Output Directory**: `src/umuthi.Functions/output`

## Health Checks

The Functions app includes built-in health check endpoints:

- **Health Check**: `GET /api/HealthCheck`
  - Returns application health status
  - Includes service status information

## Monitoring and Logging

### Application Insights
The Functions app is configured with Application Insights for:
- Request tracking
- Exception logging
- Performance monitoring
- Custom telemetry

### Logging Configuration
- Structured logging using Microsoft.Extensions.Logging
- Application Insights integration
- Sampling configured to exclude Request telemetry

## Database Migration

The Functions app includes automatic database migration on startup:
- Migrations run through the `MigrationService` hosted service
- Ensures database schema is up-to-date on deployment
- Logs migration status and errors

## Security Considerations

### Authentication
- Functions use API key authentication where required
- Anonymous access only for health checks and public APIs

### Environment Variables
- Connection strings and sensitive data stored in Azure App Settings
- No secrets in code or configuration files

## Troubleshooting

### Common Issues
1. **Deployment fails**: Check Azure service principal permissions
2. **Database connection**: Verify connection string in Azure App Settings
3. **Function not responding**: Check Application Insights logs

### Useful Commands
```bash
# Check Function App status
az functionapp show --name umuthi --resource-group {resource-group}

# View Function App logs
az functionapp log tail --name umuthi --resource-group {resource-group}

# Restart Function App
az functionapp restart --name umuthi --resource-group {resource-group}
```

## Rollback Procedures

### Automated Rollback
- Azure Functions supports deployment slots for blue-green deployments
- Previous versions are maintained in deployment history

### Manual Rollback
1. Navigate to Azure Portal → Function App → Deployment Center
2. Select previous successful deployment
3. Click "Redeploy"

## Support

For deployment issues:
1. Check GitHub Actions workflow logs
2. Review Application Insights for runtime errors
3. Verify Azure resource configuration
4. Contact the development team with relevant error messages
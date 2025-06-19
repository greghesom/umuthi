# Azure Deployment Guide for Audio Processing API

This guide provides step-by-step instructions for deploying the Umuthi Audio Processing API to Azure Functions.

## Prerequisites

1. Azure subscription
2. Azure CLI installed locally or Azure Cloud Shell
3. .NET 8.0 SDK installed locally
4. Azure Functions Core Tools installed locally

## Step 1: Create Azure Resources

### Option 1: Using Azure Portal

1. Log in to the [Azure Portal](https://portal.azure.com)
2. Create a new Resource Group
3. Create a new Function App with the following settings:
   - Publish: Code
   - Runtime stack: .NET
   - Version: 8.0
   - Operating System: Windows or Linux (choose based on your preference)
   - Plan type: Consumption (serverless) or Premium (for higher processing capacity)
4. Create an Azure Speech Service in the same region
5. Create an Azure Storage account for function app storage

### Option 2: Using Azure CLI

```powershell
# Login to Azure
az login

# Set variables
$resourceGroup = "umuthi-audio-processing"
$location = "eastus"
$storageAccount = "umuthistorage"
$functionApp = "umuthi-audio-functions"
$speechServiceName = "umuthi-speech-service"

# Create a resource group
az group create --name $resourceGroup --location $location

# Create a storage account
az storage account create --name $storageAccount --location $location --resource-group $resourceGroup --sku Standard_LRS

# Create a function app
az functionapp create --name $functionApp --storage-account $storageAccount --consumption-plan-location $location --resource-group $resourceGroup --runtime dotnet --functions-version 4 --os-type Windows

# Create a speech service
az cognitiveservices account create --name $speechServiceName --resource-group $resourceGroup --kind SpeechServices --sku S0 --location $location --yes
```

## Step 2: Configure Azure Resources

### Configure Application Settings

1. In the Azure Portal, navigate to your Function App
2. Go to "Configuration" under "Settings"
3. Add the following application settings:

| Name | Value | Description |
|------|-------|-------------|
| SpeechServiceKey | [YOUR_SPEECH_SERVICE_KEY] | Key from your Azure Speech Service |
| SpeechServiceRegion | eastus | Region of your Azure Speech Service |
| ApiKey | [YOUR_PRIMARY_API_KEY] | Primary API key for your API |
| AdditionalApiKeys | make-integration-key,test-key-1,test-key-2 | Additional API keys, comma-separated |

### Retrieve Speech Service Key

```powershell
# Get the speech service key
$speechKey = az cognitiveservices account keys list --name $speechServiceName --resource-group $resourceGroup --query "key1" --output tsv

# Set the speech service key in the function app settings
az functionapp config appsettings set --name $functionApp --resource-group $resourceGroup --settings SpeechServiceKey=$speechKey SpeechServiceRegion=$location
```

## Step 3: Deploy the Function App

### Option 1: Using Visual Studio

1. Open the solution in Visual Studio
2. Right-click on the function app project
3. Select "Publish"
4. Select "Azure" as the target
5. Select "Azure Function App" as the specific target
6. Select your subscription and the function app you created
7. Click "Publish"

### Option 2: Using Azure Functions Core Tools

```powershell
# Navigate to the project directory
cd c:\Users\greg\source\umuthi\umuthi.Functions

# Build the project in Release mode
dotnet build --configuration Release

# Publish the project
dotnet publish --configuration Release

# Deploy to Azure Functions
func azure functionapp publish $functionApp
```

### Option 3: Using GitHub Actions

1. Create a GitHub repository for your code
2. Set up GitHub Actions workflow with the following YAML:

```yaml
name: Deploy Function App

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: windows-latest
    steps:
    - name: Checkout code
      uses: actions/checkout@v2

    - name: Setup .NET
      uses: actions/setup-dotnet@v1
      with:
        dotnet-version: '8.0.x'

    - name: Build
      run: dotnet build --configuration Release

    - name: Publish
      run: dotnet publish --configuration Release --output ./publish

    - name: Deploy to Azure Functions
      uses: Azure/functions-action@v1
      with:
        app-name: ${{ secrets.AZURE_FUNCTIONAPP_NAME }}
        package: ./publish
        publish-profile: ${{ secrets.AZURE_FUNCTIONAPP_PUBLISH_PROFILE }}
```

## Step 4: Configure CORS (if needed)

If your API will be accessed from browsers, enable CORS:

```powershell
# Enable CORS for specific origins
az functionapp cors add --name $functionApp --resource-group $resourceGroup --allowed-origins "https://yourdomain.com"

# Or allow all origins (not recommended for production)
az functionapp cors add --name $functionApp --resource-group $resourceGroup --allowed-origins "*"
```

## Step 5: Set Up Custom Domain (Optional)

For a professional API endpoint:

1. In the Azure Portal, go to your Function App
2. Select "Custom domains" under "Settings"
3. Follow the wizard to add and verify your domain

## Step 6: Configure Monitoring

1. In the Azure Portal, go to your Function App
2. Go to "Application Insights" under "Settings"
3. Enable Application Insights if not already enabled
4. Set up alerts for:
   - High CPU usage
   - Memory consumption
   - Error rates
   - Response times

## Step 7: Test the Deployment

Use the provided test scripts to verify your deployment:

```powershell
# Replace with your function app URL
$functionAppUrl = "https://$functionApp.azurewebsites.net"

# Test WAV to MP3 conversion
Invoke-RestMethod -Uri "$functionAppUrl/api/ConvertWavToMp3" -Method Post -InFile "sample.wav" -ContentType "multipart/form-data" -Headers @{"x-api-key" = "[YOUR_API_KEY]"} -OutFile "azure-converted.mp3"

# Test MPEG to transcript
Invoke-RestMethod -Uri "$functionAppUrl/api/ConvertMpegToTranscript?language=en-US" -Method Post -InFile "sample.mp3" -ContentType "multipart/form-data" -Headers @{"x-api-key" = "[YOUR_API_KEY]"} -OutFile "azure-transcript.txt"
```

## Troubleshooting

### Common Deployment Issues

1. **Function app fails to start**:
   - Check application settings
   - Review function app logs in the Azure Portal

2. **Authorization errors**:
   - Verify API keys are correctly configured
   - Check that client is sending the key properly

3. **Speech service errors**:
   - Verify the Speech Service key and region are correct
   - Check quota limits in the Azure Portal

4. **Memory/performance issues**:
   - Consider scaling up your function app plan
   - Optimize code for better memory usage

### Viewing Logs

```powershell
# Stream logs from the function app
az functionapp log tail --name $functionApp --resource-group $resourceGroup
```

## Performance Optimization

For production environments:

1. Consider using a Premium Function App plan for:
   - More memory (up to 14 GB)
   - Longer execution times (up to 60 minutes)
   - Dedicated instances

2. Use App Service Environment (ASE) for:
   - Network isolation
   - Dedicated capacity
   - Higher scale

3. Enable Azure CDN for:
   - Caching frequently accessed files
   - Reducing latency for global users

## Security Considerations

1. Rotate API keys regularly
2. Use Azure Key Vault for storing secrets
3. Enable Azure Private Link for network isolation
4. Set up IP restrictions for admin operations

## Cost Optimization

Monitor and optimize costs by:

1. Choosing the right Function App plan
2. Setting up budget alerts
3. Configuring auto-scaling rules
4. Using reserved instances for predictable workloads

---

For questions or support, contact your Azure administrator.

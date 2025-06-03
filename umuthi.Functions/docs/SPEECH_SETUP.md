# Azure AI Speech-to-Text Configuration Guide

This document explains how to set up Azure AI Speech Services to use the transcript functionality in this project.

## Step 1: Create an Azure Speech Service Resource

1. Log in to the [Azure Portal](https://portal.azure.com)
2. Click on "Create a resource"
3. Search for "Speech" and select "Speech"
4. Click "Create"
5. Fill in the following details:
   - **Subscription**: Select your Azure subscription
   - **Resource Group**: Create new or select existing
   - **Region**: Choose a region close to your users (e.g., East US)
   - **Name**: Give your resource a unique name
   - **Pricing tier**: Select "Free F0" for testing (limited to 5 hours/month) or "Standard S0" for production
6. Click "Review + create", then "Create"

## Step 2: Get Your Speech Service Key

1. After deployment completes, click "Go to resource"
2. From the left menu, select "Keys and Endpoint"
3. Copy either "KEY 1" or "KEY 2"
4. Note the "Location/Region" value

## Step 3: Configure Your Application

Update your `local.settings.json` file with the following values:

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

Replace:
- `YOUR_SPEECH_SERVICE_KEY` with the key you copied in Step 2
- `YOUR_SPEECH_SERVICE_REGION` with the region you noted (e.g., "eastus")

## Step 4: Test Your Configuration

Use the provided test script to verify your setup:

```powershell
./test-audio-conversion.ps1
```

## Supported Languages

The Speech-to-Text API supports many languages. Here are some common ones:

| Language | Code |
|----------|------|
| English (US) | en-US |
| English (UK) | en-GB |
| Spanish | es-ES |
| French | fr-FR |
| German | de-DE |
| Italian | it-IT |
| Japanese | ja-JP |
| Korean | ko-KR |
| Portuguese (Brazil) | pt-BR |
| Chinese (Mandarin) | zh-CN |

For a complete list, see the [Azure Speech Service language support documentation](https://docs.microsoft.com/azure/cognitive-services/speech-service/language-support).

## Troubleshooting

1. **Error 401 Unauthorized**: Check that your Speech Service key and region are correct
2. **Error 429 Too Many Requests**: You've exceeded your quota. Consider upgrading your pricing tier
3. **No Speech Detected**: Check that your audio file contains clear speech and is in a supported format
4. **Poor Transcription Quality**: Try a different language code or improve audio quality

## Advanced Configuration

For production deployments, consider:

- Using Azure Key Vault to store your Speech Service key
- Implementing Managed Identity for secure authentication
- Setting up monitoring and alerts for usage tracking
- Implementing retry logic for transient errors

## Reference Documentation

- [Azure Speech Service Documentation](https://docs.microsoft.com/azure/cognitive-services/speech-service/)
- [Speech SDK Documentation](https://docs.microsoft.com/azure/cognitive-services/speech-service/speech-sdk)

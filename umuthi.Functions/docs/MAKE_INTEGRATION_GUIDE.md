# Make.com Integration Guide

This guide explains how to integrate the Audio Conversion API with make.com (formerly Integromat).

## Prerequisites

- A make.com account
- Your Audio Conversion API deployed (or running locally for testing)
- Your API key (default: `umuthi-dev-api-key`)

## Setting Up the Integration

### 1. Create a New Scenario in make.com

1. Log in to your make.com account
2. Click "Create a new scenario"
3. Choose a trigger (e.g., "Schedule", "Webhook", or any other trigger that matches your use case)

### 2. Add an HTTP Module

1. Click the "+" button to add a new module
2. Search for and select "HTTP"
3. Choose the appropriate HTTP method (POST for conversions, GET for formats)

### 3. Configure the HTTP Module

For API format information:

```
Method: GET
URL: https://your-function-app.azurewebsites.net/api/GetSupportedFormats
Headers:
  - Name: x-api-key
  - Value: umuthi-dev-api-key (or your custom API key)
```

For WAV to MP3 conversion:

```
Method: POST
URL: https://your-function-app.azurewebsites.net/api/ConvertWavToMp3
Headers:
  - Name: x-api-key
  - Value: umuthi-dev-api-key (or your custom API key)
Body type: Multipart/Form-Data
Multipart:
  - Name: file (or any name)
  - Value: Select "Map" and map to your file data
```

For MPEG to MP3 conversion:

```
Method: POST
URL: https://your-function-app.azurewebsites.net/api/ConvertMpegToMp3
Headers:
  - Name: x-api-key
  - Value: umuthi-dev-api-key (or your custom API key)
Body type: Multipart/Form-Data
Multipart:
  - Name: file (or any name)
  - Value: Select "Map" and map to your file data
```

For audio to transcript conversion:

```
Method: POST
URL: https://your-function-app.azurewebsites.net/api/ConvertMpegToTranscript?language=en-US&timestamps=false
Headers:
  - Name: x-api-key
  - Value: umuthi-dev-api-key (or your custom API key)
Body type: Multipart/Form-Data
Multipart:
  - Name: file (or any name)
  - Value: Select "Map" and map to your file data
```

### 4. Process the Response

After the HTTP request, you'll get a response that you can process in subsequent modules:

- For MP3 conversions: The response will be binary data (the MP3 file)
- For transcript conversions: The response will be text or JSON (depending on timestamps parameter)

## Example Scenarios

### 1. Email-to-Transcript Workflow

This scenario converts audio attachments from emails to text transcripts:

1. **Gmail/Email Trigger**: Watch for new emails with audio attachments
2. **HTTP Module**: Configure as shown above for "audio to transcript conversion"
3. **Text Parser** (Optional): Process the transcript as needed
4. **Email Module**: Send the transcript to a recipient

### 2. Video-to-MP3 Archive

This scenario extracts audio from video files and archives them:

1. **Dropbox/Google Drive Trigger**: Watch for new video files
2. **HTTP Module**: Configure as shown above for "MPEG to MP3 conversion"
3. **Dropbox/Google Drive Module**: Save the resulting MP3 file to an archive folder

## Troubleshooting

If you encounter issues with the integration, check the following:

- **401 Unauthorized**: Verify your API key is correct and properly formatted in the header
- **400 Bad Request**: Check that you're sending the correct file format and staying within size limits
- **500 Server Error**: Contact the API administrator for assistance

## API Key for make.com

For production use, it's recommended to use a dedicated API key for your make.com integration. You can use:

```
make-integration-key
```

This key is specifically configured for make.com integrations and can be used to track and manage your make.com-specific API usage.

## Rate Limiting and Quotas

The API may have rate limits and quotas, especially for resource-intensive operations like speech-to-text conversion. If you're planning high-volume usage, contact the API administrator to discuss your needs.

## Testing the Integration

Before deploying a scenario to production, it's recommended to:

1. Test with various file types within the supported formats
2. Test with files of different sizes
3. Validate the output format meets your needs
4. Set up error handling in your scenario to catch and process any API errors

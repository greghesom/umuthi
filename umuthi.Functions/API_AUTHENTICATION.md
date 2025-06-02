# Audio Conversion API Authentication Guide

This guide explains how to authenticate with the Audio Conversion API, particularly when using it with make.com (formerly Integromat).

## API Key Authentication

All endpoints in the Audio Conversion API are secured with API key authentication. You need to provide a valid API key to access these endpoints.

### API Key

For development and testing, the default API key is:

```
umuthi-dev-api-key
```

For make.com integration, a dedicated API key is available:

```
make-integration-key
```

For testing purposes, additional API keys are also configured:

```
test-key-1
test-key-2
```

In production, you should set a strong, unique API key in your Azure Function's application settings by configuring the following environment variables:
- `ApiKey`: Your primary API key
- `AdditionalApiKeys`: A comma-separated list of additional valid API keys (useful for integrations with different systems)

### Authentication Methods

There are two ways to provide your API key:

1. **HTTP Header (Recommended)**
   - Header Name: `x-api-key`
   - Header Value: Your API key

2. **Query Parameter**
   - Parameter Name: `code`
   - Parameter Value: Your API key

## Using with make.com (Integromat)

When setting up a scenario in make.com to use this API, follow these steps:

1. Add an **HTTP** module to your scenario
2. Configure the module:
   - Method: Select appropriate method (POST for conversions, GET for getting formats)
   - URL: Your function URL (e.g., `https://your-function-app.azurewebsites.net/api/ConvertWavToMp3`)
   - Headers: Add header `x-api-key` with value `umuthi-dev-api-key` (or your custom API key)

### Example Configuration in make.com

**For WAV to MP3 Conversion:**

1. HTTP Module Settings:
   - Method: POST
   - URL: `https://your-function-app.azurewebsites.net/api/ConvertWavToMp3`
   - Headers:
     - `x-api-key`: `umuthi-dev-api-key`
   - Body Type: Multipart/Form-Data
   - Multipart: Add your WAV file (field name can be any name)

**For MPEG to Transcript Conversion:**

1. HTTP Module Settings:
   - Method: POST
   - URL: `https://your-function-app.azurewebsites.net/api/ConvertMpegToTranscript?language=en-US&timestamps=false`
   - Headers:
     - `x-api-key`: `umuthi-dev-api-key`
   - Body Type: Multipart/Form-Data
   - Multipart: Add your audio file(s) (field name can be any name)

## Testing with curl

You can test your API authentication using curl:

```bash
# Test with header authentication
curl -X GET https://your-function-app.azurewebsites.net/api/GetSupportedFormats -H "x-api-key: umuthi-dev-api-key"

# Test with query parameter authentication
curl -X GET "https://your-function-app.azurewebsites.net/api/GetSupportedFormats?code=umuthi-dev-api-key"

# Test file conversion with header authentication
curl -X POST https://your-function-app.azurewebsites.net/api/ConvertWavToMp3 -H "x-api-key: umuthi-dev-api-key" -F "file=@/path/to/your/audio.wav" --output converted.mp3
```

## Troubleshooting

If you receive a 401 Unauthorized response, check:

1. You've provided the correct API key
2. The API key is properly formatted in the header or query parameter
3. The environment variable `ApiKey` is correctly set in your Azure Function App settings

## Security Considerations

1. Always use HTTPS for your API requests
2. Consider implementing IP restrictions in production
3. Rotate your API keys periodically
4. Set up proper monitoring and alerting for failed authentication attempts

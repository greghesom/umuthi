# Make.com Integration Guide

This guide explains how to integrate the Umuthi API with make.com (formerly Integromat) for audio processing and SEO data retrieval.

## Prerequisites

- A make.com account
- Your Umuthi API deployed (or running locally for testing)
- Your API key (default: `umuthi-dev-api-key`)
- For SEO functions: SE Ranking API access and project IDs

## API Capabilities

### Audio Processing
- WAV to MP3 conversion
- MPEG to MP3 conversion  
- Audio to transcript conversion
- Fast transcription with timestamps

### SEO Data Retrieval
- **NEW:** SEO audit reports for domains
- **NEW:** Keywords ranking data for SE Ranking projects
- **NEW:** Competitor analysis and comparison
- **NEW:** Long-running comprehensive reports with webhook notifications

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

For SEO audit report:

```
Method: GET
URL: https://your-function-app.azurewebsites.net/api/GetSEOAuditReport?domain=example.com
Headers:
  - Name: x-api-key
  - Value: umuthi-dev-api-key (or your custom API key)
```

For SEO keywords data:

```
Method: GET
URL: https://your-function-app.azurewebsites.net/api/GetSEOKeywordsData?projectId=your-se-ranking-project-id
Headers:
  - Name: x-api-key
  - Value: umuthi-dev-api-key (or your custom API key)
```

For SEO competitor analysis:

```
Method: GET
URL: https://your-function-app.azurewebsites.net/api/GetSEOCompetitorAnalysis?projectId=your-project-id&competitorDomain=competitor.com
Headers:
  - Name: x-api-key
  - Value: umuthi-dev-api-key (or your custom API key)
```

For long-running SEO comprehensive reports:

```
Method: POST
URL: https://your-function-app.azurewebsites.net/api/RequestSEOComprehensiveReport?webhookUrl=https://your-webhook-endpoint.com/seo-ready
Headers:
  - Name: x-api-key
  - Value: umuthi-dev-api-key (or your custom API key)
  - Name: Content-Type
  - Value: application/json
Body type: Raw
Body:
{
  "projectId": "your-se-ranking-project-id",
  "reportType": "comprehensive",
  "historicalDays": 90,
  "includeCompetitors": true,
  "competitorDomains": ["competitor1.com", "competitor2.com"]
}
```

### 4. Process the Response

After the HTTP request, you'll get a response that you can process in subsequent modules:

- For MP3 conversions: The response will be binary data (the MP3 file)
- For transcript conversions: The response will be text or JSON (depending on timestamps parameter)
- **For SEO data**: The response will be JSON containing SEO metrics, rankings, and analysis data
- **For async SEO reports**: Initial response contains tracking ID, final data comes via webhook

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

### 3. **NEW: SEO Monitoring Dashboard**

This scenario creates an automated SEO monitoring workflow:

1. **Schedule Trigger**: Run daily or weekly
2. **HTTP Module**: Get SEO audit report for your domain
3. **HTTP Module**: Get keywords data for your SE Ranking project
4. **HTTP Module**: Get competitor analysis data
5. **Data Store/Google Sheets**: Save historical SEO data
6. **Email/Slack**: Send alerts for significant ranking changes

### 4. **NEW: Comprehensive SEO Report Automation**

This scenario handles long-running SEO reports:

1. **Schedule Trigger**: Run monthly for comprehensive analysis
2. **HTTP Module**: Request comprehensive SEO report (POST)
3. **Webhook**: Wait for report completion notification
4. **HTTP Module**: Retrieve completed report data
5. **Google Drive/Email**: Distribute the comprehensive report

## Troubleshooting

If you encounter issues with the integration, check the following:

### General Issues
- **401 Unauthorized**: Verify your API key is correct and properly formatted in the header
- **400 Bad Request**: Check that you're sending the correct file format and staying within size limits
- **500 Server Error**: Contact the API administrator for assistance

### SEO-Specific Issues
- **"SE Ranking API key not configured"**: Ensure SE Ranking API credentials are set in the function app
- **"Invalid project ID"**: Verify the SE Ranking project ID exists and you have access
- **"Failed to fetch SEO data"**: Check SE Ranking account limits and API status
- **Webhook not received for long reports**: Verify webhook URL is accessible and accepts POST requests
- **Slow SEO responses**: First call may be slow (fresh data), subsequent calls are faster (cached)

### SEO Data Interpretation
- **Empty keywords data**: Project may not have keywords set up in SE Ranking
- **No competitor data**: Verify competitor domain is set up in SE Ranking project
- **Missing audit data**: Domain may not be accessible for crawling or may have robots.txt restrictions

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

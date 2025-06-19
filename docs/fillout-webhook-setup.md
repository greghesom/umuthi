# Fillout.com Webhook Integration

This document describes how to configure and use the Fillout.com webhook receiver function.

## Overview

The webhook receiver accepts form submissions from Fillout.com and stores them in the database for processing. It provides idempotent handling, signature validation, and comprehensive error handling.

## Endpoint

- **URL**: `/api/webhooks/fillout`
- **Method**: `POST`
- **Authorization**: Function-level
- **Content-Type**: `application/json`

## Configuration

### Function App Settings

Add the following settings to your Function App configuration or `local.settings.json`:

```json
{
  "FilloutWebhook:Secret": "your-webhook-secret-from-fillout",
  "FilloutWebhook:MaxPayloadSize": "1048576",
  "FilloutWebhook:EnableSignatureValidation": "true"
}
```

### Database Migration

Before deploying, ensure the database migration has been applied:

```bash
dotnet ef database update --startup-project umuthi.ApiService --project umuthi.Infrastructure
```

## Fillout.com Setup

1. In your Fillout.com form settings, navigate to the Webhook section
2. Set the webhook URL to: `https://your-function-app.azurewebsites.net/api/webhooks/fillout?code=YOUR_FUNCTION_KEY`
3. If signature validation is enabled, configure the secret in Fillout.com
4. Set the webhook to trigger on form submission

## Webhook Payload

The webhook expects a JSON payload with the following structure:

```json
{
  "submissionId": "clq1234567890",
  "submissionTime": "2024-01-20T10:30:00Z",
  "formId": "clpFormId123",
  "formName": "Business Audit Request",
  "fields": {
    "yourBusinessName": "Acme Corp",
    "yourWebsiteUrl": "https://acmecorp.com",
    "businessStage": "Growth",
    // ... other dynamic fields
  },
  "metadata": {
    "userAgent": "Fillout-Webhook/1.0",
    "ip": "192.168.1.1",
    "timestamp": "2024-01-20T10:30:00Z"
  }
}
```

## Response Codes

- **200 OK**: Submission processed successfully
- **400 Bad Request**: Invalid payload, missing fields, or payload too large
- **401 Unauthorized**: Invalid or missing signature
- **500 Internal Server Error**: Processing error

## Features

### Idempotent Processing
- Duplicate submissions (same `submissionId`) are detected and handled gracefully
- Returns success response for duplicates without reprocessing

### Security
- Function-level authorization required
- Optional webhook signature validation using HMAC-SHA256
- Request size validation (1MB limit)
- IP allowlisting (can be configured)

### Error Handling
- Comprehensive validation of incoming requests
- Structured error responses with correlation IDs
- Detailed logging for troubleshooting

### Performance
- Optimized database schema with proper indexes
- Response time target < 2 seconds
- Supports up to 100 concurrent requests

## Database Schema

Submissions are stored in the `FilloutSubmissions` table with:

- Unique constraint on `SubmissionId` for idempotency
- Indexes on frequently queried fields
- JSON storage for flexible form field data
- Processing status tracking for retry logic

## Monitoring

### Logging
All webhook requests are logged with:
- Correlation IDs for tracking
- Request/response details
- Processing time metrics
- Error details for failed requests

### Recommended Alerts
- High error rate (>5% in 5 minutes)
- Slow response times (>5 seconds)
- Failed signature validations
- Database connection errors

## Troubleshooting

### Common Issues

1. **401 Unauthorized**
   - Check function key in URL
   - Verify signature validation configuration

2. **400 Bad Request**
   - Validate JSON payload structure
   - Check required fields are present
   - Verify payload size < 1MB

3. **500 Internal Server Error**
   - Check database connection
   - Review application logs
   - Verify all dependencies are available

### Testing

You can test the webhook endpoint using curl:

```bash
curl -X POST "https://your-function-app.azurewebsites.net/api/webhooks/fillout?code=YOUR_FUNCTION_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "submissionId": "test123",
    "formId": "testform",
    "formName": "Test Form",
    "submissionTime": "2024-01-20T10:30:00Z",
    "fields": {"testField": "testValue"}
  }'
```

## Support

For issues related to the webhook integration, check:
1. Function App logs in Azure Portal
2. Application Insights telemetry
3. Database connection status
4. Fillout.com webhook delivery logs
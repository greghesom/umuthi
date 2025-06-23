# SEO API Functions - SE Ranking Integration Guide

This guide explains how to use the SEO API functions for retrieving SE Ranking data through the Umuthi API endpoints. All endpoints include billing tracking and support caching for optimal performance.

## Table of Contents

- [Authentication](#authentication)
- [Quick Start](#quick-start)
- [Endpoints](#endpoints)
- [Long-Running Reports](#long-running-reports)
- [Caching Behavior](#caching-behavior)
- [Error Handling](#error-handling)
- [Make.com Integration](#makecom-integration)
- [Billing and Usage Tracking](#billing-and-usage-tracking)

## Authentication

All SEO API endpoints require API key authentication. See [API Authentication Guide](API_AUTHENTICATION.md) for details.

**Required Headers:**
```
x-api-key: your-api-key
```

**SE Ranking Configuration:**
You must configure your SE Ranking API credentials in the Azure Function App settings:
```
SEORanking:ApiKey=your-seranking-api-key
SEORanking:BaseUrl=https://api.seranking.com/
```

## Quick Start

### 1. Get SEO Audit Report
```bash
curl -X GET "https://your-function-app.azurewebsites.net/api/GetSEOAuditReport?domain=example.com" \
  -H "x-api-key: umuthi-dev-api-key"
```

### 2. Get Keywords Data
```bash
curl -X GET "https://your-function-app.azurewebsites.net/api/GetSEOKeywordsData?projectId=your-project-id" \
  -H "x-api-key: umuthi-dev-api-key"
```

### 3. Get Competitor Analysis
```bash
curl -X GET "https://your-function-app.azurewebsites.net/api/GetSEOCompetitorAnalysis?projectId=your-project-id&competitorDomain=competitor.com" \
  -H "x-api-key: umuthi-dev-api-key"
```

## Endpoints

### Get SEO Audit Report

Retrieves a comprehensive SEO audit report for a specified domain.

**Endpoint:** `GET /api/GetSEOAuditReport`

**Parameters:**
- `domain` (required): Domain to audit (e.g., "example.com")

**Response:** SEO audit report with technical issues, content issues, and performance metrics.

**Example Response:**
```json
{
  "domain": "example.com",
  "overallScore": 85,
  "technicalIssues": [
    {
      "type": "Missing Meta Description",
      "severity": "Medium",
      "description": "5 pages are missing meta descriptions",
      "affectedCount": 5,
      "recommendation": "Add unique meta descriptions to all pages"
    }
  ],
  "contentIssues": [],
  "performance": {
    "loadSpeedSeconds": 2.3,
    "mobileFriendliness": 95,
    "coreWebVitals": 88,
    "indexedPages": 127
  },
  "generatedAt": "2023-12-01T10:00:00Z",
  "cachedAt": "2023-12-01T10:00:00Z"
}
```

### Get SEO Keywords Data

Retrieves keyword ranking data for a SE Ranking project.

**Endpoint:** `GET /api/GetSEOKeywordsData`

**Parameters:**
- `projectId` (required): SE Ranking project ID

**Response:** Keywords ranking data with position tracking and metrics.

**Example Response:**
```json
{
  "projectId": "proj123",
  "totalKeywords": 100,
  "top10Keywords": 15,
  "top50Keywords": 45,
  "averagePosition": 25.5,
  "keywords": [
    {
      "keyword": "example keyword",
      "position": 5,
      "previousPosition": 8,
      "positionChange": 3,
      "searchVolume": 1200,
      "difficulty": 35,
      "rankingUrl": "https://example.com/page"
    }
  ],
  "generatedAt": "2023-12-01T10:00:00Z",
  "cachedAt": "2023-12-01T10:00:00Z"
}
```

### Get SEO Competitor Analysis

Retrieves competitor analysis data comparing your project with a competitor domain.

**Endpoint:** `GET /api/GetSEOCompetitorAnalysis`

**Parameters:**
- `projectId` (required): SE Ranking project ID
- `competitorDomain` (required): Competitor domain to analyze

**Response:** Competitor analysis with common keywords, missed opportunities, and top pages.

**Example Response:**
```json
{
  "projectId": "proj123",
  "competitorDomain": "competitor.com",
  "visibilityScore": 0.75,
  "commonKeywords": [
    {
      "keyword": "shared keyword",
      "projectPosition": 8,
      "competitorPosition": 3,
      "searchVolume": 800,
      "difficulty": 45,
      "competitorUrl": "https://competitor.com/page"
    }
  ],
  "missedOpportunities": [],
  "topPages": [
    {
      "url": "https://competitor.com/top-page",
      "keywordCount": 25,
      "estimatedTraffic": 1500,
      "title": "Competitor's Top Page"
    }
  ],
  "generatedAt": "2023-12-01T10:00:00Z",
  "cachedAt": "2023-12-01T10:00:00Z"
}
```

## Long-Running Reports

For comprehensive reports that take more than 30 seconds to generate, use the async report pattern:

### 1. Request Comprehensive Report

**Endpoint:** `POST /api/RequestSEOComprehensiveReport`

**Parameters:**
- `webhookUrl` (query parameter or X-Webhook-Url header): URL to notify when report is ready

**Body:**
```json
{
  "projectId": "your-project-id",
  "reportType": "comprehensive",
  "historicalDays": 90,
  "includeCompetitors": true,
  "competitorDomains": ["competitor1.com", "competitor2.com"],
  "parameters": {
    "includeBacklinks": true,
    "includeContentAnalysis": true
  }
}
```

**Response (202 Accepted):**
```json
{
  "trackingId": "report-abc123",
  "status": "pending",
  "estimatedCompletion": "2023-12-01T10:10:00Z",
  "webhookUrl": "https://your-webhook-endpoint.com/seo-report-ready"
}
```

### 2. Check Report Status

**Endpoint:** `GET /api/GetSEOReportStatus?trackingId=report-abc123`

**Response:**
```json
{
  "trackingId": "report-abc123",
  "status": "processing",
  "progressPercentage": 65,
  "message": "Analyzing competitor data...",
  "lastUpdated": "2023-12-01T10:05:00Z",
  "isReady": false
}
```

### 3. Retrieve Completed Report

**Endpoint:** `GET /api/GetSEOComprehensiveReport?trackingId=report-abc123`

**Response:** Complete comprehensive report with all requested data.

### 4. Webhook Notifications

When your report is ready, we'll send a POST request to your webhook URL:

```json
{
  "trackingId": "report-abc123",
  "status": "completed",
  "completedAt": "2023-12-01T10:08:00Z",
  "reportUrl": "/api/GetSEOComprehensiveReport?trackingId=report-abc123",
  "correlationId": "correlation-xyz789"
}
```

## Caching Behavior

To optimize performance and reduce SE Ranking API calls:

- **Audit Reports**: Cached for 1 hour
- **Keywords Data**: Cached for 30 minutes  
- **Competitor Analysis**: Cached for 2 hours
- **Comprehensive Reports**: Not cached (always fresh)

**Cache Headers:**
- Cached responses include `cachedAt` timestamp
- Fresh responses have `cachedAt` as null
- Response time < 5 seconds for cached data
- Response time < 30 seconds for fresh data

## Error Handling

### Common Error Responses

**401 Unauthorized:**
```json
{
  "error": "Unauthorized",
  "message": "Invalid or missing API key"
}
```

**400 Bad Request:**
```json
{
  "error": "Bad Request", 
  "message": "Domain parameter is required"
}
```

**500 Internal Server Error:**
```json
{
  "error": "Internal Server Error",
  "message": "Failed to fetch SEO data from SE Ranking"
}
```

### SE Ranking API Errors

If SE Ranking API is unavailable:
- Cached data will be returned if available
- Error responses include details about the upstream failure
- Retry logic handles temporary SE Ranking outages

## Make.com Integration

### Basic SEO Data Retrieval

1. **HTTP Module**: Configure GET request to SEO endpoints
2. **Headers**: Add `x-api-key` with your API key
3. **URL Parameters**: Set required parameters (domain, projectId, etc.)
4. **Parse Response**: Use JSON parser to extract data

### Long-Running Reports

1. **Request Report**: POST to `RequestSEOComprehensiveReport`
2. **Webhook Setup**: Configure webhook endpoint in Make.com
3. **Wait for Notification**: Receive webhook when report is ready
4. **Retrieve Report**: GET from `GetSEOComprehensiveReport`

**Example Make.com Scenario:**
```
[HTTP] Request Report → [Webhook] Wait for Completion → [HTTP] Get Report → [Process] Handle Data
```

## Billing and Usage Tracking

All SEO API calls are tracked for billing purposes:

### Tracked Metrics
- API call count
- Input/output data sizes
- Processing duration  
- Success/failure rates
- Cache hit/miss ratios

### Operation Types
- `SEOAuditReport` - Domain audit reports
- `SEOKeywordsData` - Keywords ranking data
- `SEOCompetitorAnalysis` - Competitor analysis
- `SEOComprehensiveReport` - Long-running comprehensive reports

### Billing Summary

Get billing information using the existing analytics endpoints:
```bash
curl -X GET "https://your-function-app.azurewebsites.net/api/GetBillingSummary?customerId=your-customer-id&startDate=2023-11-01&endDate=2023-11-30" \
  -H "x-api-key: umuthi-dev-api-key"
```

## Rate Limits

SEO endpoints share the same rate limits as other API functions:

- **Development Key**: Unlimited
- **Make.com Integration Key**: 120/min, 3000/hour, 30000/day  
- **Production Keys**: Configurable per customer

## Support and Troubleshooting

### Configuration Checklist
- [ ] SE Ranking API key configured
- [ ] API authentication working
- [ ] Project IDs are valid in SE Ranking
- [ ] Webhook URLs are accessible (for async reports)

### Common Issues
1. **"SE Ranking API key not configured"**: Set `SEORanking:ApiKey` in app settings
2. **"Failed to fetch SEO data"**: Check SE Ranking API key and account limits
3. **Slow responses**: Check if caching is working, verify SE Ranking API performance
4. **Missing webhook notifications**: Verify webhook URL is accessible and accepting POST requests

### Logs and Monitoring
- All API calls are logged with correlation IDs
- Performance metrics tracked in Application Insights
- Usage analytics available via billing endpoints

For additional support, check the [Monitoring Guide](MONITORING_GUIDE.md) for detailed troubleshooting steps.
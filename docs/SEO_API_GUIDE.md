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

# Optional: SE Ranking Data API configuration (uses regular API as fallback)
SEORanking:DataApiKey=your-seranking-data-api-key
SEORanking:DataApiUrl=https://api4.seranking.com/

# Optional: Sandbox configuration for testing
SEORanking:SandboxApiKey=your-sandbox-api-key
SEORanking:SandboxApiUrl=https://sandbox-api4.seranking.com/
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

### 4. Keywords Research
```bash
curl -X POST "https://your-function-app.azurewebsites.net/api/keywords/research" \
  -H "Content-Type: application/json" \
  -H "x-api-key: umuthi-dev-api-key" \
  -d '{
    "keywords": "seo platform\nsearch engine help\nanother keyword",
    "regionCode": "za"
  }'
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

### Keywords Research

Get comprehensive keyword research data with metrics including search volume, CPC, competition, difficulty, and historical trends.

**Endpoint:** `POST /api/keywords/research`

**Request Body:**
```json
{
  "keywords": "seo platform\nsearch engine help\nanother keyword",
  "regionCode": "za",
  "sortBy": "volume",
  "sortDirection": "desc",
  "minSearchVolume": 500,
  "maxDifficulty": 50,
  "includeHistoricalTrends": true
}
```

**Parameters:**
- `keywords` (required): Keywords to research, each on a new line
- `regionCode` (required): Alpha-2 country code (e.g., "US", "ZA", "GB")
- `sortBy` (optional): Sort field - "volume", "difficulty", "cpc", "competition", "keyword"
- `sortDirection` (optional): "asc" or "desc" (default: "desc")
- `minSearchVolume` (optional): Minimum search volume filter
- `maxDifficulty` (optional): Maximum difficulty filter (0-100)
- `includeHistoricalTrends` (optional): Include 12-month historical data (default: false)

**Limits:**
- Maximum 100 keywords per request
- Region code must be valid Alpha-2 format
- Difficulty must be between 0-100
- Search volume must be positive

**Response:**
```json
{
  "regionCode": "za",
  "totalKeywords": 3,
  "processedKeywords": 3,
  "keywords": [
    {
      "keyword": "seo platform",
      "searchVolume": 1200,
      "difficulty": 45,
      "competition": "medium",
      "costPerClick": 2.50,
      "estimatedClicks": 120,
      "resultsCount": 45000000,
      "serpFeatures": ["featured_snippet", "people_also_ask"],
      "relatedKeywords": ["seo tool", "seo software"],
      "longTailVariations": ["best seo platform", "seo platform comparison"],
      "historicalTrends": [
        {
          "date": "2023-11-01T00:00:00Z",
          "searchVolume": 1100,
          "difficulty": 43,
          "costPerClick": 2.30
        }
      ]
    }
  ],
  "summary": {
    "averageSearchVolume": 833.33,
    "averageDifficulty": 33.33,
    "averageCostPerClick": 1.83,
    "totalTrafficPotential": 1250,
    "lowCompetitionPercentage": 66.67,
    "highVolumeKeywordsCount": 2,
    "topOpportunityKeywords": ["search engine help", "another keyword"]
  },
  "generatedAt": "2023-12-01T10:00:00Z",
  "cachedAt": "2023-12-01T10:00:00Z"
}
```

**Example Request (curl):**
```bash
curl -X POST "https://your-function-app.azurewebsites.net/api/keywords/research" \
  -H "Content-Type: application/json" \
  -H "x-api-key: umuthi-dev-api-key" \
  -d '{
    "keywords": "seo platform\nsearch engine help\nanother keyword",
    "regionCode": "za",
    "sortBy": "volume",
    "sortDirection": "desc",
    "includeHistoricalTrends": true
  }'
```

**Caching:** 4 hours (cached data is marked with `cachedAt` timestamp)

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

## SE Ranking Data API Endpoints

The following endpoints provide direct access to SE Ranking's Data API for comprehensive SEO analysis and competitor intelligence.

### Get Domain Overview Data

Retrieves comprehensive domain overview data including authority metrics, organic keywords, traffic estimates, and backlinks data.

**Endpoint:** `GET /api/GetSEODomainOverview`

**Parameters:**
- `domain` (required): Domain to analyze (e.g., "example.com")

**Response:** Domain overview data with key SEO metrics.

**Example Request:**
```
GET /api/GetSEODomainOverview?domain=example.com
```

**Example Response:**
```json
{
  "domain": "example.com",
  "domainAuthority": 75,
  "organicKeywords": 12500,
  "organicTraffic": 150000,
  "backlinksCount": 45000,
  "referringDomains": 3200,
  "domainRating": 72,
  "generatedAt": "2024-01-15T10:30:00Z",
  "cachedAt": "2024-01-15T10:30:00Z"
}
```

### Get Domain Keyword Positions

Retrieves keyword positions data for a domain across different search engines and locations.

**Endpoint:** `GET /api/GetSEODomainPositions`

**Parameters:**
- `domain` (required): Domain to analyze
- `searchEngine` (optional): Search engine (google, bing, yahoo) - defaults to "google"
- `location` (optional): Location/country code - defaults to "US"

**Response:** Domain keyword positions with detailed ranking data.

**Example Request:**
```
GET /api/GetSEODomainPositions?domain=example.com&searchEngine=google&location=US
```

### Get Domain Competitors

Discovers and analyzes top competitors for a given domain.

**Endpoint:** `GET /api/GetSEODomainCompetitors`

**Parameters:**
- `domain` (required): Domain to analyze competitors for

**Response:** List of discovered competitors with competition metrics.

### Get Keywords Overview

Retrieves comprehensive keywords overview data for a SE Ranking project.

**Endpoint:** `GET /api/GetSEOKeywordsOverview`

**Parameters:**
- `projectId` (required): SE Ranking project ID

**Response:** Keywords overview with performance metrics and trends.

**Example Response:**
```json
{
  "projectId": "project_123456",
  "totalKeywords": 2500,
  "improvedKeywords": 150,
  "declinedKeywords": 75,
  "newKeywords": 45,
  "lostKeywords": 30,
  "averagePosition": 15.7,
  "visibilityScore": 85.3,
  "generatedAt": "2024-01-15T10:30:00Z",
  "cachedAt": "2024-01-15T10:30:00Z"
}
```

### Get Keyword Positions Tracking

Retrieves detailed keyword position tracking data with historical performance.

**Endpoint:** `GET /api/GetSEOKeywordPositions`

**Parameters:**
- `projectId` (required): SE Ranking project ID
- `searchEngine` (optional): Search engine - defaults to "google"
- `location` (optional): Location/country code - defaults to "US"
- `device` (optional): Device type (desktop, mobile) - defaults to "desktop"

### Get SERP Features

Analyzes SERP features for specific keywords including featured snippets, people also ask, etc.

**Endpoint:** `GET /api/GetSEOSerpFeatures`

**Parameters:**
- `keyword` (required): Keyword to analyze
- `searchEngine` (optional): Search engine - defaults to "google"
- `location` (optional): Location/country code - defaults to "US"

### Get Search Volume Data

Retrieves search volume data for multiple keywords.

**Endpoint:** `GET /api/GetSEOSearchVolume`

**Parameters:**
- `keywords` (required): Comma-separated list of keywords
- `location` (optional): Location/country code - defaults to "US"

### Get Backlinks Overview

Retrieves comprehensive backlinks overview including total backlinks, referring domains, and link distribution.

**Endpoint:** `GET /api/GetSEOBacklinksOverview`

**Parameters:**
- `domain` (required): Domain to analyze backlinks for

### Get Detailed Backlinks

Retrieves detailed backlinks data with individual link information.

**Endpoint:** `GET /api/GetSEOBacklinksDetailed`

**Parameters:**
- `domain` (required): Domain to analyze
- `limit` (optional): Maximum number of backlinks to return - defaults to 100

### Get Anchor Text Analysis

Analyzes anchor text distribution for domain backlinks.

**Endpoint:** `GET /api/GetSEOAnchorText`

**Parameters:**
- `domain` (required): Domain to analyze

### Get Competitors Overview

Discovers and analyzes top competitors with comprehensive competitive intelligence.

**Endpoint:** `GET /api/GetSEOCompetitorsOverview`

**Parameters:**
- `domain` (required): Domain to analyze competitors for

### Get Shared Keywords

Analyzes shared keywords between your domain and a specific competitor.

**Endpoint:** `GET /api/GetSEOSharedKeywords`

**Parameters:**
- `domain` (required): Your domain
- `competitorDomain` (required): Competitor domain to compare against

### Get Keyword Gap Analysis

Identifies keyword gaps and opportunities compared to competitors.

**Endpoint:** `GET /api/GetSEOKeywordGap`

**Parameters:**
- `domain` (required): Your domain
- `competitorDomain` (required): Competitor domain to analyze gaps against

### Get SERP Results

Retrieves detailed SERP results data for specific keywords.

**Endpoint:** `GET /api/GetSEOSerpResults`

**Parameters:**
- `keyword` (required): Keyword to search for
- `searchEngine` (optional): Search engine - defaults to "google"
- `location` (optional): Location/country code - defaults to "US"
- `device` (optional): Device type (desktop, mobile) - defaults to "desktop"

## Caching Behavior

To optimize performance and reduce SE Ranking API calls, different endpoint categories have optimized cache durations:

### Standard SEO Endpoints
- **Audit Reports**: Cached for 1 hour
- **Keywords Data**: Cached for 30 minutes  
- **Competitor Analysis**: Cached for 2 hours
- **Comprehensive Reports**: Not cached (always fresh)

### SE Ranking Data API Endpoints
- **Domain Data** (Overview, Positions, Competitors): Cached for 6 hours
- **Keywords Data** (Overview, Positions, Search Volume): Cached for 2 hours
- **Backlinks Data** (Overview, Detailed, Anchors): Cached for 12 hours
- **SERP Data** (Features, Results): Cached for 1 hour
- **Competitors Data** (Overview, Keywords, Gaps): Cached for 6 hours

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

### SE Ranking Data API Modules

The new SE Ranking Data API integration provides dedicated Make.com modules for comprehensive SEO analysis:

#### Domain Analysis Module
- **Module Name**: `seranking/getdomaindata`
- **Purpose**: Get domain overview data including authority, traffic, and backlinks
- **Required Parameters**: `domain`
- **Use Case**: Quick domain analysis and competitive research

**Example Workflow:**
```
[Domain Input] → [SE Ranking Domain Data] → [Store in Airtable] → [Send Email Report]
```

#### Keywords Analysis Module
- **Module Name**: `seranking/getkeywordsdata`
- **Purpose**: Get keywords overview and performance metrics
- **Required Parameters**: `projectId`
- **Use Case**: Track keyword performance and identify trends

**Example Workflow:**
```
[Project ID] → [SE Ranking Keywords Data] → [Filter Improved Keywords] → [Slack Notification]
```

#### Competitors Analysis Module
- **Module Name**: `seranking/getcompetitorsdata`
- **Purpose**: Discover and analyze top competitors
- **Required Parameters**: `domain`
- **Use Case**: Competitive intelligence and market analysis

**Example Workflow:**
```
[Domain Input] → [SE Ranking Competitors] → [Iterate Competitors] → [Analyze Each Competitor]
```

#### Automated SEO Monitoring Scenarios

**Daily Keyword Tracking:**
```
[Schedule: Daily] → [SE Ranking Keywords] → [Check Position Changes] → [Alert on Drops]
```

**Weekly Competitor Analysis:**
```
[Schedule: Weekly] → [SE Ranking Competitors] → [Compare Metrics] → [Generate Report]
```

**Monthly Domain Health Check:**
```
[Schedule: Monthly] → [SE Ranking Domain Data] → [Track Trends] → [Dashboard Update]
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

#### Standard SEO Operations
- `SEOAuditReport` - Domain audit reports
- `SEOKeywordsData` - Keywords ranking data
- `SEOCompetitorAnalysis` - Competitor analysis
- `SEOComprehensiveReport` - Long-running comprehensive reports
- `KeywordResearch` - Keywords research with comprehensive metrics

#### SE Ranking Data API Operations
- `SEO_DOMAIN_OVERVIEW` - Domain overview data
- `SEO_DOMAIN_POSITIONS` - Domain keyword positions
- `SEO_DOMAIN_COMPETITORS` - Domain competitors analysis
- `SEO_KEYWORDS_OVERVIEW` - Keywords overview data
- `SEO_KEYWORDS_POSITIONS` - Keyword positions tracking
- `SEO_SERP_FEATURES` - SERP features analysis
- `SEO_SEARCH_VOLUME` - Search volume data
- `SEO_BACKLINKS_OVERVIEW` - Backlinks overview
- `SEO_BACKLINKS_DETAILED` - Detailed backlinks data
- `SEO_ANCHOR_TEXT` - Anchor text analysis
- `SEO_COMPETITORS_OVERVIEW` - Competitors overview
- `SEO_SHARED_KEYWORDS` - Shared keywords analysis
- `SEO_KEYWORD_GAP` - Keyword gap analysis
- `SEO_SERP_RESULTS` - SERP results data

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
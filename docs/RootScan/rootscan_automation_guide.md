# RootScan Process Automation Guide

## Overview

This document outlines the complete automation strategy for the RootScan process, from Fillout.com webhook trigger through Make.com orchestration to final Gamma presentation delivery.

## RootScan Process Analysis

### Step 1: In-Depth Keyword Research

#### 1A: Initial Research & Setup
- **Input**: Client website + onboarding form data
- **Process**: Use Perplexity for deep keyword cluster research (8-10 high-impact clusters)
- **Output**: Strategic keyword clusters with sample keywords
- **API Potential**: ✅ High - Could integrate with Perplexity API + keyword research tools

#### 1B: ChatGPT Project Setup
- **Process**: Create project folder, add instructions, convert clusters to UK English keyword lists
- **Output**: Formatted keyword list for SE Ranking
- **API Potential**: ✅ High - ChatGPT API integration possible

#### 1C: SE Ranking Integration
- **Process**: Create project, run keyword analysis, export CSV, group into clusters
- **Output**: Keyword data with search volumes and clusters
- **API Potential**: ⚠️ Medium - SE Ranking has limited API access

#### 1D: Google Sheets Analysis
- **Process**: Create cluster summaries, charts, and insights
- **Output**: Keyword cluster visualization and data
- **API Potential**: ✅ High - Google Sheets API available

### Step 2: Competitive Analysis & Share of Voice

#### 2A: Competitor Identification
- **Input**: 5 competitors from onboarding or AI-generated list
- **Process**: Competitive landscape analysis, URL validation
- **Output**: Structured competitor dataset
- **API Potential**: ✅ High - Can be automated with web scraping/analysis

#### 2B: SE Ranking Competitive Research
- **Process**: Domain trust, organic traffic analysis
- **Output**: Competitive metrics and bubble charts
- **API Potential**: ⚠️ Medium - Limited by SE Ranking API

#### 2C: Social Media & Advertising Analysis
- **Process**: Analyze top 3 competitors' digital presence, ad strategies
- **Output**: Digital visibility summary with screenshots
- **API Potential**: ✅ High - Social media APIs + Meta Ad Library API available

### Step 3: Market Insights & Opportunities

#### 3A: Industry Demand Analysis
- **Process**: Analyze industry shifts and service demand changes
- **Output**: Market trend summary and strategic actions
- **API Potential**: ✅ High - Can leverage research APIs

#### 3B: Competitive Advantage Opportunities
- **Process**: Identify 5 strategic opportunities from competitive analysis
- **Output**: Executive-level opportunity summary
- **API Potential**: ✅ High - Data synthesis possible with AI APIs

#### 3C: Content Opportunities
- **Process**: Highlight content gaps and keyword cluster insights
- **Output**: Content strategy recommendations
- **API Potential**: ✅ High - Can be automated with content analysis

### Step 4: Website Health & Performance Audit

#### 4A: Technical SEO Audit
- **Process**: SE Ranking website audit, health score analysis
- **Output**: Website health report with top issues
- **API Potential**: ⚠️ Medium - Dependent on SE Ranking, but alternatives exist

#### 4B: Report Generation
- **Process**: Export full technical audit report
- **Output**: Branded PDF report with recommendations
- **API Potential**: ✅ High - Can generate reports programmatically

### Step 5: Self-Help Recommendations

#### 5A: Priority-Based Guidance
- **Process**: Create structured recommendations (Priority/Medium/Long-term)
- **Output**: Actionable SEO guidance with 50-word summary
- **API Potential**: ✅ High - AI can synthesize all previous data

#### 5B: Evergreen Blueprint Research
- **Process**: Deep industry research using Perplexity
- **Output**: Comprehensive industry analysis document
- **API Potential**: ✅ High - Research APIs + AI processing

## Core API Functions Architecture

### Function 1: Research Orchestrator API
```
POST /api/rootscan/initialize
- Input: Client URL + onboarding form data
- Orchestrates entire pipeline
- Returns: Process ID for tracking
```

### Function 2: Keyword Intelligence Service
```
POST /api/keyword-research
- Custom Perplexity integration via MCP
- Browser automation for SE Ranking interactions
- Custom clustering algorithms
- Google Sheets API for visualization
- Output: Comprehensive keyword strategy
```

### Function 3: Competitive Intelligence Engine
```
POST /api/competitive-analysis
- Browser automation for SE Ranking competitor research
- Social media APIs + Meta Ad Library scraping
- Custom web scraping for competitor websites
- Screenshot capture automation
- Bubble chart generation
- Output: Complete competitive landscape
```

### Function 4: Market Intelligence Service
```
POST /api/market-insights
- Custom research models
- Industry trend analysis via multiple APIs
- Opportunity identification algorithms
- Content gap analysis
- Output: Strategic market positioning
```

### Function 5: Technical Audit Engine
```
POST /api/website-audit
- Browser automation for SE Ranking audits
- Custom technical SEO crawling
- Performance testing integration
- Health score calculation
- PDF report generation
- Output: Technical audit with recommendations
```

### Function 6: AI Synthesis & Recommendation Engine
```
POST /api/generate-recommendations
- Custom models for data synthesis
- Priority-based recommendation algorithms
- Executive summary generation
- 50-word focus area summaries
- Output: Actionable strategic guidance
```

## Advanced Automation Opportunities

### Browser Automation Functions:
1. **SE Ranking Automator**
   - Auto-create projects
   - Bulk keyword uploads
   - Data extraction and export
   - Competitor analysis automation

2. **Social Media Intelligence**
   - LinkedIn/Instagram/Facebook scraping
   - Ad library data collection
   - Engagement metrics analysis
   - Screenshot capture of competitor pages

3. **Competitor Website Analysis**
   - Homepage screenshot capture
   - Content structure analysis
   - Technical stack detection
   - UX/design pattern identification

### Custom Model Opportunities:
1. **Industry Classification Model**
   - Automatically categorize client industry
   - Identify relevant competitors
   - Suggest appropriate keyword clusters

2. **Content Gap Analysis Model**
   - Analyze competitor content strategies
   - Identify underserved topics
   - Recommend content opportunities

3. **Recommendation Prioritization Model**
   - Score recommendations by impact/effort
   - Customize advice based on client size/resources
   - Generate industry-specific guidance

### MCP Service Integrations:
1. **Research Context Service**
   - Maintain context across multiple AI research calls
   - Cross-reference findings between tools
   - Ensure consistency in recommendations

2. **Data Pipeline Service**
   - Seamlessly pass data between functions
   - Handle rate limiting and retries
   - Manage API quotas across services

## Suggested Function App Structure

### Core APIs:
```
/api/rootscan/
├── /initialize              # Start full process
├── /status/{processId}      # Check progress
├── /keyword-research        # Step 1 automation
├── /competitive-analysis    # Step 2 automation
├── /market-insights        # Step 3 automation
├── /technical-audit        # Step 4 automation
├── /generate-report        # Step 5 + final output
└── /webhook/complete       # Completion notification
```

### Utility APIs:
```
/api/utils/
├── /screenshot             # Browser screenshot service
├── /pdf-generator         # Report generation
├── /chart-creator         # Visualization service
├── /competitor-finder     # AI competitor identification
└── /industry-analyzer     # Industry classification
```

### Data APIs:
```
/api/data/
├── /keywords              # Keyword management
├── /competitors           # Competitor data
├── /insights             # Market insights storage
└── /recommendations      # Generated advice
```

## Make.com Workflow Integration

### Workflow Overview
```
Fillout Webhook → Initialize → Research APIs → Generate Report → Deliver Results
```

### Module 1: Webhook Receiver
```
Trigger: Webhook (from Fillout.com)
URL: https://hook.integromat.com/your-webhook-url
```
**Expected Data:**
```json
{
  "submissionId": "sub_123",
  "clientWebsite": "https://client.com",
  "industry": "digital marketing",
  "competitors": ["comp1.com", "comp2.com", "comp3.com"],
  "services": ["SEO", "Content Strategy", "PPC"],
  "contactEmail": "client@email.com",
  "companyName": "Client Corp"
}
```

### Module 2: Initialize RootScan Process
```
HTTP Module: POST Request
URL: https://your-function-app.azurewebsites.net/api/rootscan/initialize
Headers:
  - x-api-key: {{your-api-key}}
  - Content-Type: application/json
Body:
```
```json
{
  "clientUrl": "{{1.clientWebsite}}",
  "industry": "{{1.industry}}",
  "competitors": "{{1.competitors}}",
  "services": "{{1.services}}",
  "submissionId": "{{1.submissionId}}",
  "clientInfo": {
    "companyName": "{{1.companyName}}",
    "email": "{{1.contactEmail}}"
  }
}
```
**Response Expected:**
```json
{
  "processId": "process_uuid_123",
  "estimatedCompletion": "25 minutes",
  "status": "initialized"
}
```

### Module 3: Keyword Research API
```
HTTP Module: POST Request
URL: https://your-function-app.azurewebsites.net/api/keyword-research
Headers:
  - x-api-key: {{your-api-key}}
  - Content-Type: application/json
Body:
```
```json
{
  "processId": "{{2.processId}}",
  "clientUrl": "{{1.clientWebsite}}",
  "industry": "{{1.industry}}",
  "services": "{{1.services}}",
  "targetRegions": ["UK", "US"]
}
```
**Response Expected:**
```json
{
  "keywordClusters": [...],
  "totalSearchVolume": 125000,
  "competitionLevel": "medium",
  "chartUrl": "https://sheets.googleapis.com/chart123",
  "status": "completed"
}
```

### Module 4: Competitive Analysis API
```
HTTP Module: POST Request
URL: https://your-function-app.azurewebsites.net/api/competitive-analysis
Headers:
  - x-api-key: {{your-api-key}}
  - Content-Type: application/json
Body:
```
```json
{
  "processId": "{{2.processId}}",
  "clientUrl": "{{1.clientWebsite}}",
  "competitors": "{{1.competitors}}",
  "keywordData": "{{3.keywordClusters}}"
}
```
**Response Expected:**
```json
{
  "competitorMetrics": [...],
  "shareOfVoice": {...},
  "bubbleChartUrl": "https://storage.com/bubble-chart.png",
  "opportunities": [...],
  "status": "completed"
}
```

### Module 5: Market Insights API
```
HTTP Module: POST Request
URL: https://your-function-app.azurewebsites.net/api/market-insights
Headers:
  - x-api-key: {{your-api-key}}
  - Content-Type: application/json
Body:
```
```json
{
  "processId": "{{2.processId}}",
  "industry": "{{1.industry}}",
  "services": "{{1.services}}",
  "competitorData": "{{4.competitorMetrics}}"
}
```
**Response Expected:**
```json
{
  "industryTrends": [...],
  "competitiveAdvantages": [...],
  "contentOpportunities": [...],
  "demandShifts": {...},
  "status": "completed"
}
```

### Module 6: Technical Audit API
```
HTTP Module: POST Request
URL: https://your-function-app.azurewebsites.net/api/technical-audit
Headers:
  - x-api-key: {{your-api-key}}
  - Content-Type: application/json
Body:
```
```json
{
  "processId": "{{2.processId}}",
  "clientUrl": "{{1.clientWebsite}}",
  "keywordData": "{{3.keywordClusters}}"
}
```
**Response Expected:**
```json
{
  "healthScore": 69,
  "topIssues": [...],
  "technicalRecommendations": [...],
  "auditReportUrl": "https://storage.com/audit-report.pdf",
  "status": "completed"
}
```

### Module 7: Generate Final Report
```
HTTP Module: POST Request
URL: https://your-function-app.azurewebsites.net/api/generate-report
Headers:
  - x-api-key: {{your-api-key}}
  - Content-Type: application/json
Body:
```
```json
{
  "processId": "{{2.processId}}",
  "clientInfo": {
    "companyName": "{{1.companyName}}",
    "website": "{{1.clientWebsite}}",
    "email": "{{1.contactEmail}}"
  },
  "data": {
    "keywords": "{{3}}",
    "competitors": "{{4}}",
    "marketInsights": "{{5}}",
    "technicalAudit": "{{6}}"
  }
}
```
**Response Expected:**
```json
{
  "reportUrl": "https://storage.com/client-rootscan-report.pdf",
  "presentationUrl": "https://gamma.app/presentation/123",
  "keyFindings": "Focus on content audits and SEO optimization...",
  "status": "completed"
}
```

### Module 8: Send Email Notification
```
Email Module: Send Email
To: {{1.contactEmail}}
Subject: Your RootScan Analysis is Ready - {{1.companyName}}
Body Template:
```
```html
Hi {{1.companyName}} team,

Your comprehensive RootScan analysis is now complete! 🎉

📊 **Key Findings:**
{{7.keyFindings}}

📋 **Your Reports:**
• Full Analysis Report: {{7.reportUrl}}
• Executive Presentation: {{7.presentationUrl}}

**Next Steps:**
Our team will follow up within 24 hours to discuss these insights and potential next steps.

Best regards,
The Umuthi Team
```

### Module 9: Slack Notification (Internal)
```
Slack Module: Send Message
Channel: #rootscan-completions
Message:
```
```
🚀 New RootScan Completed!

**Client:** {{1.companyName}}
**Website:** {{1.clientWebsite}}
**Industry:** {{1.industry}}
**Process ID:** {{2.processId}}

**Health Score:** {{6.healthScore}}/100
**Total Keywords:** {{3.totalSearchVolume}}
**Report:** {{7.reportUrl}}

Time to follow up! 📞
```

### Module 10: CRM Update (Optional)
```
HTTP Module: PATCH Request
URL: https://api.your-crm.com/contacts/{{1.contactEmail}}
Headers:
  - Authorization: Bearer {{crm-token}}
  - Content-Type: application/json
Body:
```
```json
{
  "customFields": {
    "rootscanCompleted": true,
    "rootscanDate": "{{now}}",
    "healthScore": "{{6.healthScore}}",
    "reportUrl": "{{7.reportUrl}}",
    "followUpRequired": true
  },
  "tags": ["rootscan-completed", "{{1.industry}}"]
}
```

## Error Handling Modules

### Error Handler Route
```
Router Module: Set up parallel error handling path
If any HTTP module returns status ≠ 200:
```

### Error Notification
```
Email Module: Send Error Alert
To: operations@umuthi.com
Subject: RootScan Process Failed - {{1.companyName}}
Body:
```
```
❌ RootScan process failed for {{1.companyName}}

**Error Details:**
- Client: {{1.companyName}}
- Website: {{1.clientWebsite}}
- Process ID: {{2.processId}}
- Failed at: {{error.moduleName}}
- Error: {{error.message}}

**Client Contact:** {{1.contactEmail}}

Please investigate and reach out to the client manually.
```

### Client Error Notification
```
Email Module: Send Client Update
To: {{1.contactEmail}}
Subject: Update on Your RootScan Analysis
Body:
```
```
Hi {{1.companyName}} team,

We're currently processing your RootScan analysis and have encountered a technical delay. 

Our team has been notified and will complete your analysis manually within 24 hours.

We apologize for any inconvenience and will be in touch soon with your results.

Best regards,
The Umuthi Team
```

## Gamma Presentation Automation

Since Gamma doesn't have an API, here are several automation approaches:

### Option 1: Browser Automation (Recommended)

#### Implementation with Playwright/Puppeteer

```javascript
// Function to create Gamma presentation
async function createGammaPresentation(reportData) {
  const browser = await playwright.chromium.launch();
  const context = await browser.newContext();
  const page = await context.newPage();
  
  try {
    // Login to Gamma
    await page.goto('https://gamma.app/signin');
    await page.fill('[data-testid="email"]', process.env.GAMMA_EMAIL);
    await page.fill('[data-testid="password"]', process.env.GAMMA_PASSWORD);
    await page.click('[data-testid="signin-button"]');
    
    // Wait for dashboard
    await page.waitForSelector('[data-testid="new-presentation"]');
    
    // Create new presentation
    await page.click('[data-testid="new-presentation"]');
    
    // Choose template or start from scratch
    await page.click('[data-testid="blank-template"]');
    
    // Add title slide
    await addTitleSlide(page, reportData);
    
    // Add content slides
    await addContentOpportunities(page, reportData.keywords);
    await addShareOfVoice(page, reportData.competitors);
    await addMarketInsights(page, reportData.insights);
    await addWebsiteHealth(page, reportData.audit);
    await addRecommendations(page, reportData.recommendations);
    
    // Get shareable link
    const presentationUrl = await page.url();
    await page.click('[data-testid="share-button"]');
    const shareUrl = await page.locator('[data-testid="share-link"]').textContent();
    
    return { presentationUrl: shareUrl };
    
  } finally {
    await browser.close();
  }
}
```

#### Detailed Slide Creation Functions

```javascript
async function addTitleSlide(page, reportData) {
  // Click title area
  await page.click('[data-testid="slide-title"]');
  await page.fill('[data-testid="slide-title"]', `RootScan Analysis: ${reportData.companyName}`);
  
  // Add subtitle
  await page.click('[data-testid="slide-subtitle"]');
  await page.fill('[data-testid="slide-subtitle"]', 
    `Comprehensive SEO & Digital Strategy Analysis\n${new Date().toLocaleDateString()}`);
}

async function addContentOpportunities(page, keywordData) {
  // Add new slide
  await page.click('[data-testid="add-slide"]');
  await page.click('[data-testid="content-slide-template"]');
  
  // Add title
  await page.fill('[data-testid="slide-title"]', 'Content Opportunities');
  
  // Add chart image
  await page.click('[data-testid="add-image"]');
  await page.setInputFiles('[data-testid="image-upload"]', keywordData.chartPath);
  
  // Add insight text
  await page.click('[data-testid="text-area"]');
  await page.fill('[data-testid="text-area"]', keywordData.insights);
}

async function addShareOfVoice(page, competitorData) {
  await page.click('[data-testid="add-slide"]');
  await page.fill('[data-testid="slide-title"]', 'Brand Share of Voice');
  
  // Add bubble chart
  await page.setInputFiles('[data-testid="image-upload"]', competitorData.bubbleChartPath);
  
  // Add competitive insights
  await page.fill('[data-testid="text-area"]', competitorData.analysis);
}
```

### Option 2: Alternative Tools with APIs

#### Google Slides API
```javascript
// Create presentation using Google Slides API
async function createGoogleSlidesPresentation(reportData) {
  const slides = google.slides({ version: 'v1', auth: oauth2Client });
  
  // Create presentation
  const presentation = await slides.presentations.create({
    requestBody: {
      title: `RootScan Analysis: ${reportData.companyName}`
    }
  });
  
  const presentationId = presentation.data.presentationId;
  
  // Add slides with content
  const requests = [
    {
      createSlide: {
        objectId: 'slide1',
        slideLayoutReference: { predefinedLayout: 'TITLE_AND_BODY' }
      }
    },
    {
      insertText: {
        objectId: 'slide1',
        text: reportData.insights,
        insertionIndex: 0
      }
    }
  ];
  
  await slides.presentations.batchUpdate({
    presentationId,
    requestBody: { requests }
  });
  
  return `https://docs.google.com/presentation/d/${presentationId}`;
}
```

#### Canva API Integration
```javascript
async function createCanvaPresentation(reportData) {
  const response = await fetch('https://api.canva.com/v1/designs', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${process.env.CANVA_API_KEY}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      design_type: 'presentation',
      name: `RootScan Analysis: ${reportData.companyName}`
    })
  });
  
  const design = await response.json();
  return design.urls.view_url;
}
```

### Option 3: Hybrid Approach - Generate + Import

#### Create PowerPoint then Import to Gamma

```javascript
// Generate PowerPoint using officegen
const generatePowerPoint = async (reportData) => {
  const pptx = officegen('pptx');
  
  // Title slide
  let slide = pptx.makeNewSlide();
  slide.addText(`RootScan Analysis: ${reportData.companyName}`, {
    x: 'c', y: 'c', font_size: 32, bold: true
  });
  
  // Content slides
  slide = pptx.makeNewSlide();
  slide.addText('Content Opportunities', { x: 10, y: 10, font_size: 24 });
  slide.addImage(reportData.keywords.chartPath, { x: 10, y: 50 });
  
  // Generate file
  const filename = `rootscan-${reportData.companyName}.pptx`;
  pptx.generate(fs.createWriteStream(filename));
  
  return filename;
};

// Then upload to Gamma via browser automation
const uploadToGamma = async (pptxFile) => {
  const page = await browser.newPage();
  await page.goto('https://gamma.app/import');
  await page.setInputFiles('[data-testid="file-upload"]', pptxFile);
  await page.waitForSelector('[data-testid="import-complete"]');
  return await page.url();
};
```

### Option 4: HTML-to-Presentation Conversion

#### Generate HTML Presentation then Convert

```javascript
// Create HTML presentation
const createHTMLPresentation = (reportData) => {
  const html = `
    <!DOCTYPE html>
    <html>
    <head>
      <title>RootScan Analysis: ${reportData.companyName}</title>
      <style>
        .slide { width: 100vw; height: 100vh; padding: 2rem; }
        .title { font-size: 3rem; text-align: center; }
        .chart { max-width: 80%; margin: 2rem auto; }
      </style>
    </head>
    <body>
      <div class="slide">
        <h1 class="title">RootScan Analysis</h1>
        <h2>${reportData.companyName}</h2>
      </div>
      
      <div class="slide">
        <h2>Content Opportunities</h2>
        <img src="${reportData.keywords.chartUrl}" class="chart">
        <p>${reportData.keywords.insights}</p>
      </div>
      
      <!-- More slides... -->
    </body>
    </html>
  `;
  
  return html;
};

// Convert to PDF then import to Gamma
const convertToPDF = async (html) => {
  const browser = await puppeteer.launch();
  const page = await browser.newPage();
  await page.setContent(html);
  const pdf = await page.pdf({ 
    format: 'A4', 
    landscape: true,
    printBackground: true 
  });
  await browser.close();
  return pdf;
};
```

## Recommended Azure Function Implementation

```javascript
// Azure Function for Gamma automation
module.exports = async function (context, req) {
  const reportData = req.body;
  
  try {
    // Choose automation method based on environment
    let presentationUrl;
    
    if (process.env.USE_GAMMA_AUTOMATION === 'true') {
      // Browser automation approach
      presentationUrl = await createGammaPresentation(reportData);
    } else {
      // Fallback to Google Slides
      presentationUrl = await createGoogleSlidesPresentation(reportData);
    }
    
    context.res = {
      status: 200,
      body: {
        presentationUrl,
        status: 'completed'
      }
    };
    
  } catch (error) {
    context.log.error('Presentation creation failed:', error);
    
    // Fallback: Create basic HTML report
    const htmlReport = createHTMLPresentation(reportData);
    const reportUrl = await uploadToStorage(htmlReport);
    
    context.res = {
      status: 200,
      body: {
        presentationUrl: reportUrl,
        status: 'completed_fallback',
        note: 'Created HTML report due to Gamma automation issue'
      }
    };
  }
};
```

## Make.com Configuration Tips

### Data Stores to Use:
1. **Process Tracking** - Store process IDs and status
2. **Rate Limiting** - Track API usage per hour
3. **Client History** - Store previous analyses

### Filters to Add:
- Check if client website is valid URL
- Verify email format
- Ensure required fields are present
- Rate limit protection (max 5 analyses per hour)

### Scheduling Options:
- **Immediate**: Run as soon as webhook received
- **Scheduled**: Queue for off-peak hours if processing is resource-intensive
- **Business Hours**: Only process during business hours for manual follow-up

## Backup Strategy

1. **Primary**: Gamma browser automation
2. **Secondary**: Google Slides API
3. **Tertiary**: Canva API
4. **Fallback**: Generated HTML/PDF report

## Advanced Features to Consider

1. **Real-time Progress Dashboard**
   - WebSocket updates during processing
   - Live preview of generated insights
   - Ability to pause/modify mid-process

2. **Custom Model Training**
   - Train on successful client outcomes
   - Industry-specific recommendation models
   - Continuously improving advice quality

3. **Multi-client Batch Processing**
   - Process multiple clients simultaneously
   - Comparative industry analysis
   - Bulk competitive intelligence

4. **Integration Hub**
   - WordPress plugin for direct integration
   - Zapier/Make.com connectors
   - CRM system integrations

5. **White-label API Service**
   - Allow other agencies to use your research engine
   - Custom branding and reporting
   - Revenue-generating API product

## Next Steps

1. **Phase 1**: Implement core API functions with manual Gamma creation
2. **Phase 2**: Add browser automation for Gamma presentations
3. **Phase 3**: Implement advanced features like real-time tracking
4. **Phase 4**: Scale to white-label offering

This comprehensive automation will transform the RootScan process from a 6+ hour manual task to a 20-30 minute automated pipeline, dramatically improving efficiency and consistency while maintaining high-quality outputs.
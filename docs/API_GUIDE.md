# Umuthi API Guide for Make.com

This guide explains how to integrate the Umuthi API with make.com for project initialization and SEO data retrieval.

## Prerequisites

- A make.com account
- Your Umuthi API deployed (or running locally for testing)
- Your API key (see [Authentication Guide](API_AUTHENTICATION.md))

## API Capabilities

### Project Initialization
- Initialize projects from Google Sheets and Fillout forms.
- Generate a correlation ID for tracking.

### SEO Data Retrieval
- SEO audit reports for domains.
- Keywords ranking data for SE Ranking projects.
- Competitor analysis and comparison.
- Long-running comprehensive reports with webhook notifications.

## Setting Up the Integration

### 1. Create a New Scenario in make.com

1. Log in to your make.com account
2. Click "Create a new scenario"
3. Choose a trigger (e.g., "Schedule", "Webhook", or any other trigger that matches your use case)

### 2. Add an HTTP Module

1. Click the "+" button to add a new module
2. Search for and select "HTTP"
3. Choose the appropriate HTTP method (POST for creating resources, GET for retrieving data)

### 3. Configure the HTTP Module

For detailed information on each endpoint, please refer to the specific guides:

- [Project Initialization API](PROJECT_INIT_API.md)
- [SEO API Guide](SEO_API_GUIDE.md)
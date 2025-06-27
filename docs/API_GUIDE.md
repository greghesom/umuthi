# Umuthi API Guide for Make.com

This guide explains how to integrate the Umuthi API with make.com for project initialization and RootScan data retrieval and processing.

## Prerequisites

- A make.com account
- Your Umuthi API deployed (or running locally for testing)
- Your API key (see [Authentication Guide](API_AUTHENTICATION.md))

## API Capabilities

### Project Initialization
- Initialize projects from Google Sheets and Fillout forms.
- Generate a correlation ID for tracking.

### RootScan Process
- **Orchestration handled by Make.com**: Each step of the RootScan process is exposed as a separate API endpoint, allowing Make.com to control the workflow and data flow.
- **Data Minimization**: Each API endpoint receives only the data it requires for its specific task, along with a `correlationId` for tracking the overall process.

#### RootScan Steps:
1.  **Initialize Project**: (via `Create Project` module) Initiates the RootScan process and provides a `correlationId`.
2.  **Get Keyword Intelligence**: Retrieves keyword research data.
3.  **Get Competitive Intelligence**: Retrieves competitive analysis data.
4.  **Get Market Intelligence**: Retrieves market intelligence data.
5.  **Get Technical Audit**: Retrieves technical SEO audit data.
6.  **Generate Report**: Generates the final comprehensive RootScan report and presentation.

## Setting Up the Integration

### 1. Create a New Scenario in make.com

1. Log in to your make.com account
2. Click "Create a new scenario"
3. Choose a trigger (e.g., "Schedule", "Webhook", or any other trigger that matches your use case)

### 2. Add an HTTP Module for each RootScan Step

For each step of your RootScan workflow, you will add an HTTP module in Make.com and configure it to call the corresponding Umuthi API endpoint. Ensure you pass the `correlationId` and any necessary data from previous steps.

For detailed information on each endpoint, please refer to the specific guides:

- [Project Initialization API](PROJECT_INIT_API.md)
- [SEO API Guide](SEO_API_GUIDE.md)


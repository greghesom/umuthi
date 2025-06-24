# Project Initialization API Documentation

## Overview

The Project Initialization API endpoint allows Make.com integration users to initialize a new project with customer data and receive a correlation ID for tracking all subsequent operations and billing.

## Endpoint

### POST /api/project/init

Initialize a new project with customer data and receive a correlation ID.

#### Authentication

This endpoint requires API key authentication:
- **Header**: `x-api-key: YOUR_API_KEY`
- **Default API Key**: `umuthi-dev-api-key` (for development)
- **Make.com API Key**: `make-integration-key`

#### Request Body

```json
{
  "email": "customer@example.com",
  "googleSheetRowId": "ROW123",
  "filloutData": "{\"formId\":\"abc123\",\"submissionId\":\"xyz789\"}",
  "makeCustomerId": "MAKE456"
}
```

#### Request Fields

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|------------|
| `email` | string | Yes | Customer email address | Must be valid email format |
| `googleSheetRowId` | string | Yes | Google Sheet row identifier | Must be alphanumeric characters only |
| `filloutData` | string | Yes | Fillout form data as JSON string | Must be valid JSON format |
| `makeCustomerId` | string | Yes | Make.com customer identifier | Required, non-empty string |

#### Response Codes

| Code | Description |
|------|-------------|
| 200 | Project initialized successfully |
| 400 | Validation error (invalid request data) |
| 401 | Authentication failed (invalid or missing API key) |
| 409 | Duplicate project (same email + googleSheetRowId combination already exists) |
| 500 | Internal server error |

#### Success Response (200)

```json
{
  "success": true,
  "message": "Project initialized successfully",
  "correlationId": "PROJ8A2B",
  "createdAt": "2025-01-01T10:30:00Z"
}
```

#### Error Response (400)

```json
{
  "success": false,
  "message": "Validation failed: Email is required, Invalid email format",
  "correlationId": "",
  "createdAt": "2025-01-01T10:30:00Z"
}
```

#### Duplicate Error Response (409)

```json
{
  "success": false,
  "message": "A project with the same email and Google Sheet row ID already exists.",
  "correlationId": "",
  "createdAt": "2025-01-01T10:30:00Z"
}
```

#### Server Error Response (500)

```json
{
  "success": false,
  "message": "An unexpected error occurred",
  "correlationId": "",
  "createdAt": "2025-01-01T10:30:00Z"
}
```

## Features

### Correlation ID Generation

- Format: `PROJ` + 4 random alphanumeric characters (e.g., `PROJ8A2B`)
- Guaranteed uniqueness within the system
- Used for tracking all subsequent operations
- Supports billing and usage analytics

### Duplicate Detection

- Prevents duplicate projects based on email + googleSheetRowId combination
- Returns 409 Conflict status for duplicates
- Helps maintain data integrity

### Usage Tracking

- All API calls are tracked for billing purposes
- Operation type: `PROJECT_INIT`
- Includes request size, processing time, and success/failure status
- Supports Make.com customer identification headers:
  - `X-Customer-ID`
  - `X-Team-ID`
  - `X-Organization-Name`

### Validation

- Email format validation using standard email validation
- JSON format validation for filloutData field
- Alphanumeric validation for googleSheetRowId
- Required field validation for all inputs

## Example Usage

### cURL Example

```bash
curl -X POST "https://your-function-app.azurewebsites.net/api/project/init" \
  -H "Content-Type: application/json" \
  -H "x-api-key: umuthi-dev-api-key" \
  -d '{
    "email": "customer@example.com",
    "googleSheetRowId": "ROW123",
    "filloutData": "{\"formId\":\"abc123\",\"submissionId\":\"xyz789\"}",
    "makeCustomerId": "MAKE456"
  }'
```

### Make.com HTTP Module Configuration

1. **Method**: POST
2. **URL**: `https://your-function-app.azurewebsites.net/api/project/init`
3. **Headers**:
   - `x-api-key`: `make-integration-key`
   - `Content-Type`: `application/json`
4. **Body Type**: Raw
5. **Body**: JSON object with required fields

## Database Schema

The endpoint creates records in the `ProjectInitializations` table with the following structure:

| Column | Type | Description |
|--------|------|-------------|
| `Id` | uniqueidentifier | Primary key |
| `CorrelationId` | nvarchar(8) | Unique correlation ID (indexed) |
| `CustomerEmail` | nvarchar(255) | Customer email (indexed) |
| `GoogleSheetRowId` | nvarchar(100) | Google Sheet row ID |
| `FilloutData` | nvarchar(max) | JSON data from Fillout |
| `MakeCustomerId` | nvarchar(100) | Make.com customer ID (indexed) |
| `CreatedAt` | datetime2 | Record creation timestamp |
| `UpdatedAt` | datetime2 | Record update timestamp |
| `CreatedBy` | nvarchar(max) | Optional creator information |
| `UpdatedBy` | nvarchar(max) | Optional updater information |

### Indexes

- Unique index on `CorrelationId`
- Unique composite index on `CustomerEmail` + `GoogleSheetRowId`
- Standard indexes on `CustomerEmail`, `MakeCustomerId`, and `CreatedAt`

## Error Handling

The endpoint implements comprehensive error handling:

- **Validation Errors**: Returns detailed validation messages
- **Duplicate Detection**: Specific error message for duplicate projects
- **JSON Validation**: Validates FilloutData as proper JSON
- **Exception Handling**: Graceful handling of unexpected errors
- **Usage Tracking**: All operations (successful and failed) are tracked

## Testing

Comprehensive test coverage includes:

- Unit tests for service layer logic
- Function-level integration tests
- Validation scenarios
- Error handling scenarios
- Duplicate detection tests
- Correlation ID generation tests

HTTP test cases are available in `tests/umuthi.Functions.Tests/FunctionsTests.http` for manual testing.
# API Authentication Guide

This guide explains how to authenticate with the Umuthi API.

## API Key Authentication

All endpoints in the Umuthi API are secured with API key authentication. You need to provide a valid API key to access these endpoints.

### API Key

For development and testing, the default API key is:

```
umuthi-dev-api-key
```

For make.com integration, a dedicated API key is available:

```
make-integration-key
```

In production, you should set a strong, unique API key in your Azure Function's application settings by configuring the `ApiKey` environment variable.

### Authentication Methods

There are two ways to provide your API key:

1. **HTTP Header (Recommended)**
   - Header Name: `x-api-key`
   - Header Value: Your API key

2. **Query Parameter**
   - Parameter Name: `code`
   - Parameter Value: Your API key

## Troubleshooting

If you receive a 401 Unauthorized response, check that you've provided the correct API key and that it is formatted correctly in the header or query parameter.
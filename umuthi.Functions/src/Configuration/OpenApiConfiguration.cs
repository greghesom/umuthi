using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace umuthi.Functions.Configuration;

/// <summary>
/// OpenAPI configuration for the Umuthi Audio Processing API
/// </summary>
public static class OpenApiConfiguration
{
    /// <summary>
    /// Gets the OpenAPI information for the API
    /// </summary>
    public static OpenApiInfo GetOpenApiInfo()
    {
        return new OpenApiInfo
        {
            Version = "v1.0.0",
            Title = "Umuthi Audio Processing API",
            Description = @"
**Umuthi Audio Processing API** provides powerful audio conversion and speech transcription services with comprehensive usage tracking and billing.

## Features
- **Audio Conversion**: Convert between various audio formats (WAV, MPEG, MP4, M4A, AAC → MP3)
- **Speech Transcription**: Convert audio files to text using Azure Cognitive Services
- **Fast Transcription**: High-speed batch transcription using Azure Fast Transcription API (single file, up to 1GB)
- **Usage Tracking**: Comprehensive billing and analytics with Make.com integration
- **Secure Authentication**: API key-based authentication with rate limiting

## Authentication
All endpoints (except `/api/GetSupportedFormats` and `/api/HealthCheck`) require API key authentication:
- **Header**: `x-api-key: YOUR_API_KEY`
- **Query Parameter**: `?code=YOUR_API_KEY`

## Make.com Integration
The API supports Make.com workflow headers for customer tracking:
- `X-Customer-ID`: Customer identifier
- `X-Team-ID`: Team identifier  
- `X-Organization-Name`: Organization name

## Rate Limits
- Default: 100 requests per minute per API key
- File size limits: 50MB for conversion, 100MB for transcription, 1GB for fast transcription

## Usage Tracking
All API calls are tracked for billing purposes including:
- File sizes processed
- Processing duration
- Success/failure rates
- Customer attribution via Make.com headers
",
            Contact = new OpenApiContact
            {
                Name = "Umuthi Support",
                Email = "support@umuthi.com"
            },
            License = new OpenApiLicense
            {
                Name = "Commercial License"
            }
        };
    }

    /// <summary>
    /// Gets the security schemes for API key authentication
    /// </summary>
    public static Dictionary<string, OpenApiSecurityScheme> GetSecuritySchemes()
    {
        return new Dictionary<string, OpenApiSecurityScheme>
        {
            {
                "ApiKeyAuth",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "x-api-key",
                    Description = "API Key for authentication. Contact support to obtain your API key."
                }
            },
            {
                "ApiKeyQuery",
                new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Query,
                    Name = "code",
                    Description = "API Key as query parameter (alternative to header)"
                }
            }
        };
    }

    /// <summary>
    /// Gets the default security requirements
    /// </summary>
    public static List<OpenApiSecurityRequirement> GetSecurityRequirements()
    {
        return new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKeyAuth"
                        }
                    },
                    new string[] { }
                }
            }
        };
    }

    /// <summary>
    /// Gets the API tags for organizing endpoints
    /// </summary>
    public static List<OpenApiTag> GetTags()
    {
        return new List<OpenApiTag>
        {
            new OpenApiTag
            {
                Name = "Audio Conversion",
                Description = "Convert audio files between different formats"
            },
            new OpenApiTag
            {
                Name = "Speech Transcription", 
                Description = "Convert speech in audio files to text"
            },
            new OpenApiTag
            {
                Name = "Usage Analytics",
                Description = "Billing and usage analytics (requires authentication)"
            },
            new OpenApiTag
            {
                Name = "API Information",
                Description = "Public information about API capabilities"
            },
            new OpenApiTag
            {
                Name = "Health",
                Description = "API health and status checks"
            }
        };
    }
}

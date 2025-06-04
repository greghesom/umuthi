using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace umuthi.Functions.Functions;

/// <summary>
/// Azure Functions for OpenAPI/Swagger documentation
/// </summary>
public class OpenApiFunctions
{
    private readonly ILogger<OpenApiFunctions> _logger;

    public OpenApiFunctions(ILogger<OpenApiFunctions> logger)
    {
        _logger = logger;
    }    [Function("SwaggerUI")]
    public IActionResult GetSwaggerUI([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger")] HttpRequest req)
    {
        _logger.LogInformation("Swagger UI requested.");

        var baseUrl = GetBaseUrl(req);
        var swaggerHtml = GenerateSwaggerHtml(baseUrl);

        return new ContentResult
        {
            Content = swaggerHtml,
            ContentType = "text/html"
        };
    }    [Function("OpenApiDocument")]
    public IActionResult GetOpenApiDocument([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "openapi.json")] HttpRequest req)
    {
        _logger.LogInformation("OpenAPI document requested.");

        var baseUrl = GetBaseUrl(req);
        var openApiSpec = GenerateOpenApiSpec(baseUrl);

        return new ContentResult
        {
            Content = openApiSpec,
            ContentType = "application/json"
        };
    }

    private string GetBaseUrl(HttpRequest req)
    {
        var scheme = req.Scheme;
        var host = req.Host.Value;
        return $"{scheme}://{host}";
    }

    private string GenerateOpenApiSpec(string baseUrl)
    {
        return $$"""
        {
          "openapi": "3.0.3",
          "info": {
            "title": "Umuthi Audio Processing API",
            "description": "**Umuthi Audio Processing API** provides powerful audio conversion and speech transcription services with comprehensive usage tracking and billing.\n\n## Features\n- **Audio Conversion**: Convert between various audio formats (WAV, MPEG, MP4, M4A, AAC → MP3)\n- **Speech Transcription**: Convert audio files to text using Azure Cognitive Services\n- **Usage Tracking**: Comprehensive billing and analytics with Make.com integration\n- **Secure Authentication**: API key-based authentication with rate limiting\n\n## Authentication\nAll endpoints (except `/api/GetSupportedFormats` and `/api/HealthCheck`) require API key authentication:\n- **Header**: `x-api-key: YOUR_API_KEY`\n- **Query Parameter**: `?code=YOUR_API_KEY`\n\n## Make.com Integration\nThe API supports Make.com workflow headers for customer tracking:\n- `X-Customer-ID`: Customer identifier\n- `X-Team-ID`: Team identifier\n- `X-Organization-Name`: Organization name\n\n## Rate Limits\n- Default: 100 requests per minute per API key\n- File size limits: 50MB for conversion, 100MB for transcription",
            "version": "1.0.0",
            "contact": {
              "name": "Umuthi Support",
              "email": "support@umuthi.com"
            }
          },
          "servers": [
            {
              "url": "{{baseUrl}}",
              "description": "Umuthi Audio Processing API"
            }
          ],
          "components": {
            "securitySchemes": {
              "ApiKeyAuth": {
                "type": "apiKey",
                "in": "header",
                "name": "x-api-key",
                "description": "API Key for authentication. Contact support to obtain your API key."
              },
              "ApiKeyQuery": {
                "type": "apiKey",
                "in": "query",
                "name": "code",
                "description": "API Key as query parameter (alternative to header)"
              }
            },
            "schemas": {
              "AudioConversionResponse": {
                "type": "object",
                "properties": {
                  "success": {
                    "type": "boolean",
                    "description": "Indicates if the conversion was successful"
                  },
                  "message": {
                    "type": "string",
                    "description": "Success or error message"
                  },
                  "originalFileName": {
                    "type": "string",
                    "description": "Original file name"
                  },
                  "convertedFileName": {
                    "type": "string",
                    "description": "Converted file name"
                  },
                  "originalFileSizeBytes": {
                    "type": "integer",
                    "format": "int64",
                    "description": "Original file size in bytes"
                  },
                  "convertedFileSizeBytes": {
                    "type": "integer",
                    "format": "int64",
                    "description": "Converted file size in bytes"
                  },
                  "processingDurationMs": {
                    "type": "integer",
                    "format": "int64",
                    "description": "Processing duration in milliseconds"
                  },
                  "audioDurationSeconds": {
                    "type": "number",
                    "format": "double",
                    "description": "Audio duration in seconds"
                  },
                  "compressionRatio": {
                    "type": "number",
                    "format": "double",
                    "description": "Compression ratio achieved"
                  }
                }
              },
              "TranscriptionResponse": {
                "type": "object",
                "properties": {
                  "success": {
                    "type": "boolean",
                    "description": "Indicates if the transcription was successful"
                  },
                  "message": {
                    "type": "string",
                    "description": "Success or error message"
                  },
                  "originalFileName": {
                    "type": "string",
                    "description": "Original audio file name"
                  },
                  "transcriptText": {
                    "type": "string",
                    "description": "Transcribed text content"
                  },
                  "language": {
                    "type": "string",
                    "description": "Language detected or specified"
                  },
                  "confidence": {
                    "type": "number",
                    "format": "double",
                    "description": "Confidence score (0.0 to 1.0)"
                  },
                  "audioDurationSeconds": {
                    "type": "number",
                    "format": "double",
                    "description": "Audio duration in seconds"
                  },
                  "processingDurationMs": {
                    "type": "integer",
                    "format": "int64",
                    "description": "Processing duration in milliseconds"
                  },
                  "wordCount": {
                    "type": "integer",
                    "description": "Number of words transcribed"
                  }
                }
              },
              "ErrorResponse": {
                "type": "object",
                "properties": {
                  "code": {
                    "type": "string",
                    "description": "Error code"
                  },
                  "message": {
                    "type": "string",
                    "description": "Error message"
                  },
                  "details": {
                    "type": "string",
                    "description": "Additional error details"
                  },
                  "timestamp": {
                    "type": "string",
                    "format": "date-time",
                    "description": "Timestamp of the error"
                  }
                }
              },
              "SupportedFormatsResponse": {
                "type": "object",
                "properties": {
                  "input": {
                    "type": "array",
                    "items": {
                      "type": "string"
                    },
                    "description": "Supported input audio formats"
                  },
                  "output": {
                    "type": "array",
                    "items": {
                      "type": "string"
                    },
                    "description": "Supported output formats"
                  },
                  "maxFileSize": {
                    "type": "string",
                    "description": "Maximum file size limitations"
                  },
                  "functions": {
                    "type": "array",
                    "items": {
                      "type": "string"
                    },
                    "description": "Available API endpoints"
                  },
                  "version": {
                    "type": "string",
                    "description": "API version"
                  },
                  "description": {
                    "type": "string",
                    "description": "Additional information about the API"
                  }
                }
              }
            }
          },
          "paths": {
            "/api/GetSupportedFormats": {
              "get": {
                "tags": ["API Information"],
                "summary": "Get supported audio formats",
                "description": "Returns information about supported input/output formats, file size limits, and available API endpoints.",
                "operationId": "GetSupportedFormats",
                "responses": {
                  "200": {
                    "description": "Supported formats information",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/SupportedFormatsResponse"
                        }
                      }
                    }
                  }
                }
              }
            },
            "/api/HealthCheck": {
              "get": {
                "tags": ["Health"],
                "summary": "API health check",
                "description": "Returns the health status of the API service.",
                "operationId": "HealthCheck",
                "responses": {
                  "200": {
                    "description": "Health status",
                    "content": {
                      "application/json": {
                        "schema": {
                          "type": "object",
                          "properties": {
                            "status": {
                              "type": "string"
                            },
                            "timestamp": {
                              "type": "string",
                              "format": "date-time"
                            },
                            "version": {
                              "type": "string"
                            },
                            "services": {
                              "type": "object"
                            }
                          }
                        }
                      }
                    }
                  }
                }
              }
            },
            "/api/ConvertWavToMp3": {
              "post": {
                "tags": ["Audio Conversion"],
                "summary": "Convert WAV audio to MP3",
                "description": "Converts uploaded WAV audio files to MP3 format with configurable quality settings.",
                "operationId": "ConvertWavToMp3",
                "security": [
                  {
                    "ApiKeyAuth": []
                  }
                ],
                "requestBody": {
                  "required": true,
                  "description": "WAV audio file to convert",
                  "content": {
                    "multipart/form-data": {
                      "schema": {
                        "type": "object",
                        "properties": {
                          "file": {
                            "type": "string",
                            "format": "binary",
                            "description": "WAV audio file (max 50MB)"
                          }
                        },
                        "required": ["file"]
                      }
                    }
                  }
                },
                "responses": {
                  "200": {
                    "description": "MP3 audio file",
                    "content": {
                      "application/octet-stream": {
                        "schema": {
                          "type": "string",
                          "format": "binary"
                        }
                      }
                    }
                  },
                  "400": {
                    "description": "Bad Request - Invalid file format or missing file",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  },
                  "401": {
                    "description": "Unauthorized - Invalid or missing API key",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  },
                  "413": {
                    "description": "File Too Large - File exceeds 50MB limit",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  },
                  "500": {
                    "description": "Internal Server Error - Conversion processing failed",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  }
                }
              }
            },
            "/api/ConvertMpegToMp3": {
              "post": {
                "tags": ["Audio Conversion"],
                "summary": "Convert MPEG audio to MP3",
                "description": "Converts uploaded MPEG/MPG/MP4/M4A/AAC audio files to MP3 format.",
                "operationId": "ConvertMpegToMp3",
                "security": [
                  {
                    "ApiKeyAuth": []
                  }
                ],
                "requestBody": {
                  "required": true,
                  "description": "MPEG audio file to convert",
                  "content": {
                    "multipart/form-data": {
                      "schema": {
                        "type": "object",
                        "properties": {
                          "file": {
                            "type": "string",
                            "format": "binary",
                            "description": "MPEG/MPG/MP4/M4A/AAC audio file (max 50MB)"
                          }
                        },
                        "required": ["file"]
                      }
                    }
                  }
                },
                "responses": {
                  "200": {
                    "description": "MP3 audio file",
                    "content": {
                      "application/octet-stream": {
                        "schema": {
                          "type": "string",
                          "format": "binary"
                        }
                      }
                    }
                  },
                  "400": {
                    "description": "Bad Request - Invalid file format or missing file",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  },
                  "401": {
                    "description": "Unauthorized - Invalid or missing API key",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  },
                  "413": {
                    "description": "File Too Large - File exceeds 50MB limit",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  },
                  "500": {
                    "description": "Internal Server Error - Conversion processing failed",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  }
                }
              }
            },
            "/api/ConvertAudioToTranscript": {
              "post": {
                "tags": ["Speech Transcription"],
                "summary": "Convert audio to text transcript",
                "description": "Transcribes speech in audio files to text using Azure Cognitive Services. Supports various audio formats and languages.",
                "operationId": "ConvertAudioToTranscript",
                "security": [
                  {
                    "ApiKeyAuth": []
                  }
                ],
                "requestBody": {
                  "required": true,
                  "description": "Audio file to transcribe",
                  "content": {
                    "multipart/form-data": {
                      "schema": {
                        "type": "object",
                        "properties": {
                          "file": {
                            "type": "string",
                            "format": "binary",
                            "description": "Audio file (WAV, MP3, MP4, M4A, AAC, etc.) - max 100MB"
                          },
                          "language": {
                            "type": "string",
                            "description": "Optional language code (e.g., 'en-US', 'es-ES')",
                            "example": "en-US"
                          }
                        },
                        "required": ["file"]
                      }
                    }
                  }
                },
                "responses": {
                  "200": {
                    "description": "Transcription successful",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/TranscriptionResponse"
                        }
                      }
                    }
                  },
                  "400": {
                    "description": "Bad Request - Invalid file format or missing file",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  },
                  "401": {
                    "description": "Unauthorized - Invalid or missing API key",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  },
                  "413": {
                    "description": "File Too Large - File exceeds 100MB limit",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  },
                  "500": {
                    "description": "Internal Server Error - Transcription processing failed",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  }
                }
              }
            },
            "/api/GetBillingSummary": {
              "get": {
                "tags": ["Usage Analytics"],
                "summary": "Get billing summary",
                "description": "Returns billing summary for a customer or organization within a date range.",
                "operationId": "GetBillingSummary",
                "security": [
                  {
                    "ApiKeyAuth": []
                  }
                ],
                "parameters": [
                  {
                    "name": "customerId",
                    "in": "query",
                    "description": "Customer ID (optional if organizationName is provided)",
                    "schema": {
                      "type": "string"
                    }
                  },
                  {
                    "name": "organizationName",
                    "in": "query",
                    "description": "Organization name (optional if customerId is provided)",
                    "schema": {
                      "type": "string"
                    }
                  },
                  {
                    "name": "startDate",
                    "in": "query",
                    "required": true,
                    "description": "Start date (ISO 8601 format)",
                    "schema": {
                      "type": "string",
                      "format": "date-time"
                    }
                  },
                  {
                    "name": "endDate",
                    "in": "query",
                    "required": true,
                    "description": "End date (ISO 8601 format)",
                    "schema": {
                      "type": "string",
                      "format": "date-time"
                    }
                  }
                ],
                "responses": {
                  "200": {
                    "description": "Billing summary",
                    "content": {
                      "application/json": {
                        "schema": {
                          "type": "object",
                          "description": "Billing summary information"
                        }
                      }
                    }
                  },
                  "401": {
                    "description": "Unauthorized",
                    "content": {
                      "application/json": {
                        "schema": {
                          "$ref": "#/components/schemas/ErrorResponse"
                        }
                      }
                    }
                  }
                }
              }
            }
          },
          "tags": [
            {
              "name": "Audio Conversion",
              "description": "Convert audio files between different formats"
            },
            {
              "name": "Speech Transcription",
              "description": "Convert speech in audio files to text"
            },
            {
              "name": "Usage Analytics",
              "description": "Billing and usage analytics (requires authentication)"
            },
            {
              "name": "API Information",
              "description": "Public information about API capabilities"
            },
            {
              "name": "Health",
              "description": "API health and status checks"
            }
          ]
        }
        """;
    }

    private string GenerateSwaggerHtml(string baseUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Umuthi Audio Processing API - Swagger UI</title>
    <link rel=""stylesheet"" type=""text/css"" href=""https://unpkg.com/swagger-ui-dist@5.0.0/swagger-ui.css"" />
    <style>
        html {{
            box-sizing: border-box;
            overflow: -moz-scrollbars-vertical;
            overflow-y: scroll;
        }}

        *, *:before, *:after {{
            box-sizing: inherit;
        }}

        body {{
            margin:0;
            background: #fafafa;
        }}

        .swagger-ui .topbar {{
            background-color: #1b1b1b;
        }}

        .swagger-ui .topbar .link {{
            color: #ffffff;
        }}
    </style>
</head>
<body>
    <div id=""swagger-ui""></div>
    <script src=""https://unpkg.com/swagger-ui-dist@5.0.0/swagger-ui-bundle.js""></script>
    <script src=""https://unpkg.com/swagger-ui-dist@5.0.0/swagger-ui-standalone-preset.js""></script>
    <script>
        window.onload = function() {{
            const ui = SwaggerUIBundle({{
                url: '{baseUrl}/api/openapi.json',
                dom_id: '#swagger-ui',
                deepLinking: true,
                presets: [
                    SwaggerUIBundle.presets.apis,
                    SwaggerUIStandalonePreset
                ],
                plugins: [
                    SwaggerUIBundle.plugins.DownloadUrl
                ],
                layout: ""StandaloneLayout"",
                tryItOutEnabled: true,
                supportedSubmitMethods: ['get', 'post', 'put', 'delete', 'patch'],
                onComplete: function() {{
                    console.log('Swagger UI loaded successfully');
                }},
                onFailure: function(err) {{
                    console.error('Failed to load Swagger UI:', err);
                }}
            }});

            window.ui = ui;
        }};
    </script>
</body>
</html>";
    }
}

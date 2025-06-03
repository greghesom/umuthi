using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace umuthi.Functions.Middleware
{
    /// <summary>
    /// Static helper class to validate API keys for function endpoints
    /// </summary>
    public static class ApiKeyValidator
    {
        private const string ApiKeyHeaderName = "x-api-key";
        private const string ApiKeyQueryParamName = "code";

        /// <summary>
        /// Validates the API key from either request headers or query parameters
        /// </summary>
        /// <param name="req">The HTTP request</param>
        /// <param name="logger">ILogger instance for logging</param>
        /// <returns>True if valid API key was provided, otherwise false</returns>
        public static bool ValidateApiKey(HttpRequest req, ILogger logger)
        {
            try
            {
                string? providedApiKey = null;

                // Try to get API key from header
                if (req.Headers.TryGetValue(ApiKeyHeaderName, out var headerValues))
                {
                    providedApiKey = headerValues.FirstOrDefault();
                }

                // If not in header, try query string
                if (string.IsNullOrEmpty(providedApiKey) && 
                    req.Query.TryGetValue(ApiKeyQueryParamName, out var queryValues))
                {
                    providedApiKey = queryValues.ToString();
                }

                // Validate API key
                if (!string.IsNullOrEmpty(providedApiKey) && IsValidApiKey(providedApiKey))
                {
                    logger.LogInformation("API key authentication successful");
                    
                    // Optional: Apply rate limiting
                    // if (!RateLimitCheck(providedApiKey)) return false;
                    
                    return true;
                }

                logger.LogWarning("Invalid API key provided");
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during API key validation");
                return false;
            }
        }

        private static bool IsValidApiKey(string providedApiKey)
        {
            // Get configured API keys from environment variables
            var configuredApiKey = Environment.GetEnvironmentVariable("ApiKey");
            var additionalApiKeys = Environment.GetEnvironmentVariable("AdditionalApiKeys");
            
            if (string.IsNullOrEmpty(configuredApiKey))
            {
                // If no API key is configured, use a default one for development
                configuredApiKey = "umuthi-dev-api-key";
            }

            // Check if the provided API key matches the primary key
            if (string.Equals(providedApiKey, configuredApiKey, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check additional API keys if configured
            if (!string.IsNullOrEmpty(additionalApiKeys))
            {
                var keys = additionalApiKeys.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var key in keys)
                {
                    if (string.Equals(providedApiKey, key.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
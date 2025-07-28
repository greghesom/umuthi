using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Models;
using umuthi.Functions.Middleware;

namespace umuthi.Functions.Functions.SEO;

/// <summary>
/// Azure Function for keyword research with SE Ranking integration
/// </summary>
public class KeywordResearchFunction
{
    private readonly ILogger<KeywordResearchFunction> _logger;
    private readonly ISEORankingService _seoRankingService;
    private readonly IUsageTrackingService _usageTrackingService;

    public KeywordResearchFunction(
        ILogger<KeywordResearchFunction> logger,
        ISEORankingService seoRankingService,
        IUsageTrackingService usageTrackingService)
    {
        _logger = logger;
        _seoRankingService = seoRankingService;
        _usageTrackingService = usageTrackingService;
    }

    /// <summary>
    /// Get comprehensive keyword research data
    /// </summary>
    /// <param name="req">HTTP request containing keyword research parameters</param>
    /// <returns>Comprehensive keyword research data with metrics</returns>
    [Function("KeywordResearch")]
    [ApiKeyAuthentication]
    public async Task<IActionResult> GetKeywordResearch([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "keywords/research")] HttpRequest req)
    {
        var startTime = DateTime.UtcNow;
        long inputSize = 0;
        long outputSize = 0;

        try
        {
            // Validate API key
            if (!ApiKeyValidator.ValidateApiKey(req, _logger))
            {
                return new UnauthorizedResult();
            }

            _logger.LogInformation("Keyword research function triggered.");

            // Parse request body
            string requestBody;
            using (var reader = new StreamReader(req.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult(new { error = "Request body is required." });
            }

            inputSize = requestBody.Length;

            KeywordResearchRequestWithCountryCode? request;
            try
            {
                request = JsonSerializer.Deserialize<KeywordResearchRequestWithCountryCode>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Invalid JSON in request body");
                return new BadRequestObjectResult(new { error = "Invalid JSON in request body." });
            }

            if (request == null)
            {
                return new BadRequestObjectResult(new { error = "Invalid request format." });
            }

            // Validate required fields
            var validationResults = ValidateRequest(request);
            if (validationResults.Any())
            {
                return new BadRequestObjectResult(new { 
                    error = "Validation failed.", 
                    details = validationResults 
                });
            }

            // Clean HTML tags from keywords before processing
            var cleanedKeywords = CleanHtmlTags(request.Keywords);

            // Parse keywords from multi-line string
            var keywords = cleanedKeywords
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();

            if (keywords.Count == 0)
            {
                return new BadRequestObjectResult(new { error = "At least one keyword is required." });
            }

            // Note: Removed the 100 keyword limit as per issue requirements

            // Get the effective region code (supports both regionCode and countrycode)
            var effectiveRegionCode = request.GetEffectiveRegionCode();

            // Validate region code (Alpha-2 format)
            if (!IsValidAlpha2CountryCode(effectiveRegionCode))
            {
                return new BadRequestObjectResult(new { error = "Invalid region code. Must be a valid Alpha-2 country code (e.g., US, ZA, GB)." });
            }

            // Get keyword research data
            var keywordResearch = await _seoRankingService.GetKeywordResearchAsync(
                keywords, 
                effectiveRegionCode, 
                request.IncludeHistoricalTrends, 
                _logger);

            // Apply sorting if requested
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                keywordResearch.Keywords = ApplySorting(keywordResearch.Keywords, request.SortBy, request.SortDirection);
            }

            // Apply filters if requested
            if (request.MinSearchVolume.HasValue)
            {
                keywordResearch.Keywords = keywordResearch.Keywords
                    .Where(k => k.SearchVolume >= request.MinSearchVolume.Value)
                    .ToList();
            }

            if (request.MaxDifficulty.HasValue)
            {
                keywordResearch.Keywords = keywordResearch.Keywords
                    .Where(k => k.Difficulty <= request.MaxDifficulty.Value)
                    .ToList();
            }

            var jsonResponse = JsonSerializer.Serialize(keywordResearch, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            outputSize = jsonResponse.Length;

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Track this API call
            var metadata = new UsageMetadata();
            metadata.SetProcessingRegion(Environment.GetEnvironmentVariable("WEBSITE_RESOURCE_GROUP") ?? "unknown");
            metadata.SetInputFormat("json");
            metadata.SetOutputFormat("json");

            await _usageTrackingService.TrackUsageAsync(
                req,
                "KeywordResearch",
                OperationTypes.KeywordResearch,
                inputSize,
                outputSize,
                (long)duration,
                200,
                true,
                null,
                metadata);

            return new OkObjectResult(keywordResearch);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error occurred while processing keyword research request");

            // Track failed API call
            await _usageTrackingService.TrackUsageAsync(
                req,
                "KeywordResearch",
                OperationTypes.KeywordResearch,
                inputSize,
                outputSize,
                (long)duration,
                500,
                false,
                ex.Message);

            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Validate the keyword research request
    /// </summary>
    private static string[] ValidateRequest(KeywordResearchRequestWithCountryCode request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Keywords))
        {
            errors.Add("Keywords field is required.");
        }

        var effectiveRegionCode = request.GetEffectiveRegionCode();
        if (string.IsNullOrWhiteSpace(effectiveRegionCode))
        {
            errors.Add("RegionCode or CountryCode field is required.");
        }

        if (request.MinSearchVolume.HasValue && request.MinSearchVolume.Value < 0)
        {
            errors.Add("MinSearchVolume must be a positive number.");
        }

        if (request.MaxDifficulty.HasValue && (request.MaxDifficulty.Value < 0 || request.MaxDifficulty.Value > 100))
        {
            errors.Add("MaxDifficulty must be between 0 and 100.");
        }

        if (!string.IsNullOrEmpty(request.SortBy))
        {
            var validSortFields = new[] { "volume", "difficulty", "cpc", "competition", "keyword" };
            if (!validSortFields.Contains(request.SortBy.ToLower()))
            {
                errors.Add($"Invalid SortBy field. Valid options: {string.Join(", ", validSortFields)}");
            }
        }

        if (!string.IsNullOrEmpty(request.SortDirection))
        {
            var validDirections = new[] { "asc", "desc" };
            if (!validDirections.Contains(request.SortDirection.ToLower()))
            {
                errors.Add("Invalid SortDirection. Valid options: asc, desc");
            }
        }

        return errors.ToArray();
    }

    /// <summary>
    /// Apply sorting to keyword research results
    /// </summary>
    private static List<KeywordResearchData> ApplySorting(List<KeywordResearchData> keywords, string sortBy, string? sortDirection)
    {
        var ascending = sortDirection?.ToLower() != "desc";

        return sortBy.ToLower() switch
        {
            "volume" => ascending 
                ? keywords.OrderBy(k => k.SearchVolume).ToList()
                : keywords.OrderByDescending(k => k.SearchVolume).ToList(),
            "difficulty" => ascending 
                ? keywords.OrderBy(k => k.Difficulty).ToList()
                : keywords.OrderByDescending(k => k.Difficulty).ToList(),
            "cpc" => ascending 
                ? keywords.OrderBy(k => k.CostPerClick).ToList()
                : keywords.OrderByDescending(k => k.CostPerClick).ToList(),
            "competition" => ascending 
                ? keywords.OrderBy(k => k.Competition).ToList()
                : keywords.OrderByDescending(k => k.Competition).ToList(),
            "keyword" => ascending 
                ? keywords.OrderBy(k => k.Keyword).ToList()
                : keywords.OrderByDescending(k => k.Keyword).ToList(),
            _ => keywords
        };
    }

    /// <summary>
    /// Validate Alpha-2 country code format
    /// </summary>
    private static bool IsValidAlpha2CountryCode(string countryCode)
    {
        if (string.IsNullOrEmpty(countryCode) || countryCode.Length != 2)
        {
            return false;
        }

        // Basic validation - all letters and uppercase
        return countryCode.All(char.IsLetter);
    }

    /// <summary>
    /// Clean HTML tags and decode HTML entities from text
    /// </summary>
    private static string CleanHtmlTags(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // Replace <br> tags with newlines to preserve line breaks
        var cleanText = Regex.Replace(input, @"<br\s*/?>\s*", "\n", RegexOptions.IgnoreCase);
        
        // Replace </p> tags with double newlines to separate paragraphs
        cleanText = Regex.Replace(cleanText, @"</p>\s*", "\n\n", RegexOptions.IgnoreCase);

        // Remove all other HTML tags
        var htmlTagRegex = new Regex("<[^>]*>", RegexOptions.IgnoreCase);
        cleanText = htmlTagRegex.Replace(cleanText, " ");

        // Decode HTML entities
        cleanText = HttpUtility.HtmlDecode(cleanText);

        // Clean up extra whitespace but preserve line breaks
        cleanText = Regex.Replace(cleanText, @"[ \t]+", " "); // Replace multiple spaces/tabs with single space
        cleanText = Regex.Replace(cleanText, @"[ \t]*\n[ \t]*", "\n"); // Clean up whitespace around newlines
        cleanText = Regex.Replace(cleanText, @"\n\n+", "\n"); // Replace multiple newlines with single newline
        cleanText = cleanText.Trim();

        return cleanText;
    }
}

/// <summary>
/// Extended keyword research request that supports both regionCode and countrycode parameters
/// </summary>
public class KeywordResearchRequestWithCountryCode : KeywordResearchRequest
{
    /// <summary>
    /// Alternative field name for region code (countrycode)
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Gets the effective region code from either RegionCode or CountryCode
    /// </summary>
    public string GetEffectiveRegionCode()
    {
        return !string.IsNullOrEmpty(RegionCode) ? RegionCode :
               !string.IsNullOrEmpty(CountryCode) ? CountryCode.ToUpperInvariant() :
               string.Empty;
    }
}
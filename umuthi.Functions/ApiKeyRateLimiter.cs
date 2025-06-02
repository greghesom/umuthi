using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace umuthi.Functions
{
    /// <summary>
    /// Simple in-memory rate limiter for API keys
    /// For production use, consider using Azure Redis Cache or a distributed rate limiter
    /// </summary>
    public class ApiKeyRateLimiter
    {
        private static readonly ConcurrentDictionary<string, ApiKeyUsageInfo> _usageData = new();
        private readonly ILogger _logger;
        
        // Default limits
        private const int DefaultRequestsPerMinute = 60;
        private const int DefaultRequestsPerHour = 1000;
        private const int DefaultRequestsPerDay = 10000;
        
        // Special limits for specific keys can be configured
        private static readonly ConcurrentDictionary<string, RateLimit> _keySpecificLimits = new();
        
        public ApiKeyRateLimiter(ILogger logger)
        {
            _logger = logger;
            
            // Set up specific limits for known API keys
            SetupKeySpecificLimits();
        }
        
        private void SetupKeySpecificLimits()
        {
            // Default development key (unlimited for testing)
            _keySpecificLimits.TryAdd("umuthi-dev-api-key", new RateLimit
            {
                RequestsPerMinute = int.MaxValue,
                RequestsPerHour = int.MaxValue,
                RequestsPerDay = int.MaxValue
            });
            
            // make.com integration key (higher limits)
            _keySpecificLimits.TryAdd("make-integration-key", new RateLimit
            {
                RequestsPerMinute = 120,
                RequestsPerHour = 3000,
                RequestsPerDay = 30000
            });
            
            // Test keys (lower limits)
            var testKeyLimit = new RateLimit
            {
                RequestsPerMinute = 10,
                RequestsPerHour = 100,
                RequestsPerDay = 200
            };
            
            _keySpecificLimits.TryAdd("test-key-1", testKeyLimit);
            _keySpecificLimits.TryAdd("test-key-2", testKeyLimit);
        }
        
        public bool IsAllowed(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                return false;
            }
            
            var now = DateTimeOffset.UtcNow;
            
            // Get or create usage info for this API key
            var usageInfo = _usageData.GetOrAdd(apiKey, _ => new ApiKeyUsageInfo());
            
            // Update time windows if needed
            UpdateTimeWindows(usageInfo, now);
            
            // Get rate limits for this key
            var limits = GetRateLimitsForKey(apiKey);
            
            // Check if within limits
            if (usageInfo.RequestsInCurrentMinute >= limits.RequestsPerMinute)
            {
                _logger.LogWarning($"API key '{apiKey}' exceeded per-minute rate limit");
                return false;
            }
            
            if (usageInfo.RequestsInCurrentHour >= limits.RequestsPerHour)
            {
                _logger.LogWarning($"API key '{apiKey}' exceeded hourly rate limit");
                return false;
            }
            
            if (usageInfo.RequestsInCurrentDay >= limits.RequestsPerDay)
            {
                _logger.LogWarning($"API key '{apiKey}' exceeded daily rate limit");
                return false;
            }
            
            // Increment counters
            Interlocked.Increment(ref usageInfo.RequestsInCurrentMinute);
            Interlocked.Increment(ref usageInfo.RequestsInCurrentHour);
            Interlocked.Increment(ref usageInfo.RequestsInCurrentDay);
            Interlocked.Increment(ref usageInfo.TotalRequests);
            
            return true;
        }
        
        private void UpdateTimeWindows(ApiKeyUsageInfo usageInfo, DateTimeOffset now)
        {
            // Check and update minute window
            if (now.Subtract(usageInfo.CurrentMinuteStart).TotalMinutes >= 1)
            {
                usageInfo.CurrentMinuteStart = new DateTimeOffset(now.Year, now.Month, now.Day, 
                    now.Hour, now.Minute, 0, now.Offset);
                Interlocked.Exchange(ref usageInfo.RequestsInCurrentMinute, 0);
            }
            
            // Check and update hour window
            if (now.Subtract(usageInfo.CurrentHourStart).TotalHours >= 1)
            {
                usageInfo.CurrentHourStart = new DateTimeOffset(now.Year, now.Month, now.Day, 
                    now.Hour, 0, 0, now.Offset);
                Interlocked.Exchange(ref usageInfo.RequestsInCurrentHour, 0);
            }
            
            // Check and update day window
            if (now.Subtract(usageInfo.CurrentDayStart).TotalDays >= 1)
            {
                usageInfo.CurrentDayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 
                    0, 0, 0, now.Offset);
                Interlocked.Exchange(ref usageInfo.RequestsInCurrentDay, 0);
            }
        }
        
        private RateLimit GetRateLimitsForKey(string apiKey)
        {
            // Try to get specific limits for this key
            if (_keySpecificLimits.TryGetValue(apiKey, out var limits))
            {
                return limits;
            }
            
            // Return default limits
            return new RateLimit
            {
                RequestsPerMinute = DefaultRequestsPerMinute,
                RequestsPerHour = DefaultRequestsPerHour,
                RequestsPerDay = DefaultRequestsPerDay
            };
        }
        
        public UsageStatistics GetUsageStatistics(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey) || !_usageData.TryGetValue(apiKey, out var usageInfo))
            {
                return new UsageStatistics();
            }
            
            var limits = GetRateLimitsForKey(apiKey);
            
            return new UsageStatistics
            {
                TotalRequests = usageInfo.TotalRequests,
                RequestsInCurrentMinute = usageInfo.RequestsInCurrentMinute,
                RequestsInCurrentHour = usageInfo.RequestsInCurrentHour,
                RequestsInCurrentDay = usageInfo.RequestsInCurrentDay,
                MinuteLimit = limits.RequestsPerMinute,
                HourlyLimit = limits.RequestsPerHour,
                DailyLimit = limits.RequestsPerDay
            };
        }
    }
    
    internal class ApiKeyUsageInfo
    {
        public DateTimeOffset CurrentMinuteStart { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset CurrentHourStart { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset CurrentDayStart { get; set; } = DateTimeOffset.UtcNow;
        
        public int RequestsInCurrentMinute;
        public int RequestsInCurrentHour;
        public int RequestsInCurrentDay;
        public long TotalRequests;
    }
    
    internal class RateLimit
    {
        public int RequestsPerMinute { get; set; }
        public int RequestsPerHour { get; set; }
        public int RequestsPerDay { get; set; }
    }
    
    public class UsageStatistics
    {
        public long TotalRequests { get; set; }
        public int RequestsInCurrentMinute { get; set; }
        public int RequestsInCurrentHour { get; set; }
        public int RequestsInCurrentDay { get; set; }
        public int MinuteLimit { get; set; }
        public int HourlyLimit { get; set; }
        public int DailyLimit { get; set; }
    }
}

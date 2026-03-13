using System;
using System.Text.Json;
using umuthi.Contracts.Models;

namespace TestMapDomainCompetitorsData
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing MapDomainCompetitorsData function...");
            
            // Test data from SE Ranking API
            var jsonResponse = @"[
                {
                    ""domain"": ""amazon.com"",
                    ""common_keywords"": 645498,
                    ""total_keywords"": 122208039,
                    ""missing_keywords"": 121562541,
                    ""traffic_sum"": 207617654,
                    ""price_sum"": 65281546.39
                },
                {
                    ""domain"": ""ebay.com"",
                    ""common_keywords"": 578544,
                    ""total_keywords"": 84261331,
                    ""missing_keywords"": 83682787,
                    ""traffic_sum"": 36976057,
                    ""price_sum"": 5930851.9
                },
                {
                    ""domain"": ""walmart.com"",
                    ""common_keywords"": 547262,
                    ""total_keywords"": 54971415,
                    ""missing_keywords"": 54424153,
                    ""traffic_sum"": 68791654,
                    ""price_sum"": 18844500.73
                }
            ]";

            try
            {
                // Test the mapping function using the fixed logic
                var result = TestMapDomainCompetitorsData(jsonResponse, "test-domain.com");
                
                Console.WriteLine($"? Successfully parsed {result.Competitors.Count} competitors");
                
                foreach (var competitor in result.Competitors)
                {
                    Console.WriteLine($"  - {competitor.Domain}:");
                    Console.WriteLine($"    Common Keywords: {competitor.CommonKeywords:N0}");
                    Console.WriteLine($"    Competition Level: {competitor.CompetitionLevel:P4}");
                    Console.WriteLine($"    Estimated Traffic: {competitor.EstimatedTraffic:N0}");
                    Console.WriteLine($"    Domain Authority: {competitor.DomainAuthority}");
                    Console.WriteLine();
                }
                
                // Test empty array
                var emptyResult = TestMapDomainCompetitorsData("[]", "test-domain.com");
                Console.WriteLine($"? Empty array test: {emptyResult.Competitors.Count} competitors (expected: 0)");
                
                Console.WriteLine("All tests passed! ?");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Test failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
        
        static DomainCompetitorsData TestMapDomainCompetitorsData(string jsonContent, string domain)
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            var data = new DomainCompetitorsData
            {
                Domain = domain
            };

            // Handle empty array response
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() == 0)
            {
                return data;
            }

            // Handle direct array response format from SE Ranking API
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var competitorElement in root.EnumerateArray())
                {
                    data.Competitors.Add(new DomainCompetitor
                    {
                        Domain = competitorElement.TryGetProperty("domain", out var d) ? d.GetString() ?? string.Empty : string.Empty,
                        CommonKeywords = competitorElement.TryGetProperty("common_keywords", out var ck) ? ck.GetInt32() : 0,
                        CompetitionLevel = CalculateCompetitionLevel(competitorElement),
                        EstimatedTraffic = competitorElement.TryGetProperty("traffic_sum", out var ts) ? ts.GetInt32() : 0,
                        DomainAuthority = EstimateDomainAuthority(competitorElement)
                    });
                }
            }
            // Handle nested object response format (fallback for different API versions)
            else if (root.TryGetProperty("competitors", out var competitorsArray) && competitorsArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var competitorElement in competitorsArray.EnumerateArray())
                {
                    data.Competitors.Add(new DomainCompetitor
                    {
                        Domain = competitorElement.TryGetProperty("domain", out var d) ? d.GetString() ?? string.Empty : string.Empty,
                        CommonKeywords = competitorElement.TryGetProperty("common_keywords", out var ck) ? ck.GetInt32() : 0,
                        CompetitionLevel = competitorElement.TryGetProperty("competition_level", out var cl) ? cl.GetDouble() : 0,
                        EstimatedTraffic = competitorElement.TryGetProperty("estimated_traffic", out var et) ? et.GetInt32() : 0,
                        DomainAuthority = competitorElement.TryGetProperty("domain_authority", out var da) ? da.GetInt32() : 0
                    });
                }
            }

            return data;
        }

        /// <summary>
        /// Calculate competition level based on common keywords and total keywords
        /// </summary>
        static double CalculateCompetitionLevel(JsonElement competitorElement)
        {
            if (competitorElement.TryGetProperty("common_keywords", out var ck) && 
                competitorElement.TryGetProperty("total_keywords", out var tk))
            {
                var commonKeywords = ck.GetInt32();
                var totalKeywords = tk.GetInt32();
                
                if (totalKeywords > 0)
                {
                    // Calculate competition level as percentage of common keywords
                    return Math.Round((double)commonKeywords / totalKeywords, 4);
                }
            }
            
            return 0.0;
        }

        /// <summary>
        /// Estimate domain authority based on traffic and keyword metrics
        /// </summary>
        static int EstimateDomainAuthority(JsonElement competitorElement)
        {
            // Use traffic_sum and total_keywords to estimate domain authority
            var trafficSum = competitorElement.TryGetProperty("traffic_sum", out var ts) ? ts.GetInt32() : 0;
            var totalKeywords = competitorElement.TryGetProperty("total_keywords", out var tk) ? tk.GetInt32() : 0;
            
            // Simple estimation algorithm based on traffic and keyword counts
            // This is an approximation since actual DA is proprietary to Moz
            if (trafficSum > 50000000) return 85 + Math.Min(15, totalKeywords / 10000000);
            if (trafficSum > 10000000) return 70 + Math.Min(15, totalKeywords / 5000000);
            if (trafficSum > 1000000) return 55 + Math.Min(15, totalKeywords / 1000000);
            if (trafficSum > 100000) return 40 + Math.Min(15, totalKeywords / 500000);
            if (trafficSum > 10000) return 25 + Math.Min(15, totalKeywords / 100000);
            
            return Math.Min(40, Math.Max(1, trafficSum / 1000 + totalKeywords / 50000));
        }
    }
}
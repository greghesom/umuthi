using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using umuthi.Contracts.Interfaces;
using umuthi.Contracts.Interfaces.Services;
using umuthi.Core.Services;
using umuthi.Core.Services.RootScan;
using umuthi.Infrastructure.Configuration;

namespace umuthi.Functions.Extensions;

/// <summary>
/// Extension methods for configuring services in the DI container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add audio processing services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAudioProcessingServices(this IServiceCollection services)
    {
        // Register audio processing services
        services.AddScoped<IAudioConversionService, AudioConversionService>();
        services.AddScoped<ISpeechTranscriptionService, SpeechTranscriptionService>();
        
        return services;
    }
    
    /// <summary>
    /// Add SEO data services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSEOServices(this IServiceCollection services)
    {
        // Register SEO services
        services.AddScoped<ISEORankingService, SEORankingService>();
        services.AddHttpClient<SEORankingService>();
        services.AddMemoryCache(); // For caching SEO data
        
        return services;
    }
    
    /// <summary>
    /// Add usage tracking and analytics services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUsageTrackingServices(this IServiceCollection services)
    {
        // Register usage tracking service for billing
        services.AddScoped<IUsageTrackingService, UsageTrackingService>();
        
        return services;
    }
    
    /// <summary>
    /// Add all umuthi function services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUmuthiFunctionServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddInfrastructure(configuration)
            .AddAudioProcessingServices()
            .AddSEOServices()
            .AddUsageTrackingServices()
            .AddFilloutServices()
            .AddProjectServices()
            .AddRootScanServices();
    }
    
    /// <summary>
    /// Add Fillout webhook processing services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddFilloutServices(this IServiceCollection services)
    {
        // Register Fillout services
        services.AddScoped<IFilloutService, FilloutService>();
        
        return services;
    }
    
    /// <summary>
    /// Add project initialization services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddProjectServices(this IServiceCollection services)
    {
        // Register project services
        services.AddScoped<IProjectInitService, ProjectInitService>();
        
        return services;
    }
    
    /// <summary>
    /// Add RootScan services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddRootScanServices(this IServiceCollection services)
    {
        // Register RootScan services
        services.AddScoped<ICompetitiveIntelligenceEngine, CompetitiveIntelligenceEngine>();
        services.AddScoped<ITechnicalAuditEngine, TechnicalAuditEngine>();
        services.AddScoped<IKeywordIntelligenceService, KeywordIntelligenceService>();
        services.AddScoped<IMarketIntelligenceService, MarketIntelligenceService>();
        services.AddScoped<IReportGenerationService, ReportGenerationService>();
        
        return services;
    }
}
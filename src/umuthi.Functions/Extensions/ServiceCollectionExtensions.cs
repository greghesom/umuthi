using Microsoft.Extensions.DependencyInjection;
using umuthi.Contracts.Interfaces;
using umuthi.Core.Services;

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
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddUmuthiFunctionServices(this IServiceCollection services)
    {
        return services
            .AddAudioProcessingServices()
            .AddUsageTrackingServices();
    }
}
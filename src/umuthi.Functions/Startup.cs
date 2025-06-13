using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using umuthi.Functions.Extensions;

namespace umuthi.Functions;

/// <summary>
/// Startup configuration for umuthi Functions application
/// </summary>
public static class Startup
{
    /// <summary>
    /// Configure the host builder for the Functions application
    /// </summary>
    /// <returns>Configured host builder</returns>
    public static IHostBuilder ConfigureHost()
    {
        return new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureServices(ConfigureServices);
    }
    
    /// <summary>
    /// Configure services for dependency injection
    /// </summary>
    /// <param name="services">Service collection to configure</param>
    private static void ConfigureServices(IServiceCollection services)
    {
        // Add Application Insights telemetry
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Add umuthi function services
        services.AddUmuthiFunctionServices();
    }
}
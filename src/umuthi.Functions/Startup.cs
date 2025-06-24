using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using umuthi.Functions.Extensions;
using Microsoft.EntityFrameworkCore;
using umuthi.Infrastructure.Data;
using Microsoft.Extensions.Logging;

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
    /// <param name="context">Host builder context</param>
    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // Add Application Insights telemetry
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // Add umuthi function services
        services.AddUmuthiFunctionServices(context.Configuration);
        
        // Add database migration service
        services.AddHostedService<MigrationService>();
    }
}

/// <summary>
/// Service to run database migrations on application startup
/// </summary>
public class MigrationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationService> _logger;

    public MigrationService(
        IServiceProvider serviceProvider,
        ILogger<MigrationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running database migrations...");
        
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            if (dbContext.Database.GetPendingMigrations().Any())
            {
                _logger.LogInformation("Applying pending migrations...");
                await dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                _logger.LogInformation("No pending migrations found.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while applying migrations.");
            throw; // Re-throw to fail startup if migrations fail
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
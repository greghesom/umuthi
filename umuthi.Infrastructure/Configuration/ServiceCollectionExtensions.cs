using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using umuthi.Application.Interfaces;
using umuthi.Application.Services;
using umuthi.Domain.Interfaces;
using umuthi.Infrastructure.Data;

namespace umuthi.Infrastructure.Configuration;

public static class ServiceCollectionExtensions
{    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Try manual DbContext configuration with Aspire connection string
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection connection string is not configured.");
        }
        
        // Add Entity Framework with manual connection string
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        // Register repositories and unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register application services
        services.AddScoped<IWorkflowService, WorkflowService>();

        return services;
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using umuthi.Application.Interfaces;
using umuthi.Application.Services;
using umuthi.Domain.Interfaces;
using umuthi.Infrastructure.Data;

namespace umuthi.Infrastructure.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // Register repositories and unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register application services
        services.AddScoped<IWorkflowService, WorkflowService>();

        return services;
    }
}
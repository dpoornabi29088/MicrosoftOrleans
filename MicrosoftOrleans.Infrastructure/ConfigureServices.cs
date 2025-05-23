using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Domain.Interfaces;
using MicrosoftOrleans.Infrastructure.Persistence;
using MicrosoftOrleans.Infrastructure.Repositories;
using MicrosoftOrleans.Infrastructure.Services.Security;

namespace MicrosoftOrleans.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
                                                .SetBasePath(Directory.GetCurrentDirectory())
                                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                                .Build();

        services.AddSingleton<IEncryptionService, EncryptionService>();

        services.AddDbContext<IProductionDbContext, ProductionDbContext>((serviceProvider, options) =>
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.UseSqlServer(configuration.GetConnectionString("ProductionDB"), sqlserverOptions =>
            {
                sqlserverOptions.CommandTimeout(360); // 3 minutes
                sqlserverOptions.EnableRetryOnFailure(
                    maxRetryCount: 5, // Number of retry attempts
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Maximum delay between retries
                    errorNumbersToAdd: null
                );
            });
        }, ServiceLifetime.Scoped);

        services.AddDbContext<IOrleansDbContext, OrleansDbContext>((serviceProvider, options) =>
        {
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.UseSqlServer(configuration.GetConnectionString("OrleansDB"), sqlserverOptions =>
            {
                sqlserverOptions.CommandTimeout(360); // 3 minutes
                sqlserverOptions.EnableRetryOnFailure(
                    maxRetryCount: 5, // Number of retry attempts
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Maximum delay between retries
                    errorNumbersToAdd: null
                );
            });
        }, ServiceLifetime.Scoped);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();


        return services;
    }
}

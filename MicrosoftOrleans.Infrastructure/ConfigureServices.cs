using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftOrleans.Domain.Interfaces;
using MicrosoftOrleans.Infrastructure.Persistence;
using MicrosoftOrleans.Infrastructure.Repositories;

namespace MicrosoftOrleans.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        var config = new ConfigurationBuilder()
                                                .SetBasePath(Directory.GetCurrentDirectory())
                                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                                .Build();

        string connectionString = config.GetSection("DatabaseSettings:ProductionDB").Value;

        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            //var appSetting = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
            options.UseSqlServer(connectionString, sqlserverOptions =>
            {
                sqlserverOptions.CommandTimeout(360); // 3 minutes
                sqlserverOptions.EnableRetryOnFailure(
                    maxRetryCount: 5, // Number of retry attempts
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Maximum delay between retries
                    errorNumbersToAdd: null
                ); // Additional error numbers to be considered transient
            });
        }, ServiceLifetime.Scoped);

        // services.AddScoped<ApplicationDbContext>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();


        return services;
    }
}

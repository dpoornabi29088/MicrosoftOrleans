using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftOrleans.Domain.Interfaces;
using MicrosoftOrleans.Infrastructure.Repositories;
using Orleans.Configuration;
using System.Net;

internal class Program
{
    private static async Task Main(string[] args)
    {

        var host = new HostBuilder()
    .UseOrleans((context, siloBuilder) =>
    {
        siloBuilder.Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "dev";
            options.ServiceId = "UserService";
        });

        siloBuilder.UseLocalhostClustering();
        siloBuilder.Configure<EndpointOptions>(options =>
        {
            options.AdvertisedIPAddress = IPAddress.Loopback;
            options.SiloPort = 11111;
            options.GatewayPort = 30000;
        });

        siloBuilder.AddAdoNetGrainStorage("SqlStore", options =>
        {
            options.Invariant = "System.Data.SqlClient";
            options.ConnectionString = "YourSqlConnectionStringHere";
        });

        //siloBuilder.AddRedisGrainStorage("RedisStore", options =>
        //{
        //    //options.CreateMultiplexer = () => Task.FromResult(ConnectionMultiplexer.Connect("localhost:6379"));
        //});

    })

    .ConfigureServices(services =>
    {
        //services.AddDbContext<DbContext>(options => options.UseSqlServer("YourSqlConnectionStringHere"));
        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IAddressRepository, AddressRepository>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddConsole();
    })
    .UseConsoleLifetime()
    .Build();

        await host.RunAsync();
    }
}
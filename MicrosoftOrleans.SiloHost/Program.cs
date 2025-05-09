using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MicrosoftOrleans.Infrastructure;
using MicrosoftOrleans.Infrastructure.Persistence.Configurations;
using Orleans.Configuration;
using Serilog;

namespace SiloHost;

class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
                          .Enrich.FromLogContext()
                          .Enrich.WithThreadId()
                          .Enrich.WithProcessName()
                          .Enrich.WithEnvironmentUserName()
                          .WriteTo.Console()
                          .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                          .CreateLogger();


        var host = await StartSilo();
        Console.WriteLine("Silo started. Press Enter to terminate...");
        Console.ReadLine();
        await host.StopAsync();
    }

    private static async Task<IHost> StartSilo()
    {
        var hostBuilder = new HostBuilder()
                            .UseSerilog()
                            .UseOrleans(builder =>
                            {
                                var config = new ConfigurationBuilder()
                                                .SetBasePath(Directory.GetCurrentDirectory())
                                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                                .Build();

                                string connectionString = config.GetSection("DatabaseSettings:ConnectionString").Value;


                                builder.UseLocalhostClustering();

                                builder.UseAdoNetClustering(options =>
                                {
                                    options.Invariant = "System.Data.SqlClient";
                                    options.ConnectionString = "Server=164.138.22.154,57655;Database=Orleans;User Id=sa;Password=NeginSystem@1374#9128890105@@;";
                                });

                                builder.Configure<ClusterOptions>(options =>
                                {
                                    options.ClusterId = "us3";
                                    options.ServiceId = "myawesomeservice";
                                });

                                builder.ConfigureServices(services =>
                                {
                                    services.Configure<DatabaseSettings>(builder.Configuration.GetSection("DatabaseSettings"));

                                    services.AddInfrastructureServices();

                                });

                            });

        var host = hostBuilder.Build();

        await host.StartAsync();

        return host;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
                          .MinimumLevel.Debug()
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
                                builder.ConfigureLogging(logging =>
                                {
                                    logging.AddConsole();
                                    logging.SetMinimumLevel(LogLevel.Debug); // Capture detailed logs
                                });


                                var config = new ConfigurationBuilder()
                                                .SetBasePath(Directory.GetCurrentDirectory())
                                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                                .Build();

                                string connectionString = config.GetSection("DatabaseSettings:OrleansDB").Value;


                                builder.UseLocalhostClustering();

                                builder.UseAdoNetClustering(options =>
                                {
                                    options.Invariant = "Microsoft.Data.SqlClient";
                                    options.ConnectionString = connectionString;
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

                                builder.AddAdoNetGrainStorage("DefaultStorage", options =>
                                {
                                    options.Invariant = "Microsoft.Data.SqlClient";
                                    options.ConnectionString = connectionString;
                                });

                            });


        var host = hostBuilder.Build();

        await host.StartAsync();

        return host;

    }
}

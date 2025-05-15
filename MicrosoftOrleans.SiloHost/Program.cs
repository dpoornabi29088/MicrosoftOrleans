using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftOrleans.Infrastructure;
using Orleans.Configuration;
using Serilog;
using System.Text.Json.Serialization;

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


                                var configuration = new ConfigurationBuilder()
                                                .SetBasePath(Directory.GetCurrentDirectory())
                                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                                .Build();

                                string? connectionString = configuration.GetConnectionString("OrleansDB");

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
                                    // Set JSON serializer options to ignore cycles
                                    services.Configure<JsonOptions>(options =>
                                    {
                                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                                    });
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

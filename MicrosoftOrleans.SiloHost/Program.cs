using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftOrleans.Infrastructure;
using Orleans.Configuration;
using Orleans.Streams;
using Serilog;
using System.Text.Json.Serialization;

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
                                builder.ConfigureLogging(logging =>
                                {
                                    logging.AddConsole();
                                    logging.SetMinimumLevel(LogLevel.Error);
                                    logging.AddFilter("Orleans.Streams", LogLevel.Error);
                                    logging.AddFilter("Orleans.Runtime.Scheduler", LogLevel.Error);
                                    logging.AddFilter("Orleans.Runtime.Dispatcher", LogLevel.Error);
                                    logging.AddFilter("Orleans.Streaming", LogLevel.Error);

                                });

                                var configuration = new ConfigurationBuilder()
                                                .SetBasePath(Directory.GetCurrentDirectory())
                                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                                .Build();

                                string? connectionString = configuration.GetConnectionString("OrleansDB");

                                builder.UseLocalhostClustering()

                                .AddMemoryStreams("MemoryStream")//"DefaultStreamProvider") // Register the provider
                                .AddMemoryGrainStorage("PubSubStore") // Required for pub-sub


                                .UseDashboard(options =>
                                {
                                    options.Host = "*"; // Allow access from any host
                                    options.Port = 8080; // Default dashboard port
                                })
                                .UseAdoNetClustering(options =>
                                {
                                    options.Invariant = "Microsoft.Data.SqlClient";
                                    options.ConnectionString = connectionString;
                                })
                                .Configure<ClusterOptions>(options =>
                                {
                                    options.ClusterId = "us3";
                                    options.ServiceId = "myawesomeservice";
                                })
                                .AddAdoNetGrainStorage("DefaultStorage", options =>
                                {
                                    options.Invariant = "Microsoft.Data.SqlClient";
                                    options.ConnectionString = connectionString;
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

                            });


        var host = hostBuilder.Build();

        await host.StartAsync();

        return host;

    }
}

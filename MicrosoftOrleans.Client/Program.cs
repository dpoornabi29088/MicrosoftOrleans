// Program.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;
using Serilog;

namespace Client;

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

        var services = new ServiceCollection();

        var client = await ConnectClient();

        var serviceProvider = services
            .AddSingleton(client)
            .AddLogging(loggingBuilder => loggingBuilder.AddSerilog())
            .BuildServiceProvider();

        Console.WriteLine("Client connected. Press Enter to terminate...");
        Console.ReadLine();
    }

    private static async Task<IGrainFactory> ConnectClient()
    {
        using IHost host = new HostBuilder()
                             .UseSerilog()
                             .UseOrleansClient(clientBuilder =>
                             {
                                 clientBuilder.UseLocalhostClustering();
                                 clientBuilder.Configure<ClusterOptions>(options =>
                                 {
                                     options.ClusterId = "us3";
                                     options.ServiceId = "myawesomeservice";
                                 });


                             })
                             .Build();

        await host.StartAsync();

        IGrainFactory client = host.Services.GetRequiredService<IGrainFactory>();

        return client;
    }
}

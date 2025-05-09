using Orleans.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
             .Enrich.FromLogContext()
             .Enrich.WithThreadId()
             .Enrich.WithProcessName()
             .Enrich.WithEnvironmentUserName()
             .WriteTo.Console()
             .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSingleton(client);
builder.Services.AddSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
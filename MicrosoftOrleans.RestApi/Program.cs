using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MicrosoftOrleans.Application.Interfaces;
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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyOrleans API",
        Version = "v1",
        Description = "Orleans-based API Client",
        Contact = new OpenApiContact
        {
            Name = "Davood",
            Email = "davood@example.com",
            Url = new Uri("https://myorleansapi.example.com")
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyOrleans API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapGet("/AddUser", async ([FromServices] IGrainFactory grainFactory, int userId) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(userId);
    return await userGrain.GetUserAsync();
});

app.Run();
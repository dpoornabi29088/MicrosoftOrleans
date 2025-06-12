using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Application.Grains.Observer;
using MicrosoftOrleans.Application.Grains.Observer.Implementation;
using MicrosoftOrleans.RestApi.Middlewares;
using Orleans.Configuration;
using Serilog;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
             .Enrich.FromLogContext()
             .Enrich.WithThreadId()
             .Enrich.WithProcessName()
             .Enrich.WithEnvironmentUserName()
             .WriteTo.Console()
             .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
             .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

// Set JSON serializer options to ignore cycles
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

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

IClusterClient client = host.Services.GetRequiredService<IClusterClient>();

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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyOrleans API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.Urls.Add("http://127.0.0.1:5001");
app.Urls.Add("http://127.0.0.1:5002");


app.MapPost("/Login", async ([FromServices] IClusterClient clusterClient, [FromBody] LoginDto loginDto) =>
{
    var userGrain = clusterClient.GetGrain<IUserGrain>(loginDto.UserName);
    return await userGrain.LoginAsync(loginDto);
});

app.MapGet("/GetUser", async ([FromServices] IClusterClient clusterClient, string userName) =>
{
    var userGrain = clusterClient.GetGrain<IUserGrain>(userName);
    return await userGrain.GetUserAsync();
});

app.MapPost("/AddUser", async ([FromServices] IClusterClient clusterClient, [FromBody] CreateUserDto userDto) =>
{
    var userGrain = clusterClient.GetGrain<IUserGrain>(userDto.UserName);
    await userGrain.AddUserAsync(userDto);
});

app.MapPost("/UpdateUserName", async ([FromServices] IClusterClient clusterClient, string oldUserName, string newUserName) =>
{
    var userGrain = clusterClient.GetGrain<IUserGrain>(oldUserName);
    await userGrain.UpdateUserNameAsync(newUserName);
});

app.MapPost("/DeleteUser", async ([FromServices] IClusterClient clusterClient, string userName) =>
{
    var userGrain = clusterClient.GetGrain<IUserGrain>(userName);
    await userGrain.DeleteCurrentUserAsync();
});

app.MapPost("/GetAddresses", async ([FromServices] IClusterClient clusterClient, string userName) =>
{
    var userGrain = clusterClient.GetGrain<IUserGrain>(userName);
    return await userGrain.GetAddressesAsync();
});

app.MapPost("/AddAddress", async ([FromServices] IClusterClient clusterClient, [FromBody] CreateAddressDto createAddressDto) =>
{
    var userGrain = clusterClient.GetGrain<IUserGrain>(createAddressDto.UserName);
    await userGrain.AddAddressAsync(createAddressDto);
});

app.MapPost("/UpdateAddress", async ([FromServices] IClusterClient clusterClient, [FromBody] UpdateAddressDto updateAddressDto) =>
{
    var userGrain = clusterClient.GetGrain<IUserGrain>(updateAddressDto.UserName);
    await userGrain.UpdateAddressAsync(updateAddressDto);
});

app.MapPost("/DeleteAddress", async ([FromServices] IClusterClient clusterClient, [FromBody] DeleteAddressDto deleteAddressDto) =>
{
    var userGrain = clusterClient.GetGrain<IUserGrain>(deleteAddressDto.UserName);
    await userGrain.DeleteAddressAsync(deleteAddressDto);
});

app.MapPost("/Subscribe", async ([FromServices] IClusterClient clusterClient) =>
{
    var friend = clusterClient.GetGrain<IHelloGrain>(0); // Get grain reference
    Chat c = new Chat(); // Create local observer instance
    var obj = clusterClient.CreateObjectReference<IChat>(c); // Convert to Orleans reference
    await friend.Subscribe(obj); // Subscribe to receive updates
});

app.MapPost("/SendMessage", async ([FromServices] IClusterClient clusterClient, [FromQuery] string message) =>
{
    var helloGrain = clusterClient.GetGrain<IHelloGrain>(0);
    await helloGrain.SendUpdateMessage(message);
});

app.Run();
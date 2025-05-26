using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
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

app.MapPost("/Login", async ([FromServices] IClusterClient grainFactory, [FromBody] LoginDto loginDto) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(loginDto.UserName);
    return await userGrain.LoginAsync(loginDto);
});

app.MapGet("/GetUser", async ([FromServices] IClusterClient grainFactory, string userName) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(userName);
    return await userGrain.GetUserAsync();
});

app.MapPost("/AddUser", async ([FromServices] IClusterClient grainFactory, [FromBody] CreateUserDto userDto) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(userDto.UserName);
    await userGrain.AddUserAsync(userDto);
});

app.MapPost("/UpdateUserName", async ([FromServices] IClusterClient grainFactory, string oldUserName, string newUserName) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(oldUserName);
    await userGrain.UpdateUserNameAsync(newUserName);
});

app.MapPost("/DeleteUser", async ([FromServices] IClusterClient grainFactory, string userName) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(userName);
    await userGrain.DeleteCurrentUserAsync();
});

app.MapPost("/GetAddresses", async ([FromServices] IClusterClient grainFactory, string userName) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(userName);
    return await userGrain.GetAddressesAsync();
});

app.MapPost("/AddAddress", async ([FromServices] IClusterClient grainFactory, [FromBody] CreateAddressDto createAddressDto) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(createAddressDto.UserName);
    await userGrain.AddAddressAsync(createAddressDto);
});

app.MapPost("/UpdateAddress", async ([FromServices] IClusterClient grainFactory, [FromBody] UpdateAddressDto updateAddressDto) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(updateAddressDto.UserName);
    await userGrain.UpdateAddressAsync(updateAddressDto);
});

app.MapPost("/DeleteAddress", async ([FromServices] IClusterClient grainFactory, [FromBody] DeleteAddressDto deleteAddressDto) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(deleteAddressDto.UserName);
    await userGrain.DeleteAddressAsync(deleteAddressDto);
});

app.Run();
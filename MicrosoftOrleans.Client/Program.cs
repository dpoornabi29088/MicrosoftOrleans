using MicrosoftOrleans.Application.Interfaces;
using Orleans.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Configure the Orleans client
builder.Host.UseOrleansClient(clientBuilder =>
{
    clientBuilder.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = "dev";
        options.ServiceId = "UserService";
    });

    clientBuilder.UseLocalhostClustering();

});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.MapGet("/GetUser", (int value, IGrainFactory grainFactory) =>
{
    var userGrain = grainFactory.GetGrain<IUserGrain>(value);
    userGrain.GetUserAsync();

});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        options.RoutePrefix = string.Empty;
    });
}

app.Run();

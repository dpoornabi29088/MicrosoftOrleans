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
    //clientBuilder.ConfigureServices(services => { services.AddSingleton<ITestGrain, TestGrain>(); });

});

//builder.Services.AddSingleton<IClusterClient>(sp => sp.GetService<IClusterClient>());


var app = builder.Build();

//app.MapGet("/Add", (int value, IGrainFactory grainFactory) =>
//{
//    var testGrain = grainFactory.GetGrain<ITestGrain>("ITest");
//    testGrain.AddInstruction(value);

//});

app.Run();

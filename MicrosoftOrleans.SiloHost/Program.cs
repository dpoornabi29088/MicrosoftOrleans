using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftOrleans.Domain.Interfaces;
using MicrosoftOrleans.Infrastructure.Repositories;
using Orleans.Configuration;
using System.Net;

internal class Program
{
    private static async Task Main(string[] args)
    {

        var host = new HostBuilder()
                    .UseOrleans((context, siloBuilder) =>
                    {
                        siloBuilder.Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "dev";
                            options.ServiceId = "UserService";
                        });

                        siloBuilder.UseLocalhostClustering();
                        siloBuilder.Configure<EndpointOptions>(options =>
                        {
                            options.AdvertisedIPAddress = IPAddress.Loopback;
                            options.SiloPort = 11111;
                            options.GatewayPort = 30000;
                        });

                        siloBuilder.ConfigureLogging(logging =>
                        {
                            logging.AddConsole();
                            logging.SetMinimumLevel(LogLevel.Debug);
                        });


                        //siloBuilder.AddAdoNetGrainStorage("SqlStore", options =>
                        //{
                        //    options.Invariant = "System.Data.SqlClient";
                        //    options.ConnectionString = "YourSqlConnectionStringHere";
                        //});

                        //siloBuilder.AddRedisGrainStorage("s", c =>
                        //{
                        //    /*
                        //       //for example
                        //         ConfigurationOptions option = new ConfigurationOptions
                        //         {
                        //             AbortOnConnectFail = false,
                        //             SyncTimeout = 50000,
                        //             ConnectTimeout = 10000,
                        //             AllowAdmin = true,
                        //             KeepAlive = 10,
                        //             EndPoints = { EndPointCollection.TryParse("127.0.0.1:6379") }
                        //         };

                        //     */
                        //    c.CreateMultiplexer = (x)=> Task.FromResult((IConnectionMultiplexer)ConnectionMultiplexer.Connect(""));
                        //});


                        //siloBuilder.AddRedisGrainStorage("RedisStore", options =>
                        //{
                        //    //options.CreateMultiplexer = () => Task.FromResult(ConnectionMultiplexer.Connect("localhost:6379"));
                        //});

                    })

                    .ConfigureServices(services =>
                    {
                        // services.AddDbContext<DbContext>(options => options.UseSqlServer("YourSqlConnectionStringHere"));
                        services.AddScoped<IUserRepository, UserRepository>();
                        services.AddScoped<IAddressRepository, AddressRepository>();
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.AddConsole();
                    })
                    .UseConsoleLifetime()
                    .Build();

        await host.RunAsync();
    }
}
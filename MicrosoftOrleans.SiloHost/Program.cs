// Program.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MicrosoftOrleans.Domain.Interfaces;
using MicrosoftOrleans.Infrastructure.Grains;
using MicrosoftOrleans.Infrastructure.Repositories;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace SiloHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = await StartSilo();
            Console.WriteLine("Silo started. Press Enter to terminate...");
            Console.ReadLine();
            await host.StopAsync();
        }

        private static async Task<ISiloHost> StartSilo()
        {
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "UserService";
                })
                .AddAdoNetGrainStorage("SqlStore", options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = "YourSqlConnectionStringHere";
                })
                .AddRedisGrainStorage("RedisStore", options =>
                {
                    options.ConnectionString = "YourRedisConnectionStringHere";
                })
                .ConfigureServices(services =>
                {
                    services.AddDbContext<DbContext>(options =>
                        options.UseSqlServer("YourSqlConnectionStringHere"));
                    services.AddTransient<IUserRepository, UserRepository>();
                    services.AddTransient<IAddressRepository, AddressRepository>();
                })
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(UserGrain).Assembly).WithReferences());

            var host = builder.Build();

            await host.StartAsync();

            return host;
        }
    }
}

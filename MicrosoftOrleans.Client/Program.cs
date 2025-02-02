// Program.cs
using Microsoft.Extensions.DependencyInjection;
using MicrosoftOrleans.Application.Services;
using MicrosoftOrleans.Domain.Entities;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IClusterClient client = await ConnectClient();
            var serviceProvider = new ServiceCollection()
                .AddSingleton(client)
                .AddTransient<UserService>()
                .BuildServiceProvider();

            var userService = serviceProvider.GetService<UserService>();

            // Example usage
            var user = new User { Id = 1, UserName = "Davood123", Password = "123456" };

            await userService.AddUserAsync(user);

            var retrievedUser = await userService.GetUserAsync(1);
            Console.WriteLine($"User: {retrievedUser.UserName}");

            var address = new Address { Id = 1, Street = "123 Main St", City = "Anytown" };
            await userService.AddAddressAsync(1, address);

            var addresses = await userService.GetAddressesAsync(1);
            foreach (var addr in addresses)
            {
                Console.WriteLine($"Address: {addr.Street}, {addr.City}");
            }

            Console.WriteLine("Client connected. Press Enter to terminate...");
            Console.ReadLine();
        }

        private static async Task<IClusterClient> ConnectClient()
        {
            var client = new ClientBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "UserService";
                })
                .Build();

            await client.Connect();
            return client;
        }
    }
}

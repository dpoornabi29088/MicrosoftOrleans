using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Domain.Entities;
using System.Reflection;

namespace MicrosoftOrleans.Infrastructure.Persistence;

public class ProductionDbContext : DbContext, IProductionDbContext
{
    private const string ConnectionName = "ProductionDB";
    public ProductionDbContext(DbContextOptions<ProductionDbContext> options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Address> Addresses { get; set; }

    public DatabaseFacade GetDatabase()
    {
        return Database;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(builder);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        SetDataProvider(optionsBuilder);
    }

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ProductionDbContext>
    {
        public ApplicationDbContextFactory()
        {
        }
        public ProductionDbContext CreateDbContext(string[] args)
        {
            return new(SetDataProvider(new DbContextOptionsBuilder<ProductionDbContext>()).Options);
        }
    }

    private static void SetDataProvider(DbContextOptionsBuilder optionsBuilder)
    {
        IConfiguration _configuration = GetConfiguration();
        optionsBuilder.UseSqlServer(_configuration.GetConnectionString(ConnectionName));
    }

    private static DbContextOptionsBuilder<ProductionDbContext> SetDataProvider(DbContextOptionsBuilder<ProductionDbContext> optionsBuilder)
    {
        IConfiguration _configuration = GetConfiguration();
        optionsBuilder.UseSqlServer(_configuration.GetConnectionString(ConnectionName));
        return optionsBuilder;
    }

    private static IConfiguration GetConfiguration()
    {
        IConfiguration _configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build();

        return _configuration;
    }
}

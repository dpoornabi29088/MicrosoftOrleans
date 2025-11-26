using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Domain.Entities;
using System.Reflection;

namespace MicrosoftOrleans.Infrastructure.Persistence;

public class OrleansDbContext : DbContext, IOrleansDbContext
{
    private const string ConnectionName = "OrleansDB";
    public OrleansDbContext(DbContextOptions<OrleansDbContext> options)
    {
    }

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

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<OrleansDbContext>
    {
        public ApplicationDbContextFactory()
        {
        }
        public OrleansDbContext CreateDbContext(string[] args)
        {
            return new(SetDataProvider(new DbContextOptionsBuilder<OrleansDbContext>()).Options);
        }
    }

    private static void SetDataProvider(DbContextOptionsBuilder optionsBuilder)
    {
        IConfiguration _configuration = GetConfiguration();
        optionsBuilder.UseSqlServer(_configuration.GetConnectionString(ConnectionName));
    }

    private static DbContextOptionsBuilder<OrleansDbContext> SetDataProvider(DbContextOptionsBuilder<OrleansDbContext> optionsBuilder)
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

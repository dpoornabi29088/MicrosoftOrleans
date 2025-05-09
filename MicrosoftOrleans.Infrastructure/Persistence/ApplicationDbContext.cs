using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using MicrosoftOrleans.Domain.Entities;
using System.Reflection;

namespace MicrosoftOrleans.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    {
    }

    public DbSet<User> User { get; set; }
    public DbSet<Address> Address { get; set; }
    public DatabaseFacade GetDatabase()
    {
        return Database;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        //Set sequence
        builder.HasSequence<long>("ExectingProcessSequences")
            .StartsAt(1)
        .IncrementsBy(1);

        //builder.Ignore<ExecutingProcessVm>();

        base.OnModelCreating(builder);
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        SetDataProvider(optionsBuilder);
    }

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContextFactory()
        {
        }
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            return new(SetDataProvider(new DbContextOptionsBuilder<ApplicationDbContext>()).Options);
        }
    }

    private static void SetDataProvider(DbContextOptionsBuilder optionsBuilder)
    {
        IConfiguration _configuration = GetConfiguration();
        optionsBuilder.UseSqlServer(_configuration.GetConnectionString("ProductionDB"));
    }
    private static DbContextOptionsBuilder<ApplicationDbContext> SetDataProvider(DbContextOptionsBuilder<ApplicationDbContext> optionsBuilder)
    {
        IConfiguration _configuration = GetConfiguration();
        optionsBuilder.UseSqlServer(_configuration.GetConnectionString("ProductionDB"));
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

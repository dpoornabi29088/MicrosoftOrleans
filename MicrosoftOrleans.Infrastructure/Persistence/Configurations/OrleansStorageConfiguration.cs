using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Infrastructure.Persistence.Configurations;

public class OrleansStorageConfiguration : IEntityTypeConfiguration<OrleansStorage>
{
    public void Configure(EntityTypeBuilder<OrleansStorage> builder)
    {
        builder.HasNoKey();
        builder.ToTable(nameof(OrleansStorage));
    }
}

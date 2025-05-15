using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        //builder.HasMany(x => x.Addresses)
        //   .WithOne(x => x.User)
        //   .OnDelete(DeleteBehavior.NoAction);
    }
}

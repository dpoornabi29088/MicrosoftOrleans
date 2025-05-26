using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        //builder.HasOne(x => x.User)
       //    .WithMany(x => x.Addresses)
       //    .HasForeignKey(x => x.UserId)
        //   .OnDelete(DeleteBehavior.NoAction);
    }
}

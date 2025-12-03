using Microsoft.EntityFrameworkCore;
using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Application.Common.Interfaces;

public interface IProductionDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<Address> Addresses { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

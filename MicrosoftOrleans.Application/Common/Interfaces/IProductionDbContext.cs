using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Application.Common.Interfaces;

public interface IProductionDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<Address> Addresses { get; set; }
    DatabaseFacade GetDatabase();
}

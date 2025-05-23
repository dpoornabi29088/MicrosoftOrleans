using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;
using MicrosoftOrleans.Infrastructure.Persistence;

namespace MicrosoftOrleans.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly ProductionDbContext _context;

    public AddressRepository(ProductionDbContext context)
    {
        _context = context;
    }

    public async Task<Address?> FindAsync(int addressId) => await _context.Addresses.FindAsync(addressId);

    public async Task AddAsync(Address address) => await _context.Addresses.AddAsync(address);

    public async Task Update(Address address) => _context.Addresses.Update(address);

    public async Task Remove(int addressId)
    {
        var address = await FindAsync(addressId);

        if (address is not null)
        {
            _context.Addresses.Remove(address);
        }
    }
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}

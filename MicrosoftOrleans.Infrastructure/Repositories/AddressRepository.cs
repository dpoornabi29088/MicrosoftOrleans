using Microsoft.EntityFrameworkCore;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;

namespace MicrosoftOrleans.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly DbContext _context;

    public AddressRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<Address> GetAddressAsync(int addressId)
    {
        return await _context.Set<Address>().FindAsync(addressId);
    }

    public async Task AddAddressAsync(Address address)
    {
        await _context.Set<Address>().AddAsync(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAddressAsync(Address address)
    {
        _context.Set<Address>().Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAddressAsync(int addressId)
    {
        var address = await GetAddressAsync(addressId);
        if (address != null)
        {
            _context.Set<Address>().Remove(address);
            await _context.SaveChangesAsync();
        }
    }
}

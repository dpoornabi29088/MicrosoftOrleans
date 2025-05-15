using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Domain.Interfaces;

public interface IAddressRepository
{
    Task<Address> FindAsync(int addressId);
    Task AddAsync(Address address);
    Task Update(Address address);
    Task Remove(int addressId);
    Task SaveChangesAsync();
}

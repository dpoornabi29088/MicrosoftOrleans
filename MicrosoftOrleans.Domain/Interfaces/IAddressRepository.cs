using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Domain.Interfaces;

public interface IAddressRepository
{
    Task<Address> GetAddressAsync(int addressId);
    Task AddAddressAsync(Address address);
    Task UpdateAddressAsync(Address address);
    Task DeleteAddressAsync(int addressId);
}

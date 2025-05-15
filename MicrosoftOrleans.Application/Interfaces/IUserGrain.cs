using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Domain.Entities;
using Orleans;

namespace MicrosoftOrleans.Application.Interfaces
{
    public interface IUserGrain : IGrainWithIntegerKey
    {
        Task<User?> GetUserAsync();
        Task AddUserAsync(CreateUserDto user);
        Task UpdateUsernameAsync(string userName);
        Task DeleteUserAsync();
        //Task<List<Address>> GetAddressesAsync();
        //Task AddAddressAsync(Address address);
        //Task UpdateAddressAsync(Address address);
        //Task DeleteAddressAsync(int addressId);
    }
}

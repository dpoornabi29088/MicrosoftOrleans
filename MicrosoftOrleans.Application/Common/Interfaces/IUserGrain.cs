using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Domain.Entities;
using Orleans;

namespace MicrosoftOrleans.Application.Common.Interfaces
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<bool> LoginAsync(LoginDto loginDto);
        Task<User?> GetUserAsync();
        Task AddUserAsync(CreateUserDto user);
        Task ChangeUserNameAsync(string newUserName);
        Task DeleteUserAsync();
        //Task<List<Address>> GetAddressesAsync();
        //Task AddAddressAsync(Address address);
        //Task UpdateAddressAsync(Address address);
        //Task DeleteAddressAsync(int addressId);
    }
}

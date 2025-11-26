using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Application.Common.Interfaces
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<bool> LoginAsync(LoginDto loginDto);
        Task<User> GetUserAsync();
        Task AddUserAsync(CreateUserDto user);
        Task UpdateUserNameAsync(string newUserName);
        Task DeleteCurrentUserAsync();
        Task<List<Address>> GetAddressesAsync();
        Task AddAddressAsync(CreateAddressDto addressDto);
        Task UpdateAddressAsync(UpdateAddressDto updateCityDto);
        Task DeleteAddressAsync(DeleteAddressDto deleteAddressDto);
        Task<List<StockTickDto>> GetMyStockHistory();
    }
}

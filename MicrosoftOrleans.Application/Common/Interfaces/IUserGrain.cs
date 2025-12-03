using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Application.StateModels;
using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Application.Common.Interfaces
{
    public interface IUserGrain : IGrainWithStringKey
    {
        Task<bool> LoginAsync(LoginDto loginDto);
        Task<UserState> GetUserAsync();
        Task AddUserAsync(CreateUserDto user);
        Task UpdateUserNameAsync(string newUserName);
        Task DeleteCurrentUserAsync();
        Task<List<AddressState>> GetAddressesAsync();
        Task AddAddressAsync(CreateAddressDto addressDto);
        Task UpdateAddressAsync(UpdateAddressDto updateCityDto);
        Task DeleteAddressAsync(DeleteAddressDto deleteAddressDto);
        Task<List<StockTickDto>> GetMyStockHistory();
    }
}

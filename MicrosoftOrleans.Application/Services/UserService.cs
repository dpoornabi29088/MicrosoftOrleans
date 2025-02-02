using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Application.Services;

public class UserService
{
    private readonly IGrainFactory _grainFactory;

    public UserService(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<User> GetUserAsync(int userId)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);
        return await userGrain.GetUserAsync();
    }

    public async Task AddUserAsync(User user)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(user.Id);
        await userGrain.AddUserAsync(user);
    }

    public async Task UpdateUserAsync(User user)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(user.Id);
        await userGrain.UpdateUserAsync(user);
    }

    public async Task DeleteUserAsync(int userId)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);
        await userGrain.DeleteUserAsync();
    }

    public async Task<List<Address>> GetAddressesAsync(int userId)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);
        return await userGrain.GetAddressesAsync();
    }

    public async Task AddAddressAsync(int userId, Address address)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);
        await userGrain.AddAddressAsync(address);
    }

    public async Task UpdateAddressAsync(int userId, Address address)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);
        await userGrain.UpdateAddressAsync(address);
    }

    public async Task DeleteAddressAsync(int userId, int addressId)
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(userId);
        await userGrain.DeleteAddressAsync(addressId);
    }
}

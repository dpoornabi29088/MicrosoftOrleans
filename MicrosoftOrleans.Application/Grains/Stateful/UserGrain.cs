using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;

namespace MicrosoftOrleans.Application.Grains.Stateful;

public class UserGrain : Grain, IUserGrain
{
    private readonly IEncryptionService _encryptionService;
    private readonly IPersistentState<User> _userState;
    private IStockConsumerGrain? _stockConsumer;
    private readonly IClusterClient _clusterClient;
    public UserGrain(IEncryptionService encryptionService,
                     [PersistentState("user", "DefaultStorage")] IPersistentState<User> userState,
                     IClusterClient clusterClient
        )
    {
        _encryptionService = encryptionService;
        _userState = userState;
        _clusterClient = clusterClient;
    }

    public async Task SubscribeToStock(string symbol)
    {
        // Create unique consumer instance per user+symbol
        _stockConsumer = _clusterClient.GetGrain<IStockConsumerGrain>(
            $"{this.GetPrimaryKeyString()}_{symbol}");

        await _stockConsumer.SubscribeToSymbol(symbol);
        Console.WriteLine($"USER {this.GetPrimaryKeyString()}: Subscribed to {symbol}");
    }

    public Task<List<StockTickDto>> GetMyStockHistory()
        => _stockConsumer?.GetHistory() ?? Task.FromResult(new List<StockTickDto>());

    public Task<User> GetUserAsync()
    {
        return Task.FromResult(_userState.State);
    }

    public Task<bool> LoginAsync(LoginDto loginDto)
    {
        var decryptedPassword = _encryptionService.Decrypt(_userState.State.Password, _userState.State.IV);
        if (decryptedPassword != loginDto.Password)
            throw new Exception("The username or password is incorrect!");

        return Task.FromResult(true);
    }

    public async Task AddUserAsync(CreateUserDto userDto)
    {
        var user = User.Create(userDto.UserName, userDto.Password, _encryptionService);

        _userState.State = user;

        await _userState.WriteStateAsync();
    }

    public async Task UpdateUserNameAsync(string newUserName)
    {
        _userState.State.SetUserName(newUserName);

        await _userState.WriteStateAsync();
    }

    public async Task DeleteCurrentUserAsync()
    {
        await _userState.ClearStateAsync();

        await _userState.WriteStateAsync();

        DeactivateOnIdle();
    }

    public async Task AddAddressAsync(CreateAddressDto addressDto)
    {
        var address = Address.Create(_userState.State.Id, addressDto.City, addressDto.Street, addressDto.Alley, addressDto.Plaque);

        if (_userState.State.Addresses.Any(x => x.Equals(address)))
            throw new Exception("Duplicate address!");

        _userState.State.Addresses.Add(address);

        await _userState.WriteStateAsync();
    }

    public async Task<List<Address>> GetAddressesAsync()
    {
        return await Task.FromResult(_userState.State.Addresses);
    }

    public async Task UpdateAddressAsync(UpdateAddressDto updateAddressDto)
    {
        var address = _userState.State.Addresses.SingleOrDefault(x => x.Id == updateAddressDto.addressId);

        if (address is null)
            throw new Exception("The address not found!");

        address.SetCity(updateAddressDto.City);
        address.SetStreet(updateAddressDto.Street);
        address.SetAlley(updateAddressDto.Alley);
        address.SetPlaque(updateAddressDto.Plaque);

        await _userState.WriteStateAsync();
    }

    public async Task DeleteAddressAsync(DeleteAddressDto deleteAddressDto)
    {
        var address = _userState.State.FindAddress(deleteAddressDto.City, deleteAddressDto.Street, deleteAddressDto.Alley, deleteAddressDto.Plaque);

        if (address is null)
            return;

        _userState.State.Addresses.Remove(address);

        await _userState.WriteStateAsync();
    }
}

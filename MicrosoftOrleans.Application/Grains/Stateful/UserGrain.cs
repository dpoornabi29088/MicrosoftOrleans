using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;
using Orleans.Streams;
using Serilog;

namespace MicrosoftOrleans.Application.Grains.Stateful;

public class UserGrain : Grain, IUserGrain, IAsyncObserver<StockTickDto>
{
    private readonly IEncryptionService _encryptionService;
    private readonly IPersistentState<User> _userState;
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

    private readonly List<StockTickDto> _history = new();

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var provider = this.GetStreamProvider("MemoryStream");
        var stream = provider.GetStream<StockTickDto>("STOCKS", "GlobalStream");
        await stream.SubscribeAsync(this);
    }

    public Task OnNextAsync(StockTickDto tick, StreamSequenceToken? token = null)
    {
        _history.Add(tick);
        return Task.CompletedTask;
    }

    public Task OnCompletedAsync() => Task.CompletedTask;
    public Task OnErrorAsync(Exception ex) => Task.CompletedTask;

    public Task<List<StockTickDto>> GetMyStockHistory() => Task.FromResult(_history);

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

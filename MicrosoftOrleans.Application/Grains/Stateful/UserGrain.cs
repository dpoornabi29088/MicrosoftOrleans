using Microsoft.EntityFrameworkCore;
using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Application.StateModels;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;
using Orleans.Streams;
using Serilog;

namespace MicrosoftOrleans.Application.Grains.Stateful;

public class UserGrain : Grain, IUserGrain, IAsyncObserver<StockTickDto>
{
    private readonly IEncryptionService _encryptionService;
    private readonly IPersistentState<UserState> _userState;
    private readonly IClusterClient _clusterClient;
    private readonly IProductionDbContext _productionDbContext;
    public UserGrain(IEncryptionService encryptionService,
                     [PersistentState("user", "DefaultStorage")] IPersistentState<UserState> userState,
                     IClusterClient clusterClient
,
                     IProductionDbContext productionDbContext)
    {
        _encryptionService = encryptionService;
        _userState = userState;
        _clusterClient = clusterClient;
        _productionDbContext = productionDbContext;
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

    public Task<UserState> GetUserAsync()
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
        try
        {
            var user = User.Create(userDto.UserName, userDto.Password, _encryptionService);

            _userState.State = new UserState
            {
                Id = user.Id,
                IV = user.IV,
                UserName = user.UserName,
                Password = user.Password,
                Addresses = new List<AddressState> { },
            };

            await _userState.WriteStateAsync();

            _productionDbContext.Users.Add(user);

            await _productionDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "AddUserAsync --> {@CreateUserDto}", userDto);
        }
    }

    public async Task UpdateUserNameAsync(string newUserName)
    {
        try
        {
            var user = await _productionDbContext.Users.FirstOrDefaultAsync(x => x.UserName == newUserName);
            if (user is null)
                throw new ArgumentException("User name not found!");

            _userState.State.UserName = newUserName;

            await _userState.WriteStateAsync();

            user.SetUserName(newUserName);

            await _productionDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "UpdateUserNameAsync --> {UserName}", newUserName);
        }
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

        try
        {
            if (_userState.State.Addresses.Any(x => x.Equals(address)))
                throw new Exception("Duplicate address!");

            _userState.State.Addresses.Add(new AddressState
            {
                UserId = _userState.State.Id,
                City = addressDto.City,
                Street = addressDto.Street,
                Alley = addressDto.Alley,
                Plaque = addressDto.Plaque
            });


            await _userState.WriteStateAsync();

            _productionDbContext.Addresses.Add(address);
            await _productionDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "AddAddressAsync --> {@Address}", address);
        }
    }

    public async Task<List<AddressState>> GetAddressesAsync()
    {
        return await Task.FromResult(_userState.State.Addresses);
    }

    public async Task UpdateAddressAsync(UpdateAddressDto updateAddressDto)
    {
        var addressState = _userState.State.Addresses.SingleOrDefault(x => x.Id == updateAddressDto.addressId);

        try
        {
            var address = await _productionDbContext.Addresses.SingleOrDefaultAsync(x => x.Id == updateAddressDto.addressId);

            if (addressState is null ||
                address is null)
                throw new Exception("The address not found!");

            addressState.City = updateAddressDto.City;
            addressState.Street = updateAddressDto.Street;
            addressState.Alley = updateAddressDto.Alley;
            addressState.Plaque = updateAddressDto.Plaque;

            await _userState.WriteStateAsync();

            address.SetCity(updateAddressDto.City);
            address.SetStreet(updateAddressDto.Street);
            address.SetAlley(updateAddressDto.Alley);
            address.SetPlaque(updateAddressDto.Plaque);

            await _productionDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "UpdateAddressAsync --> {@Address}", addressState);
        }
    }

    public async Task DeleteAddressAsync(DeleteAddressDto deleteAddressDto)
    {
        try
        {
            var addressState = _userState.State.Addresses.FirstOrDefault(x=>x.Id == deleteAddressDto.AddressId);
            var address = await _productionDbContext.Addresses.SingleOrDefaultAsync(x => x.Id == deleteAddressDto.AddressId);

            if (addressState is null || 
                address is null)
                return;

            _userState.State.Addresses.Remove(addressState);

            await _userState.WriteStateAsync();

            _productionDbContext.Addresses.Remove(address);

            await _productionDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "DeleteAddressAsync --> {AddressId}", deleteAddressDto.AddressId);
        }
    }
}

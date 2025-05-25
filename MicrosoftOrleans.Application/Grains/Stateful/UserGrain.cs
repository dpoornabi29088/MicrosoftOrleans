using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;
using Orleans;
using Orleans.Providers;
using Serilog;

namespace MicrosoftOrleans.Application.Grains.Stateful;

[StorageProvider(ProviderName = "DefaultStorage")]
public class UserGrain : Grain<User>, IUserGrain
{
    private readonly IEncryptionService _encryptionService;
    //private readonly IPersistentState<User> _userState;
    public UserGrain(IEncryptionService encryptionService//,
                                                         //[PersistentState("user", "DefaultStorage")] IPersistentState<User> userState
        )
    {
        _encryptionService = encryptionService;
        //_userState = userState;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await ReadStateAsync();
        if (State == null)
        {
            Log.Information("No existing state found for user {UserId}", this.GetPrimaryKey());
            State = new User();
        }
    }

    public async Task<User?> GetUserAsync()
    {
        return await Task.FromResult(State);
    }

    public async Task<bool> LoginAsync(LoginDto loginDto)
    {
        var decryptedPassword = _encryptionService.Decrypt(State.Password, State.IV);
        if (decryptedPassword != loginDto.Password)
            throw new Exception("The username or password is incorrect!");

        return true;
    }

    public async Task AddUserAsync(CreateUserDto userDto)
    {
        var user = User.Create(userDto.UserName, userDto.Password, _encryptionService);

        State = user;

        await WriteStateAsync();
    }

    public async Task UpdateUserNameAsync(string newUserName)
    {
        State.SetUserName(newUserName);

        await WriteStateAsync();
    }

    public async Task DeleteCurrentUserAsync()
    {
        await ClearStateAsync();

        await WriteStateAsync();

        DeactivateOnIdle();
    }

    public async Task AddAddressAsync(CreateAddressDto addressDto)
    {
        var address = Address.Create(State.Id, addressDto.City, addressDto.Street, addressDto.Alley, addressDto.Plaque);

        if (State.Addresses.Any(x => x.Equals(address)))
            throw new Exception("Duplicate address!");

        State.Addresses.Add(address);

        await WriteStateAsync();
    }

    public async Task<List<Address>> GetAddressesAsync()
    {
        return await Task.FromResult(State.Addresses);
    }

    public async Task UpdateAddressAsync(UpdateAddressDto updateAddressDto)
    {
        var address = State.Addresses.SingleOrDefault(x => x.Id == updateAddressDto.addressId);

        if (address is null)
            throw new Exception("The address not found!");

        address.SetCity(updateAddressDto.City);
        address.SetStreet(updateAddressDto.Street);
        address.SetAlley(updateAddressDto.Alley);
        address.SetPlaque(updateAddressDto.Plaque);

        await WriteStateAsync();
    }

    public async Task DeleteAddressAsync(DeleteAddressDto deleteAddressDto)
    {
        var address = State.FindAddress(deleteAddressDto.City, deleteAddressDto.Street, deleteAddressDto.Alley, deleteAddressDto.Plaque);

        if (address is null)
            return;

        State.Addresses.Remove(address);

        await WriteStateAsync();
    }
}

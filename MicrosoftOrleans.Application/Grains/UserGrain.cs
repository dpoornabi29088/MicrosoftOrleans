using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;

namespace MicrosoftOrleans.Infrastructure.Grains;

[StorageProvider(ProviderName = "DefaultStorage")]
public class UserGrain : Grain<User>, IUserGrain
{
    private readonly IUserRepository _userRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IPersistentState<User> _userState;
    private readonly IEncryptionService _encryptionService;

    public UserGrain([PersistentState("user", "DefaultStorage")] IPersistentState<User> userState,
        IUserRepository userRepository,
        IAddressRepository addressRepository,
        IEncryptionService encryptionService)
    {
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _userState = userState;
        _encryptionService = encryptionService;
    }

    public async override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);

        var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        if (user == null)
        {
            try
            {
                DeactivateOnIdle();
                return;
            }
            catch (Exception)
            {

            }

        }

        _userState.State = user;
    }

    public async Task<User?> GetUserAsync()
    {
        var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        if (user is null)
            throw new Exception("The user not found");

        return user;
    }

    public async Task<bool> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        if (user is null)
            throw new Exception("The user not found!");

        var decryptedPassword = _encryptionService.Decrypt(user.Password, user.IV);

        if (decryptedPassword != loginDto.Password)
            throw new Exception("The username or password is incorrect!");

        return true;
    }

    public async Task AddUserAsync(CreateUserDto userDto)
    {
        var user = User.Create(userDto.UserName, userDto.Password, _encryptionService);

        await _userRepository.AddAsync(user);

        await _userRepository.SaveChangesAsync();

        _userState.State = user;

        await WriteStateAsync();
    }

    public async Task ChangeUserNameAsync(string newUserName)
    {
        var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        if (user is null)
        {
            throw new Exception("User not found!");
        }

        user.SetUserName(newUserName);

        await _userRepository.SaveChangesAsync();

        _userState.State = user;

        await WriteStateAsync();
    }

    public async Task DeleteUserAsync()
    {
        var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        if (user is null)
            return;

        _userRepository.Remove(user);

        await _userRepository.SaveChangesAsync();

        await _userState.ClearStateAsync();

        await WriteStateAsync();

        DeactivateOnIdle();
    }

    public async Task AddAddressAsync(CreateAddressDto addressDto)
    {
        var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        var address = Address.Create(user.Id, addressDto.City, addressDto.Street, addressDto.Alley, addressDto.Plaque);

        if (_userState.State.Addresses.Any(x => x.Equals(address)))
            throw new Exception("Duplicate address!");

        await _addressRepository.AddAsync(address);

        await _addressRepository.SaveChangesAsync();

        _userState.State.Addresses.Add(address);

        await WriteStateAsync();
    }

    //public async Task<List<Address>> GetAddressesAsync()
    //{
    //    return State.Addresses;
    //}


    //public async Task UpdateAddressAsync(Address address)
    //{
    //    await _addressRepository.Update(address);

    //    var existingAddress = State.Addresses.Find(a => a.Id == address.Id);

    //    if (existingAddress != null)
    //    {
    //        existingAddress.Street = address.Street;
    //        existingAddress.City = address.City;
    //    }

    //    await WriteStateAsync();
    //}

    //public async Task DeleteAddressAsync(int addressId)
    //{
    //    await _addressRepository.Remove(addressId);

    //    State.Addresses.RemoveAll(a => a.Id == addressId);

    //    await WriteStateAsync();
    //}
}

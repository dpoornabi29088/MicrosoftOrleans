using Microsoft.EntityFrameworkCore;
using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;
using System.Text;
using System.Text.Json;

namespace MicrosoftOrleans.Application.Grains.Stateful;

[StorageProvider(ProviderName = "DefaultStorage")]
public class UserGrain : Grain<User>, IUserGrain
{
    private readonly IUserRepository _userRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IPersistentState<User> _userState;
    private readonly IEncryptionService _encryptionService;
    private readonly IOrleansDbContext _orleansDbContext;

    public UserGrain([PersistentState("user", "DefaultStorage")] IPersistentState<User> userState,
        IUserRepository userRepository,
        IAddressRepository addressRepository,
        IEncryptionService encryptionService,
        IOrleansDbContext orleansDbContext)
    {
        _userRepository = userRepository;
        _addressRepository = addressRepository;
        _userState = userState;
        _encryptionService = encryptionService;
        _orleansDbContext = orleansDbContext;
    }

    public async override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);

        //var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        //if (user == null)
        //    return;

        //_userState.State = user;
        var user = await GetLatestStateAsync(this.GetPrimaryKeyString());

        if (user is not null)
            _userState.State = user;

    }

    public async Task<User?> GetLatestStateAsync(string grainIdExtension)
    {
        var entity = await _orleansDbContext.OrleansStorages
            .Where(e => e.GrainIdExtensionString == grainIdExtension)
            .OrderByDescending(e => e.ModifiedOn)
            .FirstOrDefaultAsync();

        if (entity != null && entity.PayloadBinary != null)
        {
            var json = (Encoding.UTF8.GetString(entity.PayloadBinary))
           .Replace("\"Type\"", "\"IgnoreType\"") // Removes type metadata
    .Replace("\"$values\"", "\"Values\""); // Corrects empty collections




            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var user = System.Text.Json.JsonSerializer.Deserialize<User>(json, options);


            //var user = JsonConvert.DeserializeObject<User>(json);
            return user;
        }

        return null;
    }



    public async Task<User?> GetUserAsync()
    {
        //var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        //if (user is null)
        //    throw new Exception("The user not found");

        //return user;

        return await Task.FromResult(_userState.State);
    }

    public async Task<bool> LoginAsync(LoginDto loginDto)
    {
        //var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        //if (user is null)
        //    throw new Exception("The user not found!");

        //var decryptedPassword = _encryptionService.Decrypt(user.Password, user.IV);
        var decryptedPassword = _encryptionService.Decrypt(_userState.State.Password, _userState.State.IV);
        if (decryptedPassword != loginDto.Password)
            throw new Exception("The username or password is incorrect!");

        return true;
    }

    public async Task AddUserAsync(CreateUserDto userDto)
    {
        var user = User.Create(userDto.UserName, userDto.Password, _encryptionService);

        //await _userRepository.AddAsync(user);

        //await _userRepository.SaveChangesAsync();

        _userState.State = user;

        State = user;

        await WriteStateAsync();
    }

    public async Task UpdateUserNameAsync(string newUserName)
    {
        // var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        //if (user is null)
        //{
        //    throw new Exception("User not found!");
        //}

        //user.SetUserName(newUserName);

        // await _userRepository.SaveChangesAsync();

        //_userState.State = user;

        _userState.State.SetUserName(newUserName);

        await WriteStateAsync();
    }

    public async Task DeleteCurrentUserAsync()
    {
        //var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        //if (user is null)
        //    return;

        //_userRepository.Remove(user);

        //await _userRepository.SaveChangesAsync();

        await _userState.ClearStateAsync();

        await WriteStateAsync();

        DeactivateOnIdle();
    }

    public async Task AddAddressAsync(CreateAddressDto addressDto)
    {
        //var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

        //var address = Address.Create(user.Id, addressDto.City, addressDto.Street, addressDto.Alley, addressDto.Plaque);

        //if (_userState.State.Addresses.Any(x => x.Equals(address)))
        //    throw new Exception("Duplicate address!");

        //await _addressRepository.AddAsync(address);

        //await _addressRepository.SaveChangesAsync();

        var address = Address.Create(_userState.State.Id, addressDto.City, addressDto.Street, addressDto.Alley, addressDto.Plaque);

        if (_userState.State.Addresses.Any(x => x.Equals(address)))
            throw new Exception("Duplicate address!");

        _userState.State.Addresses.Add(address);

        await WriteStateAsync();
    }

    public async Task<List<Address>> GetAddressesAsync()
    {
        return await Task.FromResult(_userState.State.Addresses);
    }


    public async Task UpdateAddressAsync(UpdateAddressDto updateAddressDto)
    {
        //await _addressRepository.Update(address);

        //var existingAddress = State.Addresses.Find(a => a.Id == address.Id);

        //if (existingAddress != null)
        //{
        //    existingAddress.Street = address.Street;
        //    existingAddress.City = address.City;
        //}


        var address = _userState.State.Addresses.SingleOrDefault(x => x.Id == updateAddressDto.addressId);

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
        //await _addressRepository.Remove(addressId);
        var address = _userState.State.FindAddress(deleteAddressDto.City, deleteAddressDto.Street, deleteAddressDto.Alley, deleteAddressDto.Plaque);

        if (address is null)
            return;

        State.Addresses.Remove(address);

        await WriteStateAsync();
    }
}

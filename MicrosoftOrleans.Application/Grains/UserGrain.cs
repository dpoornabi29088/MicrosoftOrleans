using MicrosoftOrleans.Application.Common.Interfaces;
using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;
using Orleans;
using Orleans.Providers;
using Orleans.Runtime;

namespace MicrosoftOrleans.Infrastructure.Grains
{
    [StorageProvider(ProviderName = "DefaultStorage")]
    public class UserGrain : Grain<User>, IUserGrain
    {
        private readonly IUserRepository _userRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IPersistentState<List<User>> _userStates;
        private readonly IEncryptionService _encryptionService;

        public UserGrain([PersistentState("user", "DefaultStorage")] IPersistentState<List<User>> userStates,
            IUserRepository userRepository,
            IAddressRepository addressRepository,
            IEncryptionService encryptionService)
        {
            _userRepository = userRepository;
            _addressRepository = addressRepository;
            _userStates = userStates;
            _encryptionService = encryptionService;
        }

        public async override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);

            if (_userStates.State is null || !_userStates.State.Any())
            {
                _userStates.State = await _userRepository.ToListAsync();
            }
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
            var user = await _userRepository.FindByUserNameAsync(loginDto.UserName);

            if (user is null)
                throw new Exception("The user not found!");

            var decryptedPassword = _encryptionService.Decrypt(user.Password, user.IV);

            if (decryptedPassword != loginDto.Password)
                throw new Exception("The username or password is incorrect!");

            return true;
        }

        public async Task AddUserAsync(CreateUserDto userDto)
        {
            if (_userStates.State.Any(x => string.Compare(x.UserName, userDto.UserName, true) == 0))
            {
                throw new Exception("Duplicate user!");
            }
            var user = User.Create(userDto.UserName, userDto.Password, _encryptionService);

            await _userRepository.AddAsync(user);

            await _userRepository.SaveChangesAsync();

            State = user;

            _userStates.State.Add(user);

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

            State = user;

            await WriteStateAsync();
        }

        public async Task DeleteUserAsync()
        {
            var user = await _userRepository.FindByUserNameAsync(this.GetPrimaryKeyString());

            if (user is null)
                return;

            _userRepository.Remove(user);

            await _userRepository.SaveChangesAsync();

            var index = _userStates.State.FindIndex(x => x.Id == user.Id);

            if (index != -1)
                _userStates.State.RemoveAt(index);

            await WriteStateAsync();
        }

        //public async Task<List<Address>> GetAddressesAsync()
        //{
        //    return State.Addresses;
        //}

        //public async Task AddAddressAsync(Address address)
        //{
        //    address.UserId = (int)this.GetPrimaryKeyLong();

        //    await _addressRepository.AddAsync(address);

        //    State.Addresses.Add(address);

        //    await WriteStateAsync();
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
}

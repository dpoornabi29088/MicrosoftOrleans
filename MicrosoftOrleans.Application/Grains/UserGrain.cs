using MicrosoftOrleans.Application.DTOs;
using MicrosoftOrleans.Application.Interfaces;
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

        public UserGrain([PersistentState("user", "DefaultStorage")] IPersistentState<List<User>> userStates,
            IUserRepository userRepository,
            IAddressRepository addressRepository)
        {
            _userRepository = userRepository;
            _addressRepository = addressRepository;
            _userStates = userStates;
        }

        public async override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await base.OnActivateAsync(cancellationToken);

            if (_userStates.State is null || !_userStates.State.Any())
            {
                _userStates.State = await _userRepository.ToListAsync();
            }
        }

        public async Task<User?> GetUserAsync() => await _userRepository.SingleAsync((int)this.GetPrimaryKeyLong());

        public async Task AddUserAsync(CreateUserDto user)
        {
            if (_userStates.State.Any(x => string.Compare(x.UserName, user.UserName, true) == 0))
            {
                throw new Exception("Duplicate user!");
            }
            await _userRepository.AddAsync(user);

            await _userRepository.SaveChangesAsync();

            State = user;

            _userStates.State.Add(user);

            await WriteStateAsync();
        }

        public async Task UpdateUsernameAsync(string userName)
        {
            var user = await _userRepository.FindAsync((int)this.GetPrimaryKeyLong());

            if (user is null)
            {
                throw new Exception("User not found!");
            }
            user.UserName = userName;

            await _userRepository.SaveChangesAsync();

            State = user;

            await WriteStateAsync();
        }

        public async Task DeleteUserAsync()
        {
            int id = (int)this.GetPrimaryKeyLong();

            await _userRepository.Remove(id);

            await _userRepository.SaveChangesAsync();

            var index = _userStates.State.FindIndex(x => x.Id == id);

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

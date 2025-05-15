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
        private readonly IPersistentState<User> _userState;

        public UserGrain([PersistentState("user", "DefaultStorage")] IPersistentState<User> userState,
            IUserRepository userRepository,
            IAddressRepository addressRepository)
        {
            _userRepository = userRepository;
            _addressRepository = addressRepository;
            _userState = userState;
        }

        public async Task<User> GetUserAsync()
        {
            State = await _userRepository.GetUserAsync((int)this.GetPrimaryKeyLong());
            return State;
        }

        public async Task AddUserAsync(User user)
        {
            await _userRepository.AddUserAsync(user);
            State = user;
            await WriteStateAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateUserAsync(user);
            State = user;
            await WriteStateAsync();
        }

        public async Task DeleteUserAsync()
        {
            await _userRepository.DeleteUserAsync((int)this.GetPrimaryKeyLong());
            State = null;
            await WriteStateAsync();
        }

        public async Task<List<Address>> GetAddressesAsync()
        {
            return State.Addresses;
        }

        public async Task AddAddressAsync(Address address)
        {
            address.UserId = (int)this.GetPrimaryKeyLong();
            await _addressRepository.AddAddressAsync(address);
            State.Addresses.Add(address);
            await WriteStateAsync();
        }

        public async Task UpdateAddressAsync(Address address)
        {
            await _addressRepository.UpdateAddressAsync(address);
            var existingAddress = State.Addresses.Find(a => a.Id == address.Id);
            if (existingAddress != null)
            {
                existingAddress.Street = address.Street;
                existingAddress.City = address.City;
            }
            await WriteStateAsync();
        }

        public async Task DeleteAddressAsync(int addressId)
        {
            await _addressRepository.DeleteAddressAsync(addressId);
            State.Addresses.RemoveAll(a => a.Id == addressId);
            await WriteStateAsync();
        }
    }
}

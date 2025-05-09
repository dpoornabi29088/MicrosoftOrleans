using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserAsync(int userId);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId);
}

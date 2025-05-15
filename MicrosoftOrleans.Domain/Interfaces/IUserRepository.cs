using MicrosoftOrleans.Domain.Entities;

namespace MicrosoftOrleans.Domain.Interfaces;

public interface IUserRepository
{
    Task<List<User>> ToListAsync();
    Task<User?> FindAsync(int userId);
    Task<User?> SingleAsync(int userId);
    Task AddAsync(User user);
    Task Update(User user);
    Task Remove(int userId);
    Task SaveChangesAsync();
}

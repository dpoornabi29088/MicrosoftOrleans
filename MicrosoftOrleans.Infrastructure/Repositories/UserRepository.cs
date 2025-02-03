using Microsoft.EntityFrameworkCore;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;

public class UserRepository : IUserRepository
{
    private readonly DbContext _context;

    public UserRepository(DbContext context)
    {
        _context = context;
    }

    public async Task<User> GetUserAsync(long userId)
    {
        return await _context.Set<User>().Include(u => u.Addresses).FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Set<User>().AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserAsync(User user)
    {
        _context.Set<User>().Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(long userId)
    {
        var user = await GetUserAsync(userId);
        if (user != null)
        {
            _context.Set<User>().Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
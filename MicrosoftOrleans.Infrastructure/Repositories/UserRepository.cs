using Microsoft.EntityFrameworkCore;
using MicrosoftOrleans.Domain.Entities;
using MicrosoftOrleans.Domain.Interfaces;
using MicrosoftOrleans.Infrastructure.Persistence;

namespace MicrosoftOrleans.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> ToListAsync()
    {
        return await _context.Users
            .Include(u => u.Addresses)
            .ToListAsync();

    }

    public async Task<User?> FindAsync(int userId) => await _context.Users.FindAsync(userId);

    public async Task<User?> FindByUserNameAsync(string userName) =>
        await _context.Users.FirstOrDefaultAsync(x => x.UserName == userName);

    public async Task<User?> SingleAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.Addresses)
            .SingleAsync(u => u.Id == userId);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task Remove(int userId)
    {
        var user = await SingleAsync(userId);
        if (user != null)
        {
            _context.Users.Remove(user);
        }
    }

    public void Remove(User user)
    {
        _context.Users.Remove(user);
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
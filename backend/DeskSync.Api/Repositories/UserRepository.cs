
using DeskSync.Api.Data;
using DeskSync.Api.Entities;
using DeskSync.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeskSync.Api.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    private readonly AppDbContext _context = context;

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<UserLogin?> GetUserLoginAsync(ProviderType provider, string providerKey)
    {
        return await _context.UserLogins
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Provider == provider && l.ProviderKey == providerKey);
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<User?> GetUserByPasswordResetTokenAsync(string token)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.PasswordResetToken == token);
    }

    public async Task<User?> GetUserByEmailConfirmationTokenAsync(string token)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.EmailConfirmationToken == token);
    }

    public async Task<bool> IsEmailUnique(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Email == email) == null;
    }

    public async Task<bool> IsUsernameUnique(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Username == username) == null;
    }

    public async Task AddUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task AddUserLoginAsync(UserLogin login)
    {
        await _context.UserLogins.AddAsync(login);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var affectedRows = await _context.Users
                                         .Where(user => user.Id == id)
                                         .ExecuteDeleteAsync();
        
        return affectedRows > 0;
    }

    public async Task<IReadOnlyList<ProviderType>> GetLinkedProvidersAsync(Guid userId)
    {
        return await _context.UserLogins
            .AsNoTracking()
            .Where(l => l.UserId == userId)
            .Select(l => l.Provider)
            .ToListAsync();
    }

    public async Task<UserLogin?> GetUserLoginByProviderAndUserAsync(ProviderType provider, Guid userId)
        => await _context.UserLogins
            .FirstOrDefaultAsync(l => l.Provider == provider && l.UserId == userId);

    public async Task DeleteUserLoginAsync(ProviderType provider, Guid userId)
        => await _context.UserLogins
            .Where(l => l.Provider == provider && l.UserId == userId)
            .ExecuteDeleteAsync();

    public async Task<int> CountLinkedProvidersAsync(Guid userId)
        => await _context.UserLogins
            .CountAsync(l => l.UserId == userId);

    public async Task<bool> HasPasswordAsync(Guid userId)
        => await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.PasswordHash != null)
            .FirstOrDefaultAsync();

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync(); 
}

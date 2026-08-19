
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

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Username == username);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(user => user.Email == email);
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

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var affectedRows = await _context.Users
                                         .Where(user => user.Id == id)
                                         .ExecuteDeleteAsync();
        
        return affectedRows > 0;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync(); 
}

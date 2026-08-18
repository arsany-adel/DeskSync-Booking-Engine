using DeskSync.Api.Entities;

namespace DeskSync.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid id);
    Task<bool> IsEmailUnique(string email);
    Task<bool> IsUsernameUnique(string username);
    Task<User?> GetUserByUsernameAsync(string username);
    Task AddUserAsync(User user);
    Task<bool> DeleteUserAsync(Guid id);
    Task SaveChangesAsync();    
}

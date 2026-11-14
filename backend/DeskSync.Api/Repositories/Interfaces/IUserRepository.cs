using DeskSync.Api.Entities;

namespace DeskSync.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByIdAsync(Guid id);
    Task<UserLogin?> GetUserLoginAsync(ProviderType provider, string providerKey);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByPasswordResetTokenAsync(string token);
    Task<User?> GetUserByEmailConfirmationTokenAsync(string token);
    Task<bool> IsEmailUnique(string email);
    Task<bool> IsUsernameUnique(string username);
    Task AddUserAsync(User user);
    Task AddUserLoginAsync(UserLogin login);
    Task<bool> DeleteUserAsync(Guid id);
    Task<IReadOnlyList<ProviderType>> GetLinkedProvidersAsync(Guid userId);
    Task<UserLogin?> GetUserLoginByProviderAndUserAsync(ProviderType provider, Guid userId);
    Task DeleteUserLoginAsync(ProviderType provider, Guid userId);
    Task<int> CountLinkedProvidersAsync(Guid userId);
    Task<bool> HasPasswordAsync(Guid userId);
    Task SaveChangesAsync();
}
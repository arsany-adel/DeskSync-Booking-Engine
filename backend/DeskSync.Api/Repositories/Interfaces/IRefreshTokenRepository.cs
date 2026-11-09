using DeskSync.Api.Entities;
using NodaTime;

namespace DeskSync.Api.Repositories.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken token);
    Task RevokeAllForUserAsync(Guid userId, Instant revokedAt);
    Task SaveChangesAsync();
}
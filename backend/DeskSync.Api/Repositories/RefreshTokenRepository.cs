using DeskSync.Api.Data;
using DeskSync.Api.Entities;
using DeskSync.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DeskSync.Api.Repositories;

public class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    private readonly AppDbContext _context = context;

    public async Task<RefreshToken?> GetByTokenAsync(string token) =>
        await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token);

    public async Task AddAsync(RefreshToken token) =>
        await _context.RefreshTokens.AddAsync(token);

    public async Task RevokeAllForUserAsync(Guid userId, Instant revokedAt) =>
        await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, revokedAt));

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
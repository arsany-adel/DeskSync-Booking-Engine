using DeskSync.Api.Data;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.Entities;
using DeskSync.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeskSync.Api.Repositories;

public class NotificationRepository(AppDbContext context) : INotificationRepository
{
    private readonly AppDbContext _context = context;

    public async Task<PagedResult<Notification>> GetAllUserNotificationsAsync(
        Guid userId,
        int pageNumber,
        int itemsPerPage,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToListAsync(cancellationToken);

        return new PagedResult<Notification>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            ItemsPerPage = itemsPerPage,
        };
    }

    public async Task<PagedResult<Notification>> GetUnreadUserNotificationsAsync(
        Guid userId,
        int pageNumber,
        int itemsPerPage,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToListAsync(cancellationToken);

        return new PagedResult<Notification>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            ItemsPerPage = itemsPerPage,
        };
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public async Task AddRangeAsync(
        IEnumerable<Notification> notifications,
        CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
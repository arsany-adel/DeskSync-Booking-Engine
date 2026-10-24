using DeskSync.Api.DTOs.Common;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<PagedResult<Notification>> GetAllUserNotificationsAsync(
        Guid userId,
        int pageNumber,
        int itemsPerPage,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Notification>> GetUnreadUserNotificationsAsync(
        Guid userId,
        int pageNumber,
        int itemsPerPage,
        CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid id, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
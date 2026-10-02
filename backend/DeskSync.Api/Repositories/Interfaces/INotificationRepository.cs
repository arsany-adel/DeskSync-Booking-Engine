using DeskSync.Api.Entities;

namespace DeskSync.Api.Repositories.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyList<Notification>> GetAllUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Notification>> GetUnreadUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
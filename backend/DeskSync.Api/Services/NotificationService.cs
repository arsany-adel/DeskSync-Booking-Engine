using DeskSync.Api.Constants;
using DeskSync.Api.DTOs;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using ErrorOr;

namespace DeskSync.Api.Services;

public class NotificationService(INotificationRepository notificationRepository) : INotificationService
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<ErrorOr<PagedResult<NotificationResponseDto>>> GetAllUserNotificationsAsync(
        Guid userId, 
        int pageNumber, 
        int itemsPerPage, 
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationRepository.GetAllUserNotificationsAsync(
            userId, pageNumber, itemsPerPage, cancellationToken);
            
        return result.MapToPagedDto();
    }

    public async Task<ErrorOr<PagedResult<NotificationResponseDto>>> GetUnreadUserNotificationsAsync(
        Guid userId, 
        int pageNumber, 
        int itemsPerPage, 
        CancellationToken cancellationToken = default)
    {
        var result = await _notificationRepository.GetUnreadUserNotificationsAsync(
            userId, pageNumber, itemsPerPage, cancellationToken);
            
        return result.MapToPagedDto();
    }

    public async Task<ErrorOr<Success>> MarkAsReadAsync(
    Guid notificationId,
    Guid userId,
    CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId, cancellationToken);

        if (notification == null)
        {
            return DomainErrors.Notification.NotFound;
        }

        if (notification.UserId != userId)
        {
            return DomainErrors.Notification.Unauthorized;
        }

        if (!notification.IsRead)
        {
            // Feedback #5: atomic UPDATE instead of read-modify-write.
            await _notificationRepository.MarkAsReadAsync(notificationId, cancellationToken);
        }

        return Result.Success;
    }
}
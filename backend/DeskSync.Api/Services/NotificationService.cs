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
        int itemsPerPage)
    {
        var result = await _notificationRepository.GetAllUserNotificationsAsync(
            userId, pageNumber, itemsPerPage);

        return result.MapToPagedDto();
    }

    public async Task<ErrorOr<PagedResult<NotificationResponseDto>>> GetUnreadUserNotificationsAsync(
        Guid userId,
        int pageNumber,
        int itemsPerPage)
    {
        var result = await _notificationRepository.GetUnreadUserNotificationsAsync(
            userId, pageNumber, itemsPerPage);

        return result.MapToPagedDto();
    }

    public async Task<ErrorOr<Success>> MarkAsReadAsync(
        Guid notificationId,
        Guid userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);

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
            notification.IsRead = true;

            await _notificationRepository.SaveChangesAsync();
        }

        return Result.Success;
    }
}
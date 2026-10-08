using DeskSync.Api.DTOs;
using DeskSync.Api.DTOs.Common;
using ErrorOr;

namespace DeskSync.Api.Services.Interfaces;

public interface INotificationService
{
    Task<ErrorOr<PagedResult<NotificationResponseDto>>> GetAllUserNotificationsAsync(
        Guid userId,
        int pageNumber,
        int itemsPerPage);

    Task<ErrorOr<PagedResult<NotificationResponseDto>>> GetUnreadUserNotificationsAsync(
        Guid userId,
        int pageNumber,
        int itemsPerPage);

    Task<ErrorOr<Success>> MarkAsReadAsync(Guid notificationId, Guid userId);
}
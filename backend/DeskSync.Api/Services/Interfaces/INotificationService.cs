using DeskSync.Api.DTOs;
using ErrorOr;

namespace DeskSync.Api.Services.Interfaces;

// INotificationService.cs
public interface INotificationService
{
    Task<ErrorOr<IReadOnlyList<NotificationResponseDto>>> GetAllUserNotificationsAsync(Guid userId);
    Task<ErrorOr<IReadOnlyList<NotificationResponseDto>>> GetUnreadUserNotificationsAsync(Guid userId);
    Task<ErrorOr<Success>> MarkAsReadAsync(Guid notificationId, Guid userId);
}
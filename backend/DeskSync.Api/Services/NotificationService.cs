using DeskSync.Api.DTOs;
using DeskSync.Api.Entities;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using ErrorOr;

namespace DeskSync.Api.Services;

// NotificationService.cs
public class NotificationService(INotificationRepository notificationRepository) : INotificationService
{
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<ErrorOr<IReadOnlyList<NotificationResponseDto>>> GetAllUserNotificationsAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetAllUserNotificationsAsync(userId);
        return MapToDtoList(notifications);
    }

    public async Task<ErrorOr<IReadOnlyList<NotificationResponseDto>>> GetUnreadUserNotificationsAsync(Guid userId)
    {
        var notifications = await _notificationRepository.GetUnreadUserNotificationsAsync(userId);
        return MapToDtoList(notifications);
    }

    public async Task<ErrorOr<Success>> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        
        if (notification == null)
        {
            return Error.NotFound(
                code: "Notification.NotFound", 
                description: "Notification not found.");
        }

        if (notification.UserId != userId)
        {
            return Error.Unauthorized(
                code: "Notification.Unauthorized", 
                description: "You do not have permission to modify this notification.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _notificationRepository.SaveChangesAsync();
        }

        return Result.Success;
    }

    private static List<NotificationResponseDto> MapToDtoList(IEnumerable<Notification> notifications)
    {
        return notifications.Select(n => new NotificationResponseDto(
            n.Id,
            n.Title,
            n.Message,
            n.Type.ToString(),
            n.IsRead,
            n.CreatedAt
        )).ToList();
    }
}
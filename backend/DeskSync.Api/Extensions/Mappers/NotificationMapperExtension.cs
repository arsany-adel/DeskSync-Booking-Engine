using DeskSync.Api.DTOs;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Extensions.Mappers;

public static class NotificationMapperExtension
{
    public static NotificationResponseDto MapToDto(this Notification notification)
    {
        return new NotificationResponseDto(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Type.ToString(),
            notification.IsRead,
            notification.CreatedAt
        );
    }

    public static List<NotificationResponseDto> MapToDtoList(
        this IReadOnlyList<Notification> notifications)
    {
        return notifications
            .Select(n => n.MapToDto())
            .ToList();
    }

    public static PagedResult<NotificationResponseDto> MapToPagedDto(
        this PagedResult<Notification> source)
    {
        return new PagedResult<NotificationResponseDto>
        {
            Items = source.Items.MapToDtoList(),
            TotalCount = source.TotalCount,
            PageNumber = source.PageNumber,
            ItemsPerPage = source.ItemsPerPage,
        };
    }
}
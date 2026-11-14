using System.ComponentModel.DataAnnotations;
using DeskSync.Api.Constants;

namespace DeskSync.Api.DTOs.Notifications;

public record NotificationSearchQueryDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")]
    public int PageNumber { get; init; } = PaginationConstants.DefaultPageNumber;

    [Range(1, PaginationConstants.MaxPageSize, ErrorMessage = "Page size must be between 1 and 50.")]
    public int PageSize { get; init; } = PaginationConstants.DefaultPageSize;
}
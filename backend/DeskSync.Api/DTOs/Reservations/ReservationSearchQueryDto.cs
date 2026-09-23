using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using DeskSync.Api.Constants;

namespace DeskSync.Api.DTOs.Reservations;

public record ReservationSearchQueryDto
{
    public Guid? RoomId { get; init; }
    public Guid? UserId { get; init; }
    public LocalDateTime? StartDate { get; init; }
    public LocalDateTime? EndDate { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1.")]
    public int PageNumber { get; init; } = 1;

    [Range(1, PaginationConstants.MaxPageSize, ErrorMessage = "Page size must be between 1 and 50.")]
    public int PageSize { get; init; } = PaginationConstants.DefaultPageSize;
}
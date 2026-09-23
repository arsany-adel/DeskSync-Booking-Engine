using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace DeskSync.Api.DTOs.Reservations;

public record ReservationResponseDto(
    Guid Id,
    Guid UserId,
    Guid RoomId,
    LocalDateTime LocalStartTime,
    LocalDateTime LocalEndTime,
    string TimezoneId,
    Instant CreatedAt,
    string? Notes
);

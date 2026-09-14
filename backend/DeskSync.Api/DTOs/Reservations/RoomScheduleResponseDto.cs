using System.ComponentModel.DataAnnotations;
using NodaTime;

namespace DeskSync.Api.DTOs.Reservations;

public record RoomScheduleResponseDto(
    Guid ReservationId,
    LocalDateTime LocalStartTime,
    LocalDateTime LocalEndTime,
    string TimezoneId
);

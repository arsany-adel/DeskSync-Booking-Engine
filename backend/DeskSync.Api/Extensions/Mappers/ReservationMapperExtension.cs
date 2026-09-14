using DeskSync.Api.DTOs.Reservations;
using DeskSync.Api.Entities;
using NodaTime;

namespace DeskSync.Api.Extensions.Mappers;

public static class ReservationMapperExtension
{
    public static Reservation MapToEntity(
        this CreateReservationDto dto,
        Guid id,
        Guid userId,
        IDateTimeZoneProvider tzProvider,
        IClock clock
    )
    {
        return new Reservation(
            id,
            dto.RoomId,
            userId,
            dto.LocalStartTime,
            dto.LocalEndTime,
            dto.TimezoneId,
            tzProvider,
            clock,
            dto.Notes
        );
    }

    public static ReservationResponseDto ToReservationResponseDto(this Reservation reservation)
    {
        return new ReservationResponseDto(
            reservation.Id,
            reservation.UserId,
            reservation.RoomId,
            reservation.LocalStartTime,
            reservation.LocalEndTime,
            reservation.TimezoneId,
            reservation.CreatedAt,
            reservation.Notes
        );
    }
}

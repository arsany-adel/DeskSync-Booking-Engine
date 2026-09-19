using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.DTOs.Reservations;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using ErrorOr;
using NodaTime;

namespace DeskSync.Api.Services;

public class ReservationService(
    IReservationRepository reservationRepository,
    IDateTimeZoneProvider tzProvider,
    IClock clock
) : IReservationService
{
    private readonly IReservationRepository _reservationRepository = reservationRepository;
    private readonly IDateTimeZoneProvider _tzProvider = tzProvider; // used for CalculateUtcCaches()
    private readonly IClock _clock = clock; // for unit testing when creating a FakeClock

    private ErrorOr<(Instant UtcStart, Instant UtcEnd)> MapToUtcInstants(
        LocalDateTime localStart,
        LocalDateTime localEnd,
        string timezoneId
    )
    {
        var zone = _tzProvider.GetZoneOrNull(timezoneId);

        if (zone is null)
        {
            return Error.Validation(
                code: "Reservation.InvalidTimezone",
                description: $"Invalid timezone ID: '{timezoneId}'."
            );
        }

        var utcStart = zone.AtLeniently(localStart).ToInstant();
        var utcEnd = zone.AtLeniently(localEnd).ToInstant();

        return (utcStart, utcEnd);
    }

    public async Task<PagedResult<ReservationResponseDto>> SearchReservationAsync(
        Guid? roomId,
        Guid? userId,
        LocalDateTime? startDate,
        LocalDateTime? endDate,
        int pageNumber = PaginationConstants.DefaultPageNumber,
        int itemsPerPage = PaginationConstants.DefaultPageSize
    )
    {
        var result = await _reservationRepository.SearchReservationAsync(
            roomId,
            userId,
            startDate,
            endDate,
            pageNumber,
            itemsPerPage
        );

        return new PagedResult<ReservationResponseDto>
        {
            Items = result.Items.Select(r => r.ToReservationResponseDto()).ToList(),
            TotalCount = result.TotalCount,
            PageNumber = result.PageNumber,
            ItemsPerPage = result.ItemsPerPage,
        };
    }

    public async Task<ErrorOr<ReservationResponseDto>> AdminUpdateReservation(
        Guid reservationId,
        AdminUpdateReservationDto dto
    )
    {
        var reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);

        if (reservation == null)
        {
            return Error.NotFound(
                code: "Reservation.NotFound",
                description: $"Reservation with ID '{reservationId}' was not found."
            );
        }

        if (dto.LocalEndTime <= dto.LocalStartTime)
        {
            return Error.Validation(
                code: "Reservation.InvalidTimeRange",
                description: "End time must be strictly after start time."
            );
        }

        var utcInstantsResult = MapToUtcInstants(
            dto.LocalStartTime,
            dto.LocalEndTime,
            dto.TimezoneId
        );

        if (utcInstantsResult.IsError)
        {
            return utcInstantsResult.Errors;
        }

        var (utcStart, utcEnd) = utcInstantsResult.Value;

        var isAvailableTime = await _reservationRepository.IsRoomAvailableAsync(
            dto.RoomId,
            utcStart,
            utcEnd,
            reservationId
        );

        if (!isAvailableTime)
        {
            return Error.Conflict(
                code: "Reservation.RoomNotAvailable",
                description: "The room is already booked for the selected time slot."
            );
        }

        reservation.AdminUpdate(
            dto.RoomId,
            dto.UserId,
            dto.LocalStartTime,
            dto.LocalEndTime,
            dto.TimezoneId,
            dto.Notes,
            _tzProvider,
            _clock
        );

        await _reservationRepository.SaveChangesAsync();

        return reservation.ToReservationResponseDto();
    }

    public async Task<ErrorOr<ReservationResponseDto>> UpdateReservation(
        Guid reservationId,
        UpdateReservationDto dto,
        Guid userId
    )
    {
        var reservation = await _reservationRepository.GetReservationByIdAsync(reservationId);

        if (reservation == null)
        {
            return Error.NotFound(
                code: "Reservation.NotFound",
                description: $"Reservation with ID '{reservationId}' was not found."
            );
        }

        if (reservation.UserId != userId)
        {
            return Error.Unauthorized(
                code: "Reservation.Unauthorized",
                description: "You can only update your own reservations."
            );
        }

        if (dto.LocalEndTime <= dto.LocalStartTime)
        {
            return Error.Validation(
                code: "Reservation.InvalidTimeRange",
                description: "End time must be strictly after start time."
            );
        }

        var utcInstantsResult = MapToUtcInstants(
            dto.LocalStartTime,
            dto.LocalEndTime,
            dto.TimezoneId
        );

        if (utcInstantsResult.IsError)
        {
            return utcInstantsResult.Errors;
        }

        var (utcStart, utcEnd) = utcInstantsResult.Value;

        var isAvailableTime = await _reservationRepository.IsRoomAvailableAsync(
            reservation.RoomId,
            utcStart,
            utcEnd,
            reservationId
        );

        if (!isAvailableTime)
        {
            return Error.Conflict(
                code: "Reservation.RoomNotAvailable",
                description: "The room is already booked for the selected time slot."
            );
        }

        reservation.UpdateDetails(
            dto.LocalStartTime,
            dto.LocalEndTime,
            dto.TimezoneId,
            dto.Notes,
            _tzProvider,
            _clock
        );

        await _reservationRepository.SaveChangesAsync();

        return reservation.ToReservationResponseDto();
    }

    public async Task<ErrorOr<ReservationResponseDto>> CreateReservation(
        Guid userId,
        CreateReservationDto dto
    )
    {
        if (dto.LocalEndTime <= dto.LocalStartTime)
        {
            return Error.Validation(
                code: "Reservation.InvalidTimeRange",
                description: "End time must be strictly after start time."
            );
        }

        var utcInstantsResult = MapToUtcInstants(
            dto.LocalStartTime,
            dto.LocalEndTime,
            dto.TimezoneId
        );

        if (utcInstantsResult.IsError)
        {
            return utcInstantsResult.Errors;
        }

        var (utcStart, utcEnd) = utcInstantsResult.Value;

        var isAvailableTime = await _reservationRepository.IsRoomAvailableAsync(
            dto.RoomId,
            utcStart,
            utcEnd
        );

        if (!isAvailableTime)
        {
            return Error.Conflict(
                code: "Reservation.RoomNotAvailable",
                description: "The room is already booked for the selected time slot."
            );
        }

        var reservation = dto.MapToEntity(
            id: Guid.NewGuid(),
            userId: userId,
            tzProvider: _tzProvider,
            clock: _clock
        );

        _reservationRepository.AddReservation(reservation);
        await _reservationRepository.SaveChangesAsync();

        return reservation.ToReservationResponseDto();
    }

    public async Task<ErrorOr<bool>> DeleteReservationAsync(Guid reservationId, Guid userId)
    {
        var reservation = await _reservationRepository.GetReservationReadOnlyByIdAsync(
            reservationId
        );

        if (reservation == null)
        {
            return Error.NotFound(
                code: "Reservation.NotFound",
                description: $"Reservation with ID '{reservationId}' was not found."
            );
        }

        if (reservation.UserId != userId)
        {
            return Error.Unauthorized(
                code: "Reservation.Unauthorized",
                description: "You can only delete your own reservations."
            );
        }

        if (reservation.UtcStartTime <= _clock.GetCurrentInstant())
        {
            return Error.Validation(
                code: "Reservation.CannotDeleteStarted",
                description: "Cannot delete a reservation that has already started. Historical records must be preserved."
            );
        }

        await _reservationRepository.DeleteReservationAsync(reservationId);
        return true;
    }

    public async Task<ErrorOr<bool>> AdminDeleteReservationAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetReservationReadOnlyByIdAsync(
            reservationId
        );

        if (reservation == null)
        {
            return Error.NotFound(
                code: "Reservation.NotFound",
                description: $"Reservation with ID '{reservationId}' was not found."
            );
        }

        if (reservation.UtcStartTime <= _clock.GetCurrentInstant())
        {
            return Error.Validation(
                code: "Reservation.CannotDeleteStarted",
                description: "Cannot delete a reservation that has already started. Historical records must be preserved."
            );
        }

        await _reservationRepository.DeleteReservationAsync(reservationId);
        return true;
    }

    public async Task<ErrorOr<ReservationResponseDto>> GetReservationAsync(
        Guid reservationId,
        Guid? currentUserId = null
    )
    {
        var reservation = await _reservationRepository.GetReservationReadOnlyByIdAsync(
            reservationId
        );

        if (reservation == null)
        {
            return Error.NotFound(
                code: "Reservation.NotFound",
                description: $"Reservation with ID '{reservationId}' was not found."
            );
        }

        if (currentUserId.HasValue && reservation.UserId != currentUserId.Value)
        {
            return Error.Unauthorized(
                code: "Reservation.Unauthorized",
                description: "You do not have permission to view this reservation."
            );
        }

        return reservation.ToReservationResponseDto();
    }

    public async Task<IReadOnlyList<RoomScheduleDto>> GetRoomScheduleAsync(
        Guid roomId,
        LocalDateTime startDate,
        LocalDateTime endDate
    )
    {
        var reservations = await _reservationRepository.GetReservationsScheduleByRoomIdAsync(
            roomId,
            startDate,
            endDate
        );

        return reservations
            .Select(r => new RoomScheduleDto(r.LocalStartTime, r.LocalEndTime))
            .ToList();
    }
}

using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.DTOs.Reservations;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
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

    private (Instant UtcStart, Instant UtcEnd) MapToUtcInstants(
        LocalDateTime localStart,
        LocalDateTime localEnd,
        string timezoneId
    )
    {
        var zone =
            _tzProvider.GetZoneOrNull(timezoneId)
            ?? throw new ArgumentException($"Invalid timezone ID: '{timezoneId}'.");

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

    public async Task<ReservationResponseDto> AdminUpdateReservation(
        Guid reservationId,
        AdminUpdateReservationDto dto
    )
    {
        var reservation = await _reservationRepository.GetReservationByIdAsync(
            reservationId
        );

        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID '{reservationId}' was not found.");
        }

        if (dto.LocalEndTime <= dto.LocalStartTime)
        {
            throw new ArgumentException("End time must be strictly after start time.");
        }

        var (utcStart, utcEnd) = MapToUtcInstants(
            dto.LocalStartTime,
            dto.LocalEndTime,
            dto.TimezoneId
        );

        var isAvailableTime = await _reservationRepository.IsRoomAvailableAsync(
            dto.RoomId,
            utcStart,
            utcEnd,
            reservationId
        );

        if (!isAvailableTime)
        {
            throw new InvalidOperationException(
                "The room is already booked for the selected time slot."
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

    public async Task<ReservationResponseDto> UpdateReservation(
        Guid reservationId,
        UpdateReservationDto dto,
        Guid userId
    )
    {
        var reservation = await _reservationRepository.GetReservationByIdAsync(
            reservationId
        );

        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID '{reservationId}' was not found.");
        }

        if (reservation.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own reservations.");
        }

        if (dto.LocalEndTime <= dto.LocalStartTime)
        {
            throw new ArgumentException("End time must be strictly after start time.");
        }

        var (utcStart, utcEnd) = MapToUtcInstants(
            dto.LocalStartTime,
            dto.LocalEndTime,
            dto.TimezoneId
        );

        var isAvailableTime = await _reservationRepository.IsRoomAvailableAsync(
            reservation.RoomId,
            utcStart,
            utcEnd,
            reservationId
        );

        if (!isAvailableTime)
        {
            throw new InvalidOperationException(
                "The room is already booked for the selected time slot."
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

    public async Task<ReservationResponseDto> CreateReservation(
        Guid userId,
        CreateReservationDto dto
    )
    {
        if (dto.LocalEndTime <= dto.LocalStartTime)
        {
            throw new ArgumentException("End time must be strictly after start time.");
        }

        var (utcStart, utcEnd) = MapToUtcInstants(
            dto.LocalStartTime,
            dto.LocalEndTime,
            dto.TimezoneId
        );

        var isAvailableTime = await _reservationRepository.IsRoomAvailableAsync(
            dto.RoomId,
            utcStart,
            utcEnd
        );

        if (!isAvailableTime)
        {
            throw new InvalidOperationException(
                "The room is already booked for the selected time slot."
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

    public async Task DeleteReservationAsync(Guid reservationId, Guid userId)
    {
        var reservation = await _reservationRepository.GetReservationReadOnlyByIdAsync(reservationId);

        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID '{reservationId}' was not found.");
        }

        if (reservation.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only delete your own reservations.");
        }

        if (reservation.UtcStartTime <= _clock.GetCurrentInstant())
        {
            throw new InvalidOperationException(
                "Cannot delete a reservation that has already started."
            );
        }

        await _reservationRepository.DeleteReservationAsync(reservationId);
    }

    public async Task AdminDeleteReservationAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetReservationReadOnlyByIdAsync(reservationId);

        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID '{reservationId}' was not found.");
        }

        if (reservation.UtcStartTime <= _clock.GetCurrentInstant())
        {
            throw new InvalidOperationException(
                "Cannot delete a reservation that has already started. Historical records must be preserved."
            );
        }

        await _reservationRepository.DeleteReservationAsync(reservationId);
    }

    public async Task<ReservationResponseDto> GetReservationAsync(
        Guid reservationId,
        Guid? currentUserId = null
    )
    {
        var reservation = await _reservationRepository.GetReservationReadOnlyByIdAsync(reservationId);

        if (reservation == null)
        {
            throw new KeyNotFoundException($"Reservation with ID '{reservationId}' was not found.");
        }

        if (currentUserId.HasValue && reservation.UserId != currentUserId.Value)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to view this reservation."
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

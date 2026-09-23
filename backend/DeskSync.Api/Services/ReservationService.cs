using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.DTOs.Reservations;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using DeskSync.Api.Entities;
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
    private readonly IDateTimeZoneProvider _tzProvider = tzProvider;
    private readonly IClock _clock = clock;

    private ErrorOr<(Instant UtcStart, Instant UtcEnd)> MapToUtcInstants(
        LocalDateTime localStart,
        LocalDateTime localEnd,
        string timezoneId
    )
    {
        var zone = _tzProvider.GetZoneOrNull(timezoneId);

        if (zone is null)
        {
            return DomainErrors.Reservation.InvalidTimezone(timezoneId);
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
            return DomainErrors.Reservation.NotFound(reservationId);
        }

        if (dto.LocalEndTime <= dto.LocalStartTime)
        {
            return DomainErrors.Reservation.InvalidTimeRange;
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
            return DomainErrors.Reservation.RoomNotAvailable;
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
            return DomainErrors.Reservation.NotFound(reservationId);
        }

        if (reservation.UserId != userId)
        {
            return DomainErrors.General.Unauthorized;
        }

        if (dto.LocalEndTime <= dto.LocalStartTime)
        {
            return DomainErrors.Reservation.InvalidTimeRange;
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
            return DomainErrors.Reservation.RoomNotAvailable;
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
            return DomainErrors.Reservation.InvalidTimeRange;
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
            return DomainErrors.Reservation.RoomNotAvailable;
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
            return DomainErrors.Reservation.NotFound(reservationId);

        if (reservation.UserId != userId)
            return DomainErrors.General.Unauthorized;

        return await ExecuteDeleteAsync(reservation);
    }

    public async Task<ErrorOr<bool>> AdminDeleteReservationAsync(Guid reservationId)
    {
        var reservation = await _reservationRepository.GetReservationReadOnlyByIdAsync(
            reservationId
        );

        if (reservation == null)
            return DomainErrors.Reservation.NotFound(reservationId);

        return await ExecuteDeleteAsync(reservation);
    }

    private async Task<ErrorOr<bool>> ExecuteDeleteAsync(Reservation reservation)
    {
        if (reservation.UtcStartTime <= _clock.GetCurrentInstant())
            return DomainErrors.Reservation.CannotDeleteStarted;

        await _reservationRepository.DeleteReservationAsync(reservation.Id);
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
            return DomainErrors.Reservation.NotFound(reservationId);
        }

        if (currentUserId.HasValue && reservation.UserId != currentUserId.Value)
        {
            return DomainErrors.General.Unauthorized;
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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.DTOs.Reservations;
using NodaTime;
using ErrorOr;

namespace DeskSync.Api.Services.Interfaces;

public interface IReservationService
{
    Task<PagedResult<ReservationResponseDto>> SearchReservationAsync(
        Guid? roomId,
        Guid? userId,
        LocalDateTime? startDate,
        LocalDateTime? endDate,
        int pageNumber = PaginationConstants.DefaultPageNumber,
        int itemsPerPage = PaginationConstants.DefaultPageSize
    );

    Task<ErrorOr<ReservationResponseDto>> AdminUpdateReservation(
        Guid reservationId,
        AdminUpdateReservationDto dto
    );

    Task<ErrorOr<bool>> AdminDeleteReservationAsync(Guid reservationId);

    Task<ErrorOr<ReservationResponseDto>> CreateReservation(Guid userId, CreateReservationDto dto);

    Task<ErrorOr<ReservationResponseDto>> UpdateReservation(Guid reservationId, UpdateReservationDto dto,Guid userId);

    Task<ErrorOr<bool>> DeleteReservationAsync(Guid reservationId, Guid userId);

    Task<ErrorOr<ReservationResponseDto>> GetReservationAsync(
        Guid reservationId,
        Guid? currentUserId = null
    );

    Task<IReadOnlyList<RoomScheduleDto>> GetRoomScheduleAsync(
        Guid roomId,
        LocalDateTime startDate,
        LocalDateTime endDate
    );
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.DTOs.Reservations;
using NodaTime;

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

    Task<ReservationResponseDto> AdminUpdateReservation(
        Guid reservationId,
        AdminUpdateReservationDto dto
    );

    Task AdminDeleteReservationAsync(Guid reservationId);

    Task<ReservationResponseDto> CreateReservation(Guid userId, CreateReservationDto dto);

    Task<ReservationResponseDto> UpdateReservation(Guid reservationId, UpdateReservationDto dto,Guid userId);

    Task DeleteReservationAsync(Guid reservationId, Guid userId);

    Task<ReservationResponseDto> GetReservationAsync(
        Guid reservationId,
        Guid? currentUserId = null
    );

    Task<IReadOnlyList<RoomScheduleDto>> GetRoomScheduleAsync(
        Guid roomId,
        LocalDateTime startDate,
        LocalDateTime endDate
    );
}

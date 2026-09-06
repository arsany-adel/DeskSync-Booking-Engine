using DeskSync.Api.DTOs.Common;
using DeskSync.Api.Constants;
using DeskSync.Api.Entities;
using NodaTime;

namespace DeskSync.Api.Repositories.Interfaces;

public interface IReservationRepository
{
    Task<PagedResult<Reservation>> SearchReservationAsync(
        Guid? roomId,
        Guid? userId,
        LocalDateTime? startDate,
        LocalDateTime? endDate,
        int pageNumber = PaginationConstants.DefaultPageNumber,
        int itemsPerPage = PaginationConstants.DefaultPageSize);

    Task<IReadOnlyList<Reservation>> GetReservationsScheduleByRoomIdAsync(
        Guid roomId, 
        LocalDateTime startDate, 
        LocalDateTime endDate);

    Task<Reservation?> GetReservationByIdAsync(Guid id, bool trackChanges = false);

    Reservation  AddReservation(Reservation reservation);

    Task<bool> DeleteReservationAsync(Guid id);

    Task<bool> IsRoomAvailableAsync(
        Guid roomId, 
        LocalDateTime localStartTime, 
        LocalDateTime localEndTime, 
        Guid? excludeReservationId = null);
    
    Task SaveChangesAsync();
}

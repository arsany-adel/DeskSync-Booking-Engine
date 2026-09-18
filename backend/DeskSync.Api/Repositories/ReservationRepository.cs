using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeskSync.Api.Constants;
using DeskSync.Api.Data;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.Entities;
using DeskSync.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DeskSync.Api.Repositories;

public class ReservationRepository(AppDbContext context) : IReservationRepository
{
    private readonly AppDbContext _context = context;

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task<PagedResult<Reservation>> SearchReservationAsync(
        Guid? roomId,
        Guid? userId,
        LocalDateTime? startDate,
        LocalDateTime? endDate,
        int pageNumber = PaginationConstants.DefaultPageNumber,
        int itemsPerPage = PaginationConstants.DefaultPageSize
    )
    {
        var query = _context.Reservations.AsNoTracking().AsQueryable(); // For Read-Only operations, AsNoTracking() improves performance by disabling change tracking and disabling change of the row retrived from the database.

        if (roomId.HasValue)
        {
            query = query.Where(r => r.RoomId == roomId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(r => r.LocalStartTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(r => r.LocalEndTime <= endDate.Value);
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.LocalStartTime) //OrderByDescending() for making sure when Skip() that Data to be ordered because it may be not ordered and Skip() may not work properly if not ordered.
            .Skip((pageNumber - 1) * itemsPerPage)
            .Take(itemsPerPage)
            .ToListAsync();

        return new PagedResult<Reservation>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            ItemsPerPage = itemsPerPage,
        };
    }

    public async Task<IReadOnlyList<Reservation>> GetReservationsScheduleByRoomIdAsync(
        Guid roomId,
        LocalDateTime startDate,
        LocalDateTime endDate
    )
    {
        return await _context
            .Reservations.AsNoTracking()
            .Where(r => r.RoomId == roomId)
            .Where(r => r.LocalEndTime > startDate && r.LocalStartTime < endDate) // to catch the Reservations that started late (previews Day) night but ends on (current Day) morning.
            .OrderBy(r => r.LocalStartTime)
            .ToListAsync();
    }

    public async Task<Reservation?> GetReservationByIdAsync(Guid id)
    {
        return await _context.Reservations.FindAsync(id);
    }

    public async Task<Reservation?> GetReservationReadOnlyByIdAsync(Guid id)
    {
        return await _context.Reservations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
    }

    public Reservation AddReservation(Reservation reservation)
    {
        _context.Reservations.Add(reservation);

        return reservation;
    }

    public async Task<bool> DeleteReservationAsync(Guid id)
    {
        int deletedRows = await _context.Reservations.Where(r => r.Id == id).ExecuteDeleteAsync();

        return deletedRows > 0;
    }

    public async Task<bool> IsRoomAvailableAsync(
        Guid roomId,
        Instant utcStartTime,
        Instant utcEndTime,
        Guid? excludeReservationId = null
    )
    {
        var query = _context.Reservations.Where(r => r.RoomId == roomId);

        if (excludeReservationId.HasValue) //ingore the current Reservation when checking for Overlap when Updating the Reservation.
        {
            query = query.Where(r => r.Id != excludeReservationId.Value);
        }

        bool hasOverlap =
            await query.AnyAsync //AnyAsync(): add If at least one record that satisfies the condition
            (r => r.UtcEndTime > utcStartTime && r.UtcStartTime < utcEndTime);

        return !hasOverlap;
    }
}

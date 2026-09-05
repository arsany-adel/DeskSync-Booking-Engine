using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeskSync.Api.Data;
using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Entities;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeskSync.Api.Repositories;

public class RoomRepository(AppDbContext context) : IRoomRepository
{
    private readonly AppDbContext _context = context;

    public async Task<RoomResponseDto> CreateRoomAsync(CreateRoomDto dto)
    {
        var room = new Room(
            Guid.NewGuid(),
            dto.WorkspaceId,
            dto.Name,
            dto.Description,
            dto.NoOfChairs,
            dto.PricePerHour,
            dto.Status,
            dto.HasProjector,
            dto.HasBoard,
            dto.RecommendedUse
        );

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return RoomMapperExtension.MapToDto(room);
    }

    public async Task<RoomResponseDto?> GetRoomByIdAsync(Guid id)
    {
        return await _context
            .Rooms.AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => new RoomResponseDto(
                r.Id,
                r.WorkspaceId,
                r.Name,
                r.Description,
                r.NoOfChairs,
                r.PricePerHour,
                r.Status,
                r.HasProjector,
                r.HasBoard,
                r.RecommendedUse
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<RoomResponseDto?> UpdateRoomAsync(Guid id, UpdateRoomDto dto)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
            return null;

        room.UpdateRoom(
            dto.Name,
            dto.Description,
            dto.NoOfChairs,
            dto.PricePerHour,
            dto.Status,
            dto.HasProjector,
            dto.HasBoard,
            dto.RecommendedUse
        );

        await _context.SaveChangesAsync();
        return RoomMapperExtension.MapToDto(room);
    }

    public async Task<bool> DeleteRoomAsync(Guid id)
    {
        int deletedRows = await _context.Rooms.Where(r => r.Id == id).ExecuteDeleteAsync();

        return deletedRows > 0;
    }

    public async Task<IEnumerable<RoomResponseDto>> GetRoomsByWorkspaceAsync(Guid workspaceId)
    {
        return await _context
            .Rooms.AsNoTracking()
            .Where(r => r.WorkspaceId == workspaceId)
            .Select(r => new RoomResponseDto(
                r.Id,
                r.WorkspaceId,
                r.Name,
                r.Description,
                r.NoOfChairs,
                r.PricePerHour,
                r.Status,
                r.HasProjector,
                r.HasBoard,
                r.RecommendedUse
            ))
            .ToListAsync();
    }
}

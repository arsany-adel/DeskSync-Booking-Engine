using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Entities;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using DeskSync.Api.Constants;
using ErrorOr;

namespace DeskSync.Api.Services;

public class RoomService(IRoomRepository roomRepository) : IRoomService
{
    private readonly IRoomRepository _roomRepository = roomRepository;

    public async Task<ErrorOr<RoomResponseDto>> CreateRoomAsync(CreateRoomDto dto)
    {
        var room = dto.MapToEntity(Guid.NewGuid());

        _roomRepository.AddRoom(room);
        await _roomRepository.SaveChangesAsync();

        return room.MapToDto();
    }

    public async Task<ErrorOr<RoomResponseDto?>> GetRoomByIdReadOnlyAsync(Guid id)
    {
        var room = await _roomRepository.GetRoomByIdReadOnlyAsync(id);

        if (room == null) 
        {
            return DomainErrors.Room.NotFound(id);
        }

        return room.MapToDto();
    }

    public async Task<ErrorOr<RoomResponseDto?>> UpdateRoomAsync(Guid id, UpdateRoomDto dto)
    {
        var room = await _roomRepository.GetRoomByIdAsync(id);

        if (room == null) 
        {
            return DomainErrors.Room.NotFound(id);
        }

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

        await _roomRepository.SaveChangesAsync();

        return room.MapToDto();
    }

    public async Task<ErrorOr<bool>> DeleteRoomAsync(Guid id)
    {
        var deleted = await _roomRepository.DeleteRoomAsync(id);

        if (!deleted) 
        {
            return DomainErrors.Room.NotFound(id);
        }

        return true;
    }

    public async Task<IReadOnlyList<RoomResponseDto>> GetRoomsByWorkspaceIdAsync(Guid workspaceId)
    {
        var rooms = await _roomRepository.GetRoomsByWorkspaceIdAsync(workspaceId);

        return rooms
            .Select(room => room.MapToDto())
            .ToList()
            .AsReadOnly();
    }
}
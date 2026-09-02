using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;
using DeskSync.Api.DTOs.Rooms;

namespace DeskSync.Api.Services;

public class RoomService(IRoomRepository roomRepository) : IRoomService
{
    private readonly IRoomRepository _roomRepository = roomRepository;

   public async Task<RoomResponseDto> CreateRoomAsync(CreateRoomDto dto)
    {        
        return await _roomRepository.CreateRoomAsync(dto);
    }

    public async Task<RoomResponseDto?> GetRoomByIdAsync(Guid id)
    {
        return await _roomRepository.GetRoomByIdAsync(id);
    }

    public async Task<RoomResponseDto?> UpdateRoomAsync(Guid id, UpdateRoomDto dto)
    {
        return await _roomRepository.UpdateRoomAsync(id, dto);
    }

    public async Task<bool> DeleteRoomAsync(Guid id)
    {
        return await _roomRepository.DeleteRoomAsync(id);
    }

    public async Task<IEnumerable<RoomResponseDto>> GetRoomsByWorkspaceAsync(Guid workspaceId)
    {
        return await _roomRepository.GetRoomsByWorkspaceAsync(workspaceId);
    }
}

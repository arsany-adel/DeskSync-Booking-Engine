using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Entities;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services.Interfaces;

namespace DeskSync.Api.Services;

public class RoomService(IRoomRepository roomRepository) : IRoomService
{
    private readonly IRoomRepository _roomRepository = roomRepository;

    public async Task<RoomResponseDto> CreateRoomAsync(CreateRoomDto dto)
    {
        var responseDto = new RoomResponseDto(
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

        var room = responseDto.MapToEntity();

        _roomRepository.AddRoom(room);
        await _roomRepository.SaveChangesAsync();

        return responseDto;
    }

    public async Task<RoomResponseDto?> GetRoomByIdAsync(Guid id)
    {
        var room = await _roomRepository.GetRoomByIdAsync(id);

        if (room == null) return null;

        return room.MapToDto();
    }

    public async Task<RoomResponseDto?> UpdateRoomAsync(Guid id, UpdateRoomDto dto)
    {
        var room = await _roomRepository.GetRoomByIdAsync(id);

        if (room == null) return null;

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

    public async Task<bool> DeleteRoomAsync(Guid id)
    {
        return await _roomRepository.DeleteRoomAsync(id);
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
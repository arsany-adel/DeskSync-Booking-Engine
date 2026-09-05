using DeskSync.Api.DTOs.Rooms;

namespace DeskSync.Api.Services.Interfaces;

public interface IRoomService
{
    Task<RoomResponseDto> CreateRoomAsync(CreateRoomDto dto);
    Task<RoomResponseDto?> GetRoomByIdAsync(Guid id);
    Task<RoomResponseDto?> UpdateRoomAsync(Guid id, UpdateRoomDto dto);
    Task<bool> DeleteRoomAsync(Guid id);
    Task<IEnumerable<RoomResponseDto>> GetRoomsByWorkspaceAsync(Guid workspaceId);
}

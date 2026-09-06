using DeskSync.Api.DTOs.Rooms;

namespace DeskSync.Api.Repositories.Interfaces;

public interface IRoomRepository
{
    Task<RoomResponseDto> CreateRoomAsync(CreateRoomDto dto);
    Task<RoomResponseDto?> GetRoomByIdAsync(Guid id);
    Task<IEnumerable<RoomResponseDto>> GetRoomsByWorkspaceIdAsync(Guid workspaceId);
    Task<RoomResponseDto?> UpdateRoomAsync(Guid id, UpdateRoomDto dto);
    Task<bool> DeleteRoomAsync(Guid id);
}

using DeskSync.Api.DTOs.Rooms;

namespace DeskSync.Api.Repositories.Interfaces;

public interface IRoomService
{
    Task<RoomResponseDto> CreateRoomAsync(CreateRoomDto dto);
    Task<RoomResponseDto?> GetRoomByIdAsync(Guid id);
    Task<IEnumerable<RoomResponseDto?>> GetAllRoomsAsync();
    Task<IEnumerable<RoomResponseDto>> GetRoomsByWorkspaceAsync(Guid workspaceId);
    Task<RoomResponseDto?> UpdateRoomAsync(Guid id, UpdateRoomDto dto);
    Task<bool> DeleteRoomAsync(Guid id);
}

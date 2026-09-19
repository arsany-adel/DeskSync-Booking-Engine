using DeskSync.Api.DTOs.Rooms;
using ErrorOr;
namespace DeskSync.Api.Services.Interfaces;

public interface IRoomService
{
    Task<ErrorOr<RoomResponseDto>> CreateRoomAsync(CreateRoomDto dto);
    Task<ErrorOr<RoomResponseDto?>> GetRoomByIdReadOnlyAsync(Guid id);
    Task<ErrorOr<RoomResponseDto?>> UpdateRoomAsync(Guid id, UpdateRoomDto dto);
    Task<ErrorOr<bool>> DeleteRoomAsync(Guid id);
    Task<IReadOnlyList<RoomResponseDto>> GetRoomsByWorkspaceIdAsync(Guid workspaceId);

    
}

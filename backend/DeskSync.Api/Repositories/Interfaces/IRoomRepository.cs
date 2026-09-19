using DeskSync.Api.Entities;

namespace DeskSync.Api.Repositories.Interfaces;

public interface IRoomRepository
{
    Room AddRoom(Room room);
    Task<Room?> GetRoomByIdReadOnlyAsync(Guid id);
    Task<Room?> GetRoomByIdAsync(Guid id);
    Task<IReadOnlyList<Room>> GetRoomsByWorkspaceIdAsync(Guid workspaceId);
    Task<bool> DeleteRoomAsync(Guid id);
    Task SaveChangesAsync();
}

using DeskSync.Api.Entities;

namespace DeskSync.Api.Repositories.Interfaces;

public interface IRoomRepository
{
    Room AddRoom(Room room);
    Task<Room?> GetRoomByIdAsync(Guid id);
    Task<Room?> GetRoomByIdTrackedAsync(Guid id);
    Task<IReadOnlyList<Room>> GetRoomsByWorkspaceIdAsync(Guid workspaceId);
    Task<bool> DeleteRoomAsync(Guid id);
    Task SaveChangesAsync();
}

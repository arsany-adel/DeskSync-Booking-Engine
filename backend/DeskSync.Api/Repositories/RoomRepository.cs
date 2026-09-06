using DeskSync.Api.Data;
using DeskSync.Api.Entities;
using DeskSync.Api.Extensions.Mappers;
using DeskSync.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeskSync.Api.Repositories;

public class RoomRepository(AppDbContext context) : IRoomRepository
{
    private readonly AppDbContext _context = context;

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync(); 

    public async Task<Room> AddRoomAsync(Room room)
    {
        await _context.Rooms.AddAsync(room);
        return room;
    }

    public async Task<Room?> GetRoomByIdAsync(Guid id)
    {
        return await _context.Rooms.FindAsync(id);
    }

    public async Task<bool> DeleteRoomAsync(Guid id)
    {
        int deletedRows = await _context.Rooms.Where(r => r.Id == id).ExecuteDeleteAsync();

        return deletedRows > 0;
    }

    public async Task<IReadOnlyList<Room>> GetRoomsByWorkspaceIdAsync(Guid workspaceId)
    {
        return await _context
            .Rooms.AsNoTracking()
            .Where(r => r.WorkspaceId == workspaceId)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }
}

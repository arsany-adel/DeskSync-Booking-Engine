using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Extensions.Mappers;

public class RoomMapperExtension
{
    public static RoomResponseDto MapToDto(Room room)
    {
        return new RoomResponseDto(
            room.Id,
            room.WorkspaceId,
            room.Name,
            room.Description,
            room.NoOfChairs,
            room.PricePerHour,
            room.Status,
            room.HasProjector,
            room.HasBoard,
            room.RecommendedUse
        );
    }
}

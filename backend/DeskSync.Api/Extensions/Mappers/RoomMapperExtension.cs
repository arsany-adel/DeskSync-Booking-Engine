using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Extensions.Mappers;

public static class RoomMapperExtension
{
    public static Room MapToEntity(this RoomResponseDto roomDto)
    {
        return new Room(
            roomDto.Id,
            roomDto.WorkspaceId,
            roomDto.Name,
            roomDto.Description,
            roomDto.NoOfChairs,
            roomDto.PricePerHour,
            roomDto.Status,
            roomDto.HasProjector,
            roomDto.HasBoard,
            roomDto.RecommendedUse
        );
    }
}

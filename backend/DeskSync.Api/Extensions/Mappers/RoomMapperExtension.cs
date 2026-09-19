using System;
using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Entities;

namespace DeskSync.Api.Extensions.Mappers;

public static class RoomMapperExtension
{
    public static Room MapToEntity(this CreateRoomDto dto, Guid roomId)
    {
        return new Room(
            roomId,
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
    }

    public static RoomResponseDto MapToDto(this Room room)
    {
        return new(
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
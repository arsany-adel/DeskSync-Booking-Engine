using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeskSync.Api.Entities;
using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Rooms;

public record RoomResponseDto(
    Guid Id, 
    Guid WorkspaceId, 
    string Name, 
    string? Description, 
    int NoOfChairs, 
    decimal PricePerHour, 
    RoomStatus Status, 
    bool HasProjector, 
    bool HasBoard, 
    RoomRecommendedUse RecommendedUse
);

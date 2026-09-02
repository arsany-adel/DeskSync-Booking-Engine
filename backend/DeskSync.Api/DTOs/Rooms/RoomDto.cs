using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeskSync.Api.Entities;
using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Rooms;

public record CreateRoomDto(
    [Required(ErrorMessage = "WORKSPACE_ID_REQUIRED")]
    Guid WorkspaceId, 
    
    [Required(ErrorMessage = "ROOM_NAME_REQUIRED")]
    [StringLength(100, ErrorMessage = "ROOM_NAME_TOO_LONG")]
    string Name, 
    
    [StringLength(500, ErrorMessage = "ROOM_DESC_TOO_LONG")]
    string? Description, 
    
    [Range(1, 1000, ErrorMessage = "ROOM_CHAIRS_INVALID")]
    int NoOfChairs, 
    
    [Range(0, 10000, ErrorMessage = "ROOM_PRICE_INVALID")]
    decimal PricePerHour, 
    
    [EnumDataType(typeof(RoomStatus), ErrorMessage = "INVALID_ROOM_STATUS")]
    RoomStatus Status, 
    
    bool HasProjector, 
    
    bool HasBoard, 
    
    [EnumDataType(typeof(RoomRecommendedUse), ErrorMessage = "INVALID_RECOMMENDED_USE")]
    RoomRecommendedUse RecommendedUse
);

public record UpdateRoomDto(
    [Required(ErrorMessage = "ROOM_NAME_REQUIRED")]
    [StringLength(100, ErrorMessage = "ROOM_NAME_TOO_LONG")]
    string Name, 
    
    [StringLength(500, ErrorMessage = "ROOM_DESC_TOO_LONG")]
    string? Description, 
    
    [Range(1, 1000, ErrorMessage = "ROOM_CHAIRS_INVALID")]
    int NoOfChairs, 
    
    [Range(0, 10000, ErrorMessage = "ROOM_PRICE_INVALID")]
    decimal PricePerHour, 
    
    [EnumDataType(typeof(RoomStatus), ErrorMessage = "INVALID_ROOM_STATUS")]
    RoomStatus Status, 
    
    bool HasProjector, 
    
    bool HasBoard, 
    
    [EnumDataType(typeof(RoomRecommendedUse), ErrorMessage = "INVALID_RECOMMENDED_USE")]
    RoomRecommendedUse RecommendedUse
);

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

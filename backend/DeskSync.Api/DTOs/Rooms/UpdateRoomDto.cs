using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeskSync.Api.Entities;
using System.ComponentModel.DataAnnotations;

namespace DeskSync.Api.DTOs.Rooms;

public record UpdateRoomDto(
    [Required(ErrorMessage = "ROOM_NAME_REQUIRED")]
    [StringLength(100, ErrorMessage = "ROOM_NAME_TOO_LONG")]
    string Name, 
    
    [StringLength(1000, ErrorMessage = "ROOM_DESC_TOO_LONG")]
    string? Description, 
    
    [EnumDataType(typeof(RoomStatus), ErrorMessage = "INVALID_ROOM_STATUS")]
    RoomStatus Status, 
    
    [Required(ErrorMessage = "ROOM_PROJECTOR_BOOL_REQUIRED")]
    bool HasProjector, 
        
    [Required(ErrorMessage = "ROOM_BOARD_BOOL_REQUIRED")]
    bool HasBoard, 
    
    [EnumDataType(typeof(RoomRecommendedUse), ErrorMessage = "INVALID_RECOMMENDED_USE")]
    [Required(ErrorMessage = "ROOM_RECOMMENDED_USE_REQUIRED")]
    RoomRecommendedUse RecommendedUse,

    [Range(1, 1000, ErrorMessage = "ROOM_CHAIRS_INVALID")]
    [Required(ErrorMessage = "ROOM_CHAIRS_REQUIRED")]
    int NoOfChairs=1,

    [Range(typeof(decimal), "0.00", "99999999.99", ErrorMessage = "ROOM_PRICE_INVALID")]
    decimal PricePerHour =0m
);

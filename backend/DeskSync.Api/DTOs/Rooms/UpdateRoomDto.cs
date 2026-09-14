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
    [StringLength(50, ErrorMessage = "ROOM_STATUS_TOO_LONG")]
    RoomStatus Status, 
    
    [Required(ErrorMessage = "ROOM_PROJECTOR_BOOL_REQUIRED")]
    bool HasProjector, 
        
    [Required(ErrorMessage = "ROOM_BOARD_BOOL_REQUIRED")]
    bool HasBoard, 
    
    [EnumDataType(typeof(RoomRecommendedUse), ErrorMessage = "INVALID_RECOMMENDED_USE")]
    [Required(ErrorMessage = "ROOM_RECOMMENDED_USE_REQUIRED")]
    [StringLength(50, ErrorMessage = "ROOM_RECOMMENDED_USE_TOO_LONG")]
    RoomRecommendedUse RecommendedUse,

    [Range(1, 1000, ErrorMessage = "ROOM_CHAIRS_INVALID")]
    [Required(ErrorMessage = "ROOM_CHAIRS_REQUIRED")]
    int NoOfChairs=1,

    [Range(0, 10000, ErrorMessage = "ROOM_PRICE_INVALID")]
    [RegularExpression(@"^\d{1,8}(\.\d{2})?$", ErrorMessage = "Must be a valid number with up to 8 whole digits and 2 decimal places.")]
    decimal PricePerHour =0m
);

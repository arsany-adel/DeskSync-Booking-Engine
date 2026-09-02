using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Repositories.Interfaces;

namespace DeskSync.Api.Controllers;

[ApiController]
[Route("/api/room")]
[Authorize(Roles = "Admin")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoomResponseDto>> Create([FromBody] CreateRoomDto dto)
    {
        try
        {
            var room = await _roomService.CreateRoomAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }
        catch (ArgumentException ex) 
        {
            return BadRequest(new { ErrorCode = "INVALID_ROOM_DATA", Details = ex.Message });
        }
    }

    [HttpGet("get/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomResponseDto>> GetById(Guid id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        
        if (room == null) return NotFound();

        return room;
    }

    [HttpGet("get-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetAll()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    [HttpPut("update/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoomResponseDto>> Update(Guid id, [FromBody] UpdateRoomDto dto)
    {
        try
        {
            var updatedRoom = await _roomService.UpdateRoomAsync(id, dto);
            
            if (updatedRoom == null) return NotFound();

            return updatedRoom;
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { ErrorCode = "INVALID_ROOM_DATA", Details = ex.Message });
        }
    }

    [HttpDelete("delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _roomService.DeleteRoomAsync(id);
        
        if (!deleted) return NotFound();

        return NoContent();
    }

    
    //for future updates
    [HttpGet("workspace/{workspaceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetByWorkspace(Guid workspaceId)
    {
        var rooms = await _roomService.GetRoomsByWorkspaceAsync(workspaceId);
        
        return Ok(rooms); 
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Services.Interfaces;
namespace DeskSync.Api.Controllers;

[ApiController]
[Route("/api/rooms")]
public class RoomsController(IRoomService RoomService) : ControllerBase
{
    private readonly IRoomService _roomService = RoomService;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
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

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomResponseDto>> GetById(Guid id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        
        if (room == null) return NotFound();

        return room;
    }


    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
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

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _roomService.DeleteRoomAsync(id);
        
        if (!deleted) return NotFound();

        return NoContent();
    }

    
    [HttpGet("workspace/{workspaceId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<RoomResponseDto>>> GetRoomsByWorkspaceId(Guid workspaceId)
    {
        var rooms = await _roomService.GetRoomsByWorkspaceIdAsync(workspaceId);
        
        return Ok(rooms); 
    }
}
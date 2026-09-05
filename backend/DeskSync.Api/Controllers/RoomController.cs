using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Services.Interfaces;
namespace DeskSync.Api.Controllers;

[ApiController]
[Route("/api/rooms")]
public class RoomsController(IRoomService RoomService) : ControllerBase
{
    private readonly IRoomService _RoomService = RoomService;

    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomResponseDto>> Create([FromBody] CreateRoomDto dto)
    {
        try
        {
            var room = await _RoomService.CreateRoomAsync(dto);
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
    [Authorize(Roles = "Admin,Standard")]
    public async Task<ActionResult<RoomResponseDto>> GetById(Guid id)
    {
        var room = await _RoomService.GetRoomByIdAsync(id);
        
        if (room == null) return NotFound();

        return room;
    }


    [HttpPut("update/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomResponseDto>> Update(Guid id, [FromBody] UpdateRoomDto dto)
    {
        try
        {
            var updatedRoom = await _RoomService.UpdateRoomAsync(id, dto);
            
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
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _RoomService.DeleteRoomAsync(id);
        
        if (!deleted) return NotFound();

        return NoContent();
    }

    
    [HttpGet("workspace/{workspaceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<RoomResponseDto>>> GetRoomsByWorkspaceId(Guid workspaceId)
    {
        var rooms = await _RoomService.GetRoomsByWorkspaceAsync(workspaceId);
        
        return Ok(rooms); 
    }
}
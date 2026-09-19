using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DeskSync.Api.DTOs.Rooms;
using DeskSync.Api.Services.Interfaces;

namespace DeskSync.Api.Controllers;

[Route("/api/rooms")]
public class RoomsController(IRoomService roomService) : BaseApiController
{
    private readonly IRoomService _roomService = roomService;

    [HttpPost]
    [ProducesResponseType(typeof(RoomResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomResponseDto>> CreateRoom([FromBody] CreateRoomDto dto)
    {
        var result = await _roomService.CreateRoomAsync(dto);
        
        if (result.IsError) return ErrorResult(result.Errors);

        return CreatedAtAction(nameof(GetRoomById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoomResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoomResponseDto>> GetRoomById(Guid id)
    {
        var result = await _roomService.GetRoomByIdReadOnlyAsync(id);
        
        if (result.IsError) return ErrorResult(result.Errors);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(RoomResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RoomResponseDto>> UpdateRoom(Guid id, [FromBody] UpdateRoomDto dto)
    {
        var result = await _roomService.UpdateRoomAsync(id, dto);
        
        if (result.IsError) return ErrorResult(result.Errors);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteRoom(Guid id)
    {
        var result = await _roomService.DeleteRoomAsync(id);
        
        if (result.IsError) return ErrorResult(result.Errors);

        return NoContent();
    }

    [HttpGet("workspace/{workspaceId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<RoomResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IReadOnlyList<RoomResponseDto>>> GetRoomsByWorkspaceId(Guid workspaceId)
    {
        var rooms = await _roomService.GetRoomsByWorkspaceIdAsync(workspaceId);
        return Ok(rooms); 
    }
}
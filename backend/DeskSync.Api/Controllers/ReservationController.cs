using System;
using System.Security.Claims;
using DeskSync.Api.DTOs.Reservations;
using DeskSync.Api.Services.Interfaces;
using DeskSync.Api.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace DeskSync.Api.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationController(IReservationService reservationService) : ControllerBase
{
    private readonly IReservationService _reservationService = reservationService;

    // User : is HttpContext.User, which is populated by the JWT middleware.
    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (!Guid.TryParse(userIdString, out Guid userId))
            throw new UnauthorizedAccessException("Invalid user token.");
            
        return userId;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _reservationService.CreateReservation(userId, dto);
        
        return CreatedAtAction(nameof(GetReservation), new { id = result.Id }, result);
    }

    [HttpGet("my-reservations")]
    public async Task<IActionResult> GetMyReservations(
        [FromQuery] LocalDateTime? startDate, 
        [FromQuery] LocalDateTime? endDate, 
        [FromQuery] int pageNumber = PaginationConstants.DefaultPageNumber, 
        [FromQuery] int itemsPerPage = PaginationConstants.DefaultPageSize)
    {
        var userId = GetCurrentUserId();
        
        var result = await _reservationService.SearchReservationAsync(
            roomId: null, userId, startDate, endDate, pageNumber, itemsPerPage);
            
        return Ok(result);
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetReservation(Guid id)
    {
        var userId = User.IsInRole("Admin") ? (Guid?)null : GetCurrentUserId();
        
        var result = await _reservationService.GetReservationAsync(id, userId);
        return Ok(result);
    }

    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateReservation(Guid id, [FromBody] UpdateReservationDto dto)
    {
        var userId = GetCurrentUserId();
        
        var result = await _reservationService.UpdateReservation(id, dto, userId);
        return Ok(result);
    }

    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteReservation(Guid id)
    {
        var userId = GetCurrentUserId();
        await _reservationService.DeleteReservationAsync(id, userId);
        
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("room/{roomId}/schedule")]
    public async Task<IActionResult> GetRoomSchedule(
        Guid roomId, 
        [FromQuery] LocalDateTime startDate, 
        [FromQuery] LocalDateTime endDate)
    {
        var schedule = await _reservationService.GetRoomScheduleAsync(roomId, startDate, endDate);
        return Ok(schedule);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("search")]
    public async Task<IActionResult> SearchReservations(
        [FromQuery] Guid? roomId,
        [FromQuery] Guid? userId,
        [FromQuery] LocalDateTime? startDate,
        [FromQuery] LocalDateTime? endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int itemsPerPage = 10)
    {
        var result = await _reservationService.SearchReservationAsync(
            roomId, userId, startDate, endDate, pageNumber, itemsPerPage);
            
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/admin-update")]
    public async Task<IActionResult> AdminUpdateReservation(Guid id, [FromBody] AdminUpdateReservationDto dto)
    {
        var result = await _reservationService.AdminUpdateReservation(id, dto);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}/force-delete")]
    public async Task<IActionResult> AdminDeleteReservation(Guid id)
    {
        await _reservationService.AdminDeleteReservationAsync(id);
        return NoContent();
    }
}
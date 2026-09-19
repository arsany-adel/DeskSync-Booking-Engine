using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using DeskSync.Api.Constants;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.DTOs.Reservations;
using DeskSync.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace DeskSync.Api.Controllers;


[Route("api/reservations")]
[Authorize]
public class ReservationController(IReservationService reservationService) : BaseApiController
{
    private readonly IReservationService _reservationService = reservationService;

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out Guid userId))
            throw new UnauthorizedAccessException("Invalid user token.");

        return userId;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponseDto>> CreateReservation(
        [FromBody] CreateReservationDto dto
    )
    {
        var userId = GetCurrentUserId();
        var result = await _reservationService.CreateReservation(userId, dto);

        if (result.IsError)
            return ErrorResult(result.Errors);

        return CreatedAtAction(nameof(GetReservation), new { id = result.Value.Id }, result);
    }

    [HttpGet("my-reservations")]
    [ProducesResponseType(typeof(PagedResult<ReservationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ReservationResponseDto>>> GetMyReservations(
        [FromQuery] LocalDateTime? startDate,
        [FromQuery] LocalDateTime? endDate,
        [FromQuery] int pageNumber = PaginationConstants.DefaultPageNumber,
        [FromQuery] int itemsPerPage = PaginationConstants.DefaultPageSize
    )
    {
        var userId = GetCurrentUserId();

        var result = await _reservationService.SearchReservationAsync(
            roomId: null,
            userId,
            startDate,
            endDate,
            pageNumber,
            itemsPerPage
        );

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponseDto>> GetReservation(Guid id)
    {
        var userId = User.IsInRole("Admin") ? (Guid?)null : GetCurrentUserId();

        var result = await _reservationService.GetReservationAsync(id, userId);

        if (result.IsError)
            return ErrorResult(result.Errors);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponseDto>> UpdateReservation(
        Guid id,
        [FromBody] UpdateReservationDto dto
    )
    {
        var userId = GetCurrentUserId();

        var result = await _reservationService.UpdateReservation(id, dto, userId);

        if (result.IsError)
            return ErrorResult(result.Errors);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteReservation(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _reservationService.DeleteReservationAsync(id, userId);

        if (result.IsError)
            return ErrorResult(result.Errors);

        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("room/{roomId:guid}/schedule")]
    [ProducesResponseType(typeof(IReadOnlyList<RoomScheduleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<RoomScheduleDto>>> GetRoomSchedule(
        Guid roomId,
        [FromQuery] LocalDateTime startDate,
        [FromQuery] LocalDateTime endDate
    )
    {
        var schedule = await _reservationService.GetRoomScheduleAsync(roomId, startDate, endDate);
        return Ok(schedule);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<ReservationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<ReservationResponseDto>>> SearchReservations(
        [FromQuery] ReservationSearchQueryDto query
    )
    {
        var result = await _reservationService.SearchReservationAsync(
            query.RoomId,
            query.UserId,
            query.StartDate,
            query.EndDate,
            query.PageNumber,
            query.PageSize
        );

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/admin-update")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponseDto>> AdminUpdateReservation(
        Guid id,
        [FromBody] AdminUpdateReservationDto dto
    )
    {
        var result = await _reservationService.AdminUpdateReservation(id, dto);

        if (result.IsError)
            return ErrorResult(result.Errors);

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}/force-delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> AdminDeleteReservation(Guid id)
    {
        var result = await _reservationService.AdminDeleteReservationAsync(id);
        
        if (result.IsError) return ErrorResult(result.Errors);

        return NoContent();
    }
}

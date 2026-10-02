using System.Security.Claims;
using DeskSync.Api.DTOs;
using DeskSync.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeskSync.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationController(INotificationService notificationService) : BaseApiController
{
    private readonly INotificationService _notificationService = notificationService;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId = GetCurrentUserId();
        
        var result = unreadOnly 
            ? await _notificationService.GetUnreadUserNotificationsAsync(userId)
            : await _notificationService.GetAllUserNotificationsAsync(userId);

        if (result.IsError)
            return ErrorResult(result.Errors);

        return Ok(result.Value);
    }

    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var userId = GetCurrentUserId();
        
        var result = await _notificationService.MarkAsReadAsync(id, userId);

        if (result.IsError)
            return ErrorResult(result.Errors);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out Guid userId))
            throw new UnauthorizedAccessException("Invalid user token.");

        return userId;
    }
}
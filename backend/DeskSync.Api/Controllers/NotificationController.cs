using DeskSync.Api.DTOs;
using DeskSync.Api.DTOs.Common;
using DeskSync.Api.DTOs.Notifications;
using DeskSync.Api.Extensions;
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
    [ProducesResponseType(
        typeof(PagedResult<NotificationResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<NotificationResponseDto>>> GetMyNotifications(
        [FromQuery] NotificationSearchQueryDto query,
        CancellationToken cancellationToken,
        [FromQuery] bool unreadOnly = false)
    {
        var userId = User.GetUserId();

        var result = unreadOnly
            ? await _notificationService.GetUnreadUserNotificationsAsync(
                userId, query.PageNumber, query.PageSize, cancellationToken)
            : await _notificationService.GetAllUserNotificationsAsync(
                userId, query.PageNumber, query.PageSize, cancellationToken);

        if (result.IsError)
        {
            return ErrorResult(result.Errors);
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _notificationService.MarkAsReadAsync(id, userId, cancellationToken);

        if (result.IsError)
        {
            return ErrorResult(result.Errors);
        }

        return NoContent();
    }
}
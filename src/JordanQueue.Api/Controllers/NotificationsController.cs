using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Notifications;
using JordanQueue.Application.Exceptions;
using JordanQueue.Application.Interfaces.Auth;
using JordanQueue.Application.Interfaces.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JordanQueue.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationQueryService _notificationService;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(INotificationQueryService notificationService, ICurrentUserService currentUser)
    {
        _notificationService = notificationService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NotificationDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetUserNotificationsAsync(GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NotificationDto>>.Ok(result));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { id }, "Notification marked as read."));
    }

    private Guid GetUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
}

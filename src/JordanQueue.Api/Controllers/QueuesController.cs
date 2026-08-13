using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Queues;
using JordanQueue.Application.Exceptions;
using JordanQueue.Application.Interfaces.Auth;
using JordanQueue.Application.Interfaces.Queues;
using JordanQueue.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JordanQueue.Api.Controllers;

[ApiController]
[Route("api/queues")]
public class QueuesController : ControllerBase
{
    private readonly IQueueService _queueService;
    private readonly ICurrentUserService _currentUser;

    public QueuesController(IQueueService queueService, ICurrentUserService currentUser)
    {
        _queueService = queueService;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpGet("{queueId:guid}")]
    public async Task<ActionResult<ApiResponse<QueueDto>>> GetQueue(Guid queueId, CancellationToken cancellationToken)
    {
        var result = await _queueService.GetQueueAsync(queueId, cancellationToken);
        return Ok(ApiResponse<QueueDto>.Ok(result));
    }

    [Authorize(Roles = RoleNames.Customer)]
    [HttpGet("{queueId:guid}/position")]
    public async Task<ActionResult<ApiResponse<QueuePositionDto>>> GetPosition(Guid queueId, CancellationToken cancellationToken)
    {
        var result = await _queueService.GetQueuePositionAsync(queueId, GetUserId(), cancellationToken);
        return Ok(ApiResponse<QueuePositionDto>.Ok(result));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpPost("{queueId:guid}/next")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> CallNext(Guid queueId, CancellationToken cancellationToken)
    {
        var result = await _queueService.CallNextAsync(queueId, GetUserId(), cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(result, "Next customer called."));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpPost("{queueId:guid}/pause")]
    public async Task<ActionResult<ApiResponse<QueueDto>>> Pause(Guid queueId, CancellationToken cancellationToken)
    {
        var result = await _queueService.PauseQueueAsync(queueId, GetUserId(), cancellationToken);
        return Ok(ApiResponse<QueueDto>.Ok(result, "Queue paused."));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpPost("{queueId:guid}/resume")]
    public async Task<ActionResult<ApiResponse<QueueDto>>> Resume(Guid queueId, CancellationToken cancellationToken)
    {
        var result = await _queueService.ResumeQueueAsync(queueId, GetUserId(), cancellationToken);
        return Ok(ApiResponse<QueueDto>.Ok(result, "Queue resumed."));
    }

    private Guid GetUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
}

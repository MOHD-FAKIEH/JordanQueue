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
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly IQueueService _queueService;
    private readonly ICurrentUserService _currentUser;

    public TicketsController(IQueueService queueService, ICurrentUserService currentUser)
    {
        _queueService = queueService;
        _currentUser = currentUser;
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TicketDto>>>> GetMyTickets(CancellationToken cancellationToken)
    {
        var result = await _queueService.GetMyTicketsAsync(GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TicketDto>>.Ok(result));
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queueService.GetTicketAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(result));
    }

    [Authorize(Roles = $"{RoleNames.Customer},{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var isStaff = _currentUser.IsInRole(RoleNames.Staff) || _currentUser.IsInRole(RoleNames.BusinessOwner);
        var result = await _queueService.CancelTicketAsync(id, GetUserId(), isStaff, cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(result, "Ticket cancelled."));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpPost("{id:guid}/serve")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Serve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queueService.ServeTicketAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(result, "Customer served."));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpPost("{id:guid}/skip")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> Skip(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queueService.SkipTicketAsync(id, GetUserId(), cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(result, "Customer skipped."));
    }

    private Guid GetUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
}

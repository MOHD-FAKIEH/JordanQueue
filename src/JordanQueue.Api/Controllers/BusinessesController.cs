using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Businesses;
using JordanQueue.Application.DTOs.Queues;
using JordanQueue.Application.Exceptions;
using JordanQueue.Application.Interfaces.Auth;
using JordanQueue.Application.Interfaces.Businesses;
using JordanQueue.Application.Interfaces.Queues;
using JordanQueue.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JordanQueue.Api.Controllers;

[ApiController]
[Route("api/businesses")]
public class BusinessesController : ControllerBase
{
    private readonly IBusinessService _businessService;
    private readonly IQueueService _queueService;
    private readonly ICurrentUserService _currentUser;

    public BusinessesController(IBusinessService businessService, IQueueService queueService, ICurrentUserService currentUser)
    {
        _businessService = businessService;
        _queueService = queueService;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BusinessDto>>>> Search([FromQuery] BusinessSearchRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessService.SearchAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<BusinessDto>>.Ok(result));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BusinessDto>>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _businessService.GetMineAsync(GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BusinessDto>>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BusinessDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _businessService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<BusinessDetailDto>.Ok(result));
    }

    [Authorize(Roles = RoleNames.BusinessOwner)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<BusinessDto>>> Create([FromBody] CreateBusinessRequest request, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _businessService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BusinessDto>.Ok(result, "Business created."));
    }

    [Authorize(Roles = RoleNames.BusinessOwner)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BusinessDto>>> Update(Guid id, [FromBody] UpdateBusinessRequest request, CancellationToken cancellationToken)
    {
        var result = await _businessService.UpdateAsync(id, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<BusinessDto>.Ok(result, "Business updated."));
    }

    [AllowAnonymous]
    [HttpGet("{businessId:guid}/queue")]
    public async Task<ActionResult<ApiResponse<QueueDto>>> GetQueue(Guid businessId, [FromQuery] Guid? serviceId, CancellationToken cancellationToken)
    {
        var result = await _queueService.GetBusinessQueueAsync(businessId, serviceId, cancellationToken);
        return Ok(ApiResponse<QueueDto>.Ok(result));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpPost("{businessId:guid}/queue/open")]
    public async Task<ActionResult<ApiResponse<QueueDto>>> OpenQueue(Guid businessId, [FromBody] OpenQueueRequest request, CancellationToken cancellationToken)
    {
        var result = await _queueService.OpenQueueAsync(businessId, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<QueueDto>.Ok(result, "Queue opened."));
    }

    [Authorize(Roles = RoleNames.Customer)]
    [HttpPost("{businessId:guid}/queue/join")]
    public async Task<ActionResult<ApiResponse<TicketDto>>> JoinQueue(Guid businessId, [FromBody] JoinQueueRequest request, CancellationToken cancellationToken)
    {
        var result = await _queueService.JoinQueueAsync(businessId, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<TicketDto>.Ok(result, "Joined queue successfully."));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpGet("{businessId:guid}/stats/daily")]
    public async Task<ActionResult<ApiResponse<DailyStatsDto>>> GetDailyStats(Guid businessId, CancellationToken cancellationToken)
    {
        var result = await _queueService.GetDailyStatsAsync(businessId, GetUserId(), cancellationToken);
        return Ok(ApiResponse<DailyStatsDto>.Ok(result));
    }

    private Guid GetUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
}

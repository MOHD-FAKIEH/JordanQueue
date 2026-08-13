using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Services;
using JordanQueue.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JordanQueue.Domain.Constants;
using JordanQueue.Application.Interfaces.Auth;
using JordanQueue.Application.Exceptions;

namespace JordanQueue.Api.Controllers;

[ApiController]
[Route("api")]
public class ServicesController : ControllerBase
{
    private readonly IServiceManagementService _serviceManagement;
    private readonly ICurrentUserService _currentUser;

    public ServicesController(IServiceManagementService serviceManagement, ICurrentUserService currentUser)
    {
        _serviceManagement = serviceManagement;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpGet("businesses/{businessId:guid}/services")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ServiceDto>>>> GetByBusiness(Guid businessId, CancellationToken cancellationToken)
    {
        var result = await _serviceManagement.GetByBusinessAsync(businessId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ServiceDto>>.Ok(result));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpPost("businesses/{businessId:guid}/services")]
    public async Task<ActionResult<ApiResponse<ServiceDto>>> Create(Guid businessId, [FromBody] CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _serviceManagement.CreateAsync(businessId, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<ServiceDto>.Ok(result, "Service created."));
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpPut("services/{id:guid}")]
    public async Task<ActionResult<ApiResponse<ServiceDto>>> Update(Guid id, [FromBody] UpdateServiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _serviceManagement.UpdateAsync(id, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<ServiceDto>.Ok(result, "Service updated."));
    }

    private Guid GetUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
}

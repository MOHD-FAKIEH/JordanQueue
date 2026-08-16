using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Staff;
using JordanQueue.Application.Exceptions;
using JordanQueue.Application.Interfaces.Auth;
using JordanQueue.Application.Interfaces.Staff;
using JordanQueue.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JordanQueue.Api.Controllers;

[ApiController]
[Route("api/businesses/{businessId:guid}/staff")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;
    private readonly ICurrentUserService _currentUser;

    public StaffController(IStaffService staffService, ICurrentUserService currentUser)
    {
        _staffService = staffService;
        _currentUser = currentUser;
    }

    [Authorize(Roles = $"{RoleNames.BusinessOwner},{RoleNames.Staff}")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StaffMemberDto>>>> GetStaff(Guid businessId, CancellationToken cancellationToken)
    {
        var result = await _staffService.GetByBusinessAsync(businessId, GetUserId(), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<StaffMemberDto>>.Ok(result));
    }

    [Authorize(Roles = RoleNames.BusinessOwner)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<StaffMemberDto>>> AddStaff(Guid businessId, [FromBody] AddStaffRequest request, CancellationToken cancellationToken)
    {
        var result = await _staffService.AddAsync(businessId, request, GetUserId(), cancellationToken);
        return Ok(ApiResponse<StaffMemberDto>.Ok(result, "Staff member added."));
    }

    [Authorize(Roles = RoleNames.BusinessOwner)]
    [HttpDelete("{staffId:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> RemoveStaff(Guid businessId, Guid staffId, CancellationToken cancellationToken)
    {
        await _staffService.DeactivateAsync(businessId, staffId, GetUserId(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Staff member removed."));
    }

    private Guid GetUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");
}

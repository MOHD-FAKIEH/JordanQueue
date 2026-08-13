using System.Security.Claims;
using JordanQueue.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace JordanQueue.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user is null)
            {
                return null;
            }

            var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles =>
        _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public bool IsInRole(string role) =>
        _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
}

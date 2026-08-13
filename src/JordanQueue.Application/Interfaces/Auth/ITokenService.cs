using JordanQueue.Domain.Entities;

namespace JordanQueue.Application.Interfaces.Auth;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
    DateTime GetRefreshTokenExpiry();
}

using JordanQueue.Application.DTOs.Auth;

namespace JordanQueue.Application.DTOs.Auth;

public record RegisterRequest(
    string FirstName,
    string LastName,
    string MobileNumber,
    string Email,
    string Password,
    string? Role = null);

public record LoginRequest(string EmailOrMobile, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record AuthResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string MobileNumber,
    IReadOnlyList<string> Roles,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt);

public record UserProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string MobileNumber,
    string PreferredLanguage,
    IReadOnlyList<string> Roles);

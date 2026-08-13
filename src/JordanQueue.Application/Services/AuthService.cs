using FluentValidation;
using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Auth;
using JordanQueue.Application.Exceptions;
using JordanQueue.Application.Interfaces;
using JordanQueue.Application.Interfaces.Auth;
using JordanQueue.Domain.Constants;
using JordanQueue.Domain.Entities;
using JordanQueue.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace JordanQueue.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtSettings _jwtSettings;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider,
        IOptions<JwtSettings> jwtSettings,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
        _jwtSettings = jwtSettings.Value;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_registerValidator, request, cancellationToken);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
        {
            throw new ConflictException("Email is already registered.", "EMAIL_EXISTS");
        }

        if (await _context.Users.AnyAsync(u => u.MobileNumber == request.MobileNumber, cancellationToken))
        {
            throw new ConflictException("Mobile number is already registered.", "MOBILE_EXISTS");
        }

        var roleName = string.IsNullOrWhiteSpace(request.Role) ? RoleNames.Customer : request.Role;
        if (roleName is not (RoleNames.Customer or RoleNames.BusinessOwner))
        {
            throw new Exceptions.ValidationException(["Registration role must be Customer or BusinessOwner."]);
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken)
            ?? throw new AppException("Role configuration is missing.", "ROLE_MISSING", 500);

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            MobileNumber = request.MobileNumber.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            PreferredLanguage = PreferredLanguage.Arabic,
            IsActive = true,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };

        await _context.AddEntityAsync(user, cancellationToken);
        await _context.AddEntityAsync(new UserRole { UserId = user.Id, RoleId = role.Id }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return await CreateAuthResponseAsync(user, [roleName], cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAsync(_loginValidator, request, cancellationToken);

        var login = request.EmailOrMobile.Trim().ToLowerInvariant();
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == login || u.MobileNumber == request.EmailOrMobile.Trim(), cancellationToken)
            ?? throw new UnauthorizedException("Invalid credentials.", "INVALID_CREDENTIALS");

        if (!user.IsActive)
        {
            throw new UnauthorizedException("Account is inactive.", "ACCOUNT_INACTIVE");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials.", "INVALID_CREDENTIALS");
        }

        var roles = await GetUserRolesAsync(user.Id, cancellationToken);
        return await CreateAuthResponseAsync(user, roles, cancellationToken);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new Exceptions.ValidationException(["Refresh token is required."]);
        }

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken)
            ?? throw new UnauthorizedException("Invalid refresh token.", "INVALID_REFRESH_TOKEN");

        if (!storedToken.IsActive)
        {
            throw new UnauthorizedException("Refresh token is expired or revoked.", "REFRESH_TOKEN_EXPIRED");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == storedToken.UserId, cancellationToken)
            ?? throw new UnauthorizedException("User not found.", "USER_NOT_FOUND");

        storedToken.RevokedAt = _dateTimeProvider.UtcNow;
        var roles = await GetUserRolesAsync(user.Id, cancellationToken);
        return await CreateAuthResponseAsync(user, roles, cancellationToken, storedToken.Token);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        User user,
        IReadOnlyList<string> roles,
        CancellationToken cancellationToken,
        string? replacedToken = null)
    {
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshExpiry = _tokenService.GetRefreshTokenExpiry();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            CreatedAt = _dateTimeProvider.UtcNow,
            ExpiresAt = refreshExpiry,
            ReplacedByToken = null
        };

        if (replacedToken is not null)
        {
            var oldToken = await _context.RefreshTokens.FirstAsync(t => t.Token == replacedToken, cancellationToken);
            oldToken.ReplacedByToken = refreshTokenValue;
            _context.UpdateEntity(oldToken);
        }

        await _context.AddEntityAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.MobileNumber,
            roles,
            accessToken,
            refreshTokenValue,
            _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.AccessTokenMinutes),
            refreshExpiry);
    }

    private async Task<IReadOnlyList<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken) =>
        await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name)
            .ToListAsync(cancellationToken);

    private static async Task ValidateAsync<T>(IValidator<T> validator, T instance, CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            throw new Exceptions.ValidationException(result.Errors.Select(e => e.ErrorMessage));
        }
    }
}

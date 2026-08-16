using JordanQueue.Application.DTOs.Staff;
using JordanQueue.Application.Exceptions;
using JordanQueue.Application.Interfaces;
using JordanQueue.Application.Interfaces.Businesses;
using JordanQueue.Application.Interfaces.Staff;
using JordanQueue.Domain.Constants;
using JordanQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JordanQueue.Application.Services;

public class StaffService : IStaffService
{
    private readonly IApplicationDbContext _context;
    private readonly IBusinessService _businessService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public StaffService(IApplicationDbContext context, IBusinessService businessService, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _businessService = businessService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<IReadOnlyList<StaffMemberDto>> GetByBusinessAsync(Guid businessId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _businessService.EnsureOwnerOrStaffAccessAsync(businessId, userId, cancellationToken);

        return await _context.BusinessStaff
            .Where(s => s.BusinessId == businessId)
            .Include(s => s.User)
            .OrderBy(s => s.User.FirstName)
            .Select(s => new StaffMemberDto(
                s.Id, s.UserId, s.User.FirstName, s.User.LastName,
                s.User.Email, s.User.MobileNumber, s.IsActive, s.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<StaffMemberDto> AddAsync(Guid businessId, AddStaffRequest request, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            ?? throw new NotFoundException("Business not found.");

        if (business.OwnerUserId != ownerUserId)
        {
            throw new UnauthorizedException("Only the business owner can manage staff.", "OWNER_ACCESS_REQUIRED");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new NotFoundException("User not found with that email.", "USER_NOT_FOUND");

        if (user.Id == business.OwnerUserId)
        {
            throw new AppException("Business owner cannot be added as staff.", "INVALID_STAFF");
        }

        var existing = await _context.BusinessStaff
            .FirstOrDefaultAsync(s => s.BusinessId == businessId && s.UserId == user.Id, cancellationToken);

        if (existing is not null)
        {
            if (existing.IsActive)
            {
                throw new ConflictException("User is already assigned to this business.", "STAFF_EXISTS");
            }

            existing.IsActive = true;
            _context.UpdateEntity(existing);
        }
        else
        {
            existing = new BusinessStaff
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                UserId = user.Id,
                IsActive = true,
                CreatedAt = _dateTimeProvider.UtcNow
            };
            await _context.AddEntityAsync(existing, cancellationToken);
        }

        var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Staff, cancellationToken)
            ?? throw new AppException("Staff role is not configured.", "ROLE_MISSING", 500);

        var hasStaffRole = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == staffRole.Id, cancellationToken);

        if (!hasStaffRole)
        {
            await _context.AddEntityAsync(new UserRole { UserId = user.Id, RoleId = staffRole.Id }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new StaffMemberDto(
            existing.Id, user.Id, user.FirstName, user.LastName,
            user.Email, user.MobileNumber, existing.IsActive, existing.CreatedAt);
    }

    public async Task DeactivateAsync(Guid businessId, Guid staffId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            ?? throw new NotFoundException("Business not found.");

        if (business.OwnerUserId != ownerUserId)
        {
            throw new UnauthorizedException("Only the business owner can manage staff.", "OWNER_ACCESS_REQUIRED");
        }

        var staff = await _context.BusinessStaff
            .FirstOrDefaultAsync(s => s.Id == staffId && s.BusinessId == businessId, cancellationToken)
            ?? throw new NotFoundException("Staff member not found.");

        staff.IsActive = false;
        _context.UpdateEntity(staff);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

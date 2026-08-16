using FluentValidation;
using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Businesses;
using JordanQueue.Application.Exceptions;
using JordanQueue.Application.Interfaces;
using JordanQueue.Application.Interfaces.Businesses;
using JordanQueue.Domain.Entities;
using JordanQueue.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JordanQueue.Application.Services;

public class BusinessService : IBusinessService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BusinessService(IApplicationDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PagedResult<BusinessDto>> SearchAsync(BusinessSearchRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.Businesses.Where(b => b.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(b =>
                b.NameEnglish.Contains(term) ||
                b.NameArabic.Contains(term) ||
                b.DescriptionEnglish.Contains(term) ||
                b.DescriptionArabic.Contains(term));
        }

        if (request.Category.HasValue)
        {
            query = query.Where(b => b.Category == request.Category.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(b => b.NameEnglish)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BusinessDto(
                b.Id, b.NameArabic, b.NameEnglish, b.DescriptionArabic, b.DescriptionEnglish,
                b.PhoneNumber, b.AddressArabic, b.AddressEnglish, b.Category, b.IsActive))
            .ToListAsync(cancellationToken);

        return new PagedResult<BusinessDto> { Items = items, TotalCount = total, Page = request.Page, PageSize = request.PageSize };
    }

    public async Task<IReadOnlyList<BusinessDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var ownedIds = await _context.Businesses
            .Where(b => b.OwnerUserId == userId && b.IsActive)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var staffBusinessIds = await _context.BusinessStaff
            .Where(s => s.UserId == userId && s.IsActive)
            .Select(s => s.BusinessId)
            .ToListAsync(cancellationToken);

        var businessIds = ownedIds.Union(staffBusinessIds).Distinct().ToList();

        return await _context.Businesses
            .Where(b => businessIds.Contains(b.Id))
            .OrderBy(b => b.NameEnglish)
            .Select(b => new BusinessDto(
                b.Id, b.NameArabic, b.NameEnglish, b.DescriptionArabic, b.DescriptionEnglish,
                b.PhoneNumber, b.AddressArabic, b.AddressEnglish, b.Category, b.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<BusinessDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Business not found.");

        var workingHours = await _context.BusinessWorkingHours
            .Where(w => w.BusinessId == id)
            .OrderBy(w => w.DayOfWeek)
            .Select(w => new WorkingHoursDto(w.DayOfWeek, w.OpeningTime, w.ClosingTime, w.IsClosed))
            .ToListAsync(cancellationToken);

        var services = await _context.Services
            .Where(s => s.BusinessId == id && s.IsActive)
            .Select(s => new ServiceSummaryDto(s.Id, s.NameArabic, s.NameEnglish, s.AverageServiceMinutes, s.IsActive))
            .ToListAsync(cancellationToken);

        QueueSummaryDto? currentQueue = null;
        var today = _dateTimeProvider.TodayInAmman;
        var queue = await _context.Queues
            .Where(q => q.BusinessId == id && q.QueueDate == today)
            .OrderByDescending(q => q.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (queue is not null)
        {
            var tickets = await _context.QueueTickets
                .Where(t => t.QueueId == queue.Id)
                .Select(t => new { t.TicketNumber, t.Status })
                .ToListAsync(cancellationToken);

            var ticketTuples = tickets.Select(t => (t.TicketNumber, t.Status));
            var waitingCount = tickets.Count(t => t.Status == TicketStatus.Waiting);
            var nowServing = QueueCalculator.GetNowServing(ticketTuples);
            var service = await _context.Services.FirstAsync(s => s.Id == queue.ServiceId, cancellationToken);
            var estimatedWait = QueueCalculator.CalculateEstimatedWait(waitingCount, service.AverageServiceMinutes);
            currentQueue = new QueueSummaryDto(queue.Id, nowServing, waitingCount, estimatedWait, queue.Status);
        }

        return new BusinessDetailDto(
            business.Id, business.NameArabic, business.NameEnglish, business.DescriptionArabic, business.DescriptionEnglish,
            business.PhoneNumber, business.AddressArabic, business.AddressEnglish, business.Category, business.IsActive,
            workingHours, services, currentQueue);
    }

    public async Task<BusinessDto> CreateAsync(CreateBusinessRequest request, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        var business = new Business
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            NameArabic = request.NameArabic.Trim(),
            NameEnglish = request.NameEnglish.Trim(),
            DescriptionArabic = request.DescriptionArabic.Trim(),
            DescriptionEnglish = request.DescriptionEnglish.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            AddressArabic = request.AddressArabic.Trim(),
            AddressEnglish = request.AddressEnglish.Trim(),
            Category = request.Category,
            IsActive = true,
            CreatedAt = _dateTimeProvider.UtcNow,
            UpdatedAt = _dateTimeProvider.UtcNow
        };

        await _context.AddEntityAsync(business, cancellationToken);

        foreach (DayOfWeek day in Enum.GetValues<DayOfWeek>())
        {
            var isFriday = day == DayOfWeek.Friday;
            var isSaturday = day == DayOfWeek.Saturday;
            await _context.AddEntityAsync(new BusinessWorkingHours
            {
                Id = Guid.NewGuid(),
                BusinessId = business.Id,
                DayOfWeek = day,
                OpeningTime = isFriday ? TimeOnly.MinValue : isSaturday ? new TimeOnly(10, 0) : new TimeOnly(9, 0),
                ClosingTime = isFriday ? TimeOnly.MinValue : isSaturday ? new TimeOnly(14, 0) : new TimeOnly(17, 0),
                IsClosed = isFriday
            }, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new BusinessDto(
            business.Id, business.NameArabic, business.NameEnglish, business.DescriptionArabic, business.DescriptionEnglish,
            business.PhoneNumber, business.AddressArabic, business.AddressEnglish, business.Category, business.IsActive);
    }

    public async Task<BusinessDto> UpdateAsync(Guid id, UpdateBusinessRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureOwnerAccessAsync(id, userId, cancellationToken);

        var business = await _context.Businesses.FirstAsync(b => b.Id == id, cancellationToken);
        business.NameArabic = request.NameArabic.Trim();
        business.NameEnglish = request.NameEnglish.Trim();
        business.DescriptionArabic = request.DescriptionArabic.Trim();
        business.DescriptionEnglish = request.DescriptionEnglish.Trim();
        business.PhoneNumber = request.PhoneNumber.Trim();
        business.AddressArabic = request.AddressArabic.Trim();
        business.AddressEnglish = request.AddressEnglish.Trim();
        business.Category = request.Category;
        business.IsActive = request.IsActive;
        business.UpdatedAt = _dateTimeProvider.UtcNow;

        _context.UpdateEntity(business);
        await _context.SaveChangesAsync(cancellationToken);

        return new BusinessDto(
            business.Id, business.NameArabic, business.NameEnglish, business.DescriptionArabic, business.DescriptionEnglish,
            business.PhoneNumber, business.AddressArabic, business.AddressEnglish, business.Category, business.IsActive);
    }

    public async Task EnsureOwnerOrStaffAccessAsync(Guid businessId, Guid userId, CancellationToken cancellationToken = default)
    {
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            ?? throw new NotFoundException("Business not found.");

        if (business.OwnerUserId == userId)
        {
            return;
        }

        var isStaff = await _context.BusinessStaff
            .AnyAsync(s => s.BusinessId == businessId && s.UserId == userId && s.IsActive, cancellationToken);

        if (!isStaff)
        {
            throw new UnauthorizedException("You do not have access to this business.", "BUSINESS_ACCESS_DENIED");
        }
    }

    private async Task EnsureOwnerAccessAsync(Guid businessId, Guid userId, CancellationToken cancellationToken)
    {
        var business = await _context.Businesses.FirstOrDefaultAsync(b => b.Id == businessId, cancellationToken)
            ?? throw new NotFoundException("Business not found.");

        if (business.OwnerUserId != userId)
        {
            throw new UnauthorizedException("Only the business owner can perform this action.", "OWNER_ACCESS_REQUIRED");
        }
    }
}

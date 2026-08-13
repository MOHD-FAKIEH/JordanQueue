using JordanQueue.Application.DTOs.Services;
using JordanQueue.Application.Exceptions;
using JordanQueue.Application.Interfaces;
using JordanQueue.Application.Interfaces.Businesses;
using JordanQueue.Application.Interfaces.Services;
using JordanQueue.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JordanQueue.Application.Services;

public class ServiceManagementService : IServiceManagementService
{
    private readonly IApplicationDbContext _context;
    private readonly IBusinessService _businessService;

    public ServiceManagementService(IApplicationDbContext context, IBusinessService businessService)
    {
        _context = context;
        _businessService = businessService;
    }

    public async Task<IReadOnlyList<ServiceDto>> GetByBusinessAsync(Guid businessId, CancellationToken cancellationToken = default)
    {
        return await _context.Services
            .Where(s => s.BusinessId == businessId)
            .OrderBy(s => s.NameEnglish)
            .Select(s => new ServiceDto(
                s.Id, s.BusinessId, s.NameArabic, s.NameEnglish, s.DescriptionArabic, s.DescriptionEnglish,
                s.AverageServiceMinutes, s.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceDto> CreateAsync(Guid businessId, CreateServiceRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await _businessService.EnsureOwnerOrStaffAccessAsync(businessId, userId, cancellationToken);

        var service = new Service
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            NameArabic = request.NameArabic.Trim(),
            NameEnglish = request.NameEnglish.Trim(),
            DescriptionArabic = request.DescriptionArabic.Trim(),
            DescriptionEnglish = request.DescriptionEnglish.Trim(),
            AverageServiceMinutes = request.AverageServiceMinutes,
            IsActive = true
        };

        await _context.AddEntityAsync(service, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceDto(
            service.Id, service.BusinessId, service.NameArabic, service.NameEnglish,
            service.DescriptionArabic, service.DescriptionEnglish, service.AverageServiceMinutes, service.IsActive);
    }

    public async Task<ServiceDto> UpdateAsync(Guid serviceId, UpdateServiceRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId, cancellationToken)
            ?? throw new NotFoundException("Service not found.");

        await _businessService.EnsureOwnerOrStaffAccessAsync(service.BusinessId, userId, cancellationToken);

        service.NameArabic = request.NameArabic.Trim();
        service.NameEnglish = request.NameEnglish.Trim();
        service.DescriptionArabic = request.DescriptionArabic.Trim();
        service.DescriptionEnglish = request.DescriptionEnglish.Trim();
        service.AverageServiceMinutes = request.AverageServiceMinutes;
        service.IsActive = request.IsActive;

        _context.UpdateEntity(service);
        await _context.SaveChangesAsync(cancellationToken);

        return new ServiceDto(
            service.Id, service.BusinessId, service.NameArabic, service.NameEnglish,
            service.DescriptionArabic, service.DescriptionEnglish, service.AverageServiceMinutes, service.IsActive);
    }
}

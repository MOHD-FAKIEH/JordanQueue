using JordanQueue.Application.DTOs.Services;

namespace JordanQueue.Application.Interfaces.Services;

public interface IServiceManagementService
{
    Task<IReadOnlyList<ServiceDto>> GetByBusinessAsync(Guid businessId, CancellationToken cancellationToken = default);
    Task<ServiceDto> CreateAsync(Guid businessId, CreateServiceRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<ServiceDto> UpdateAsync(Guid serviceId, UpdateServiceRequest request, Guid userId, CancellationToken cancellationToken = default);
}

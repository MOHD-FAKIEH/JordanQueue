using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Businesses;

namespace JordanQueue.Application.Interfaces.Businesses;

public interface IBusinessService
{
    Task<PagedResult<BusinessDto>> SearchAsync(BusinessSearchRequest request, CancellationToken cancellationToken = default);
    Task<BusinessDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusinessDto>> GetMineAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<BusinessDto> CreateAsync(CreateBusinessRequest request, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task<BusinessDto> UpdateAsync(Guid id, UpdateBusinessRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task EnsureOwnerOrStaffAccessAsync(Guid businessId, Guid userId, CancellationToken cancellationToken = default);
}

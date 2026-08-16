using JordanQueue.Application.DTOs.Staff;

namespace JordanQueue.Application.Interfaces.Staff;

public interface IStaffService
{
    Task<IReadOnlyList<StaffMemberDto>> GetByBusinessAsync(Guid businessId, Guid userId, CancellationToken cancellationToken = default);
    Task<StaffMemberDto> AddAsync(Guid businessId, AddStaffRequest request, Guid ownerUserId, CancellationToken cancellationToken = default);
    Task DeactivateAsync(Guid businessId, Guid staffId, Guid ownerUserId, CancellationToken cancellationToken = default);
}

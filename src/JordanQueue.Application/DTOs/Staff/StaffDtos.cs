namespace JordanQueue.Application.DTOs.Staff;

public record StaffMemberDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string MobileNumber,
    bool IsActive,
    DateTime CreatedAt);

public record AddStaffRequest(string Email);

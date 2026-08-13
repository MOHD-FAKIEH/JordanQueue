using JordanQueue.Domain.Common;
using JordanQueue.Domain.Enums;

namespace JordanQueue.Domain.Entities;

public class User : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public PreferredLanguage PreferredLanguage { get; set; } = PreferredLanguage.Arabic;
    public bool IsActive { get; set; } = true;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Business> OwnedBusinesses { get; set; } = new List<Business>();
    public ICollection<BusinessStaff> StaffAssignments { get; set; } = new List<BusinessStaff>();
    public ICollection<QueueTicket> QueueTickets { get; set; } = new List<QueueTicket>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

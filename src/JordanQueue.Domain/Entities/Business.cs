using JordanQueue.Domain.Common;
using JordanQueue.Domain.Enums;

namespace JordanQueue.Domain.Entities;

public class Business : AuditableEntity
{
    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string DescriptionArabic { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressArabic { get; set; } = string.Empty;
    public string AddressEnglish { get; set; } = string.Empty;
    public BusinessCategory Category { get; set; } = BusinessCategory.Other;
    public bool IsActive { get; set; } = true;

    public ICollection<BusinessStaff> Staff { get; set; } = new List<BusinessStaff>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<BusinessWorkingHours> WorkingHours { get; set; } = new List<BusinessWorkingHours>();
    public ICollection<Queue> Queues { get; set; } = new List<Queue>();
}

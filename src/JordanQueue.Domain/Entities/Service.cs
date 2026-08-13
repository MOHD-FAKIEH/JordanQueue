using JordanQueue.Domain.Common;

namespace JordanQueue.Domain.Entities;

public class Service : BaseEntity
{
    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public string NameArabic { get; set; } = string.Empty;
    public string NameEnglish { get; set; } = string.Empty;
    public string DescriptionArabic { get; set; } = string.Empty;
    public string DescriptionEnglish { get; set; } = string.Empty;
    public int AverageServiceMinutes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Queue> Queues { get; set; } = new List<Queue>();
}

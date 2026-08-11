namespace JordanQueue.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
}

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

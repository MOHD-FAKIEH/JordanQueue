namespace JordanQueue.Domain.Entities;

public class BusinessStaff
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}

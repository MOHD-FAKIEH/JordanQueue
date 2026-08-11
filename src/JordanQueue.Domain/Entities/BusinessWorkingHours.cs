namespace JordanQueue.Domain.Entities;

public class BusinessWorkingHours
{
    public Guid Id { get; set; }
    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpeningTime { get; set; }
    public TimeOnly ClosingTime { get; set; }
    public bool IsClosed { get; set; }
}

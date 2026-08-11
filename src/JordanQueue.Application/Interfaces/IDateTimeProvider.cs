namespace JordanQueue.Application.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateOnly TodayInAmman { get; }
}

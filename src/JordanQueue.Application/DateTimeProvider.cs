using JordanQueue.Application.Interfaces;

namespace JordanQueue.Application;

public class DateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo AmmanTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Asia/Amman");

    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly TodayInAmman
    {
        get
        {
            var ammanNow = TimeZoneInfo.ConvertTimeFromUtc(UtcNow, AmmanTimeZone);
            return DateOnly.FromDateTime(ammanNow);
        }
    }
}

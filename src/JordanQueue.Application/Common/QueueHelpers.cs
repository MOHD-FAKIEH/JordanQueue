namespace JordanQueue.Application.Common;

public static class TicketNumberFormatter
{
    public static string Format(int number) => $"A{number:D3}";
}

public static class QueueCalculator
{
    public static int CalculatePosition(IEnumerable<(string TicketNumber, Domain.Enums.TicketStatus Status)> tickets, string customerTicketNumber)
    {
        return tickets.Count(t =>
            t.Status is Domain.Enums.TicketStatus.Waiting or Domain.Enums.TicketStatus.Called or Domain.Enums.TicketStatus.Serving &&
            string.Compare(t.TicketNumber, customerTicketNumber, StringComparison.Ordinal) < 0);
    }

    public static int CalculateEstimatedWait(int peopleAhead, int averageServiceMinutes) =>
        peopleAhead * averageServiceMinutes;

    public static string? GetNowServing(IEnumerable<(string TicketNumber, Domain.Enums.TicketStatus Status)> tickets) =>
        tickets
            .Where(t => t.Status is Domain.Enums.TicketStatus.Called or Domain.Enums.TicketStatus.Serving)
            .OrderBy(t => t.TicketNumber, StringComparer.Ordinal)
            .Select(t => t.TicketNumber)
            .FirstOrDefault();
}

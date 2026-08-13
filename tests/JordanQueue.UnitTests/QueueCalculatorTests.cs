using JordanQueue.Application.Common;
using JordanQueue.Domain.Enums;

namespace JordanQueue.UnitTests;

public class QueueCalculatorTests
{
    [Fact]
    public void CalculatePosition_CountsActiveTicketsAhead()
    {
        var tickets = new List<(string TicketNumber, TicketStatus Status)>
        {
            ("A001", TicketStatus.Served),
            ("A002", TicketStatus.Served),
            ("A003", TicketStatus.Waiting),
            ("A004", TicketStatus.Waiting),
            ("A005", TicketStatus.Called),
            ("A006", TicketStatus.Waiting)
        };

        var position = QueueCalculator.CalculatePosition(tickets, "A006");

        Assert.Equal(3, position);
    }

    [Fact]
    public void CalculateEstimatedWait_MultipliesCorrectly()
    {
        Assert.Equal(45, QueueCalculator.CalculateEstimatedWait(3, 15));
    }

    [Fact]
    public void GetNowServing_ReturnsLowestActiveTicket()
    {
        var tickets = new List<(string TicketNumber, TicketStatus Status)>
        {
            ("A001", TicketStatus.Served),
            ("A002", TicketStatus.Called),
            ("A003", TicketStatus.Waiting)
        };

        Assert.Equal("A002", QueueCalculator.GetNowServing(tickets));
    }

    [Fact]
    public void FormatTicketNumber_PadsToThreeDigits()
    {
        Assert.Equal("A001", TicketNumberFormatter.Format(1));
        Assert.Equal("A023", TicketNumberFormatter.Format(23));
    }
}

using JordanQueue.Application;
using JordanQueue.Application.Interfaces;
using JordanQueue.Domain.Constants;
using JordanQueue.Domain.Entities;

namespace JordanQueue.UnitTests;

public class DateTimeProviderTests
{
    [Fact]
    public void TodayInAmman_ReturnsDateOnly()
    {
        var provider = new DateTimeProvider();
        var today = provider.TodayInAmman;

        Assert.True(today.Year >= 2024);
    }
}

public class ApiResponseTests
{
    [Fact]
    public void Ok_SetsSuccessTrue()
    {
        var response = Application.Common.ApiResponse<string>.Ok("test");

        Assert.True(response.Success);
        Assert.Equal("test", response.Data);
    }

    [Fact]
    public void Fail_SetsSuccessFalse()
    {
        var response = Application.Common.ApiResponse<string>.Fail("error");

        Assert.False(response.Success);
        Assert.Equal("error", response.Message);
    }
}

public class RoleNamesTests
{
    [Fact]
    public void RoleNames_ContainExpectedValues()
    {
        Assert.Equal("Customer", RoleNames.Customer);
        Assert.Equal("BusinessOwner", RoleNames.BusinessOwner);
        Assert.Equal("Staff", RoleNames.Staff);
        Assert.Equal("SystemAdmin", RoleNames.SystemAdmin);
    }
}

public class DomainEntityTests
{
    [Fact]
    public void User_HasDefaultPreferredLanguage()
    {
        var user = new User();
        Assert.Equal(Domain.Enums.PreferredLanguage.Arabic, user.PreferredLanguage);
    }

    [Fact]
    public void Queue_HasOpenStatusByDefault()
    {
        var queue = new Queue();
        Assert.Equal(Domain.Enums.QueueStatus.Open, queue.Status);
    }

    [Fact]
    public void QueueTicket_HasWaitingStatusByDefault()
    {
        var ticket = new QueueTicket();
        Assert.Equal(Domain.Enums.TicketStatus.Waiting, ticket.Status);
    }
}

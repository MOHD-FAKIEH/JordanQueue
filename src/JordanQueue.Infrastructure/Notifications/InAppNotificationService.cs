using JordanQueue.Application.Interfaces;
using JordanQueue.Application.Interfaces.Notifications;
using JordanQueue.Domain.Entities;
using JordanQueue.Domain.Enums;
using JordanQueue.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace JordanQueue.Infrastructure.Notifications;

public class InAppNotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<InAppNotificationService> _logger;

    public InAppNotificationService(
        ApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        ILogger<InAppNotificationService> logger)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task CreateInAppNotificationAsync(
        Guid userId,
        Guid? ticketId,
        string notificationType,
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<NotificationType>(notificationType, true, out var type))
        {
            type = NotificationType.General;
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TicketId = ticketId,
            NotificationType = type,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        await _context.NotificationsSet.AddAsync(notification, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created in-app notification {NotificationId} for user {UserId}", notification.Id, userId);
    }
}

public class LoggingNotificationSender : INotificationSender
{
    private readonly ILogger<LoggingNotificationSender> _logger;

    public LoggingNotificationSender(ILogger<LoggingNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Notification stub send: User={UserId}, Type={Type}, Title={Title}",
            request.UserId,
            request.NotificationType,
            request.Title);

        return Task.CompletedTask;
    }
}

namespace JordanQueue.Application.Interfaces.Notifications;

public interface INotificationSender
{
    Task SendAsync(NotificationRequest request, CancellationToken cancellationToken = default);
}

public record NotificationRequest(
    Guid UserId,
    Guid? TicketId,
    string NotificationType,
    string Title,
    string Message);

public interface INotificationService
{
    Task CreateInAppNotificationAsync(
        Guid userId,
        Guid? ticketId,
        string notificationType,
        string title,
        string message,
        CancellationToken cancellationToken = default);
}

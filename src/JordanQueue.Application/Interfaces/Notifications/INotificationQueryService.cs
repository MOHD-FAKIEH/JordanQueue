using JordanQueue.Application.DTOs.Notifications;

namespace JordanQueue.Application.Interfaces.Notifications;

public interface INotificationQueryService
{
    Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);
}

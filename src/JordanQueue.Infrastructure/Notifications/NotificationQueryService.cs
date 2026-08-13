using JordanQueue.Application.DTOs.Notifications;
using JordanQueue.Application.Interfaces;
using JordanQueue.Application.Interfaces.Notifications;
using Microsoft.EntityFrameworkCore;

namespace JordanQueue.Infrastructure.Notifications;

public class NotificationQueryService : INotificationQueryService
{
    private readonly IApplicationDbContext _context;

    public NotificationQueryService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationDto(n.Id, n.TicketId, n.NotificationType, n.Title, n.Message, n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, cancellationToken)
            ?? throw new Application.Exceptions.NotFoundException("Notification not found.");

        notification.IsRead = true;
        _context.UpdateEntity(notification);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

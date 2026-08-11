using JordanQueue.Domain.Common;
using JordanQueue.Domain.Enums;

namespace JordanQueue.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? TicketId { get; set; }
    public QueueTicket? Ticket { get; set; }

    public NotificationType NotificationType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

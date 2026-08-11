using JordanQueue.Domain.Common;
using JordanQueue.Domain.Enums;

namespace JordanQueue.Domain.Entities;

public class QueueTicket : BaseEntity
{
    public Guid QueueId { get; set; }
    public Queue Queue { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public string TicketNumber { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Waiting;
    public DateTime JoinedAt { get; set; }
    public DateTime? CalledAt { get; set; }
    public DateTime? ServedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int EstimatedWaitMinutes { get; set; }

    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

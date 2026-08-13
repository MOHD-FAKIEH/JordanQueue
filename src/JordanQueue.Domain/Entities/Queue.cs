using JordanQueue.Domain.Common;
using JordanQueue.Domain.Enums;

namespace JordanQueue.Domain.Entities;

public class Queue : BaseEntity
{
    public Guid BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public Guid ServiceId { get; set; }
    public Service Service { get; set; } = null!;

    public DateOnly QueueDate { get; set; }
    public QueueStatus Status { get; set; } = QueueStatus.Open;
    public int CurrentTicketNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<QueueTicket> Tickets { get; set; } = new List<QueueTicket>();
}

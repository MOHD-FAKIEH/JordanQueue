namespace JordanQueue.Domain.Enums;

public enum NotificationType
{
    QueueJoined = 0,
    TurnApproaching = 1,
    TurnNow = 2,
    TicketCancelled = 3,
    TicketSkipped = 4,
    QueuePaused = 5,
    QueueResumed = 6,
    General = 7
}

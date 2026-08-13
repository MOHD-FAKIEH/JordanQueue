using JordanQueue.Domain.Enums;

namespace JordanQueue.Application.DTOs.Notifications;

public record NotificationDto(
    Guid Id,
    Guid? TicketId,
    NotificationType NotificationType,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt);

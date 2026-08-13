using JordanQueue.Domain.Enums;

namespace JordanQueue.Application.DTOs.Queues;

public record JoinQueueRequest(Guid ServiceId);

public record OpenQueueRequest(Guid ServiceId);

public record QueueDto(
    Guid Id,
    Guid BusinessId,
    Guid ServiceId,
    string ServiceNameEnglish,
    DateOnly QueueDate,
    QueueStatus Status,
    int WaitingCount,
    string? NowServing,
    int EstimatedWaitMinutes);

public record QueuePositionDto(
    Guid TicketId,
    string TicketNumber,
    int Position,
    int EstimatedWaitMinutes,
    string? NowServing,
    TicketStatus Status);

public record TicketDto(
    Guid Id,
    Guid QueueId,
    Guid BusinessId,
    string BusinessNameEnglish,
    string ServiceNameEnglish,
    string TicketNumber,
    TicketStatus Status,
    int Position,
    int EstimatedWaitMinutes,
    string? NowServing,
    DateTime JoinedAt,
    DateTime? CalledAt,
    DateTime? ServedAt,
    DateTime? CancelledAt);

public record DailyStatsDto(
    int WaitingCount,
    int ServingCount,
    int ServedToday,
    double AverageWaitMinutes,
    double AverageServiceMinutes);

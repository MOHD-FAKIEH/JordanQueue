using JordanQueue.Application.DTOs.Queues;

namespace JordanQueue.Application.Interfaces.Queues;

public interface IQueueService
{
    Task<QueueDto> GetBusinessQueueAsync(Guid businessId, Guid? serviceId, CancellationToken cancellationToken = default);
    Task<QueueDto> OpenQueueAsync(Guid businessId, OpenQueueRequest request, Guid userId, CancellationToken cancellationToken = default);
    Task<QueueDto> PauseQueueAsync(Guid queueId, Guid userId, CancellationToken cancellationToken = default);
    Task<QueueDto> ResumeQueueAsync(Guid queueId, Guid userId, CancellationToken cancellationToken = default);
    Task<TicketDto> JoinQueueAsync(Guid businessId, JoinQueueRequest request, Guid customerId, CancellationToken cancellationToken = default);
    Task<QueueDto> GetQueueAsync(Guid queueId, CancellationToken cancellationToken = default);
    Task<QueuePositionDto> GetQueuePositionAsync(Guid queueId, Guid customerId, CancellationToken cancellationToken = default);
    Task<TicketDto> GetTicketAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketDto>> GetMyTicketsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<TicketDto> CallNextAsync(Guid queueId, Guid userId, CancellationToken cancellationToken = default);
    Task<TicketDto> ServeTicketAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default);
    Task<TicketDto> SkipTicketAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default);
    Task<TicketDto> CancelTicketAsync(Guid ticketId, Guid userId, bool isStaff, CancellationToken cancellationToken = default);
    Task<DailyStatsDto> GetDailyStatsAsync(Guid businessId, Guid userId, CancellationToken cancellationToken = default);
}

using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Queues;
using JordanQueue.Application.Exceptions;
using JordanQueue.Application.Interfaces;
using JordanQueue.Application.Interfaces.Businesses;
using JordanQueue.Application.Interfaces.Notifications;
using JordanQueue.Application.Interfaces.Queues;
using JordanQueue.Domain.Entities;
using JordanQueue.Domain.Enums;
using JordanQueue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JordanQueue.Infrastructure.Queues;

public class QueueService : IQueueService
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IBusinessService _businessService;
    private readonly INotificationService _notificationService;

    public QueueService(
        ApplicationDbContext context,
        IDateTimeProvider dateTimeProvider,
        IBusinessService businessService,
        INotificationService notificationService)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
        _businessService = businessService;
        _notificationService = notificationService;
    }

    public async Task<QueueDto> GetBusinessQueueAsync(Guid businessId, Guid? serviceId, CancellationToken cancellationToken = default)
    {
        var today = _dateTimeProvider.TodayInAmman;
        var query = _context.QueuesSet
            .Include(q => q.Service)
            .Where(q => q.BusinessId == businessId && q.QueueDate == today);

        if (serviceId.HasValue)
        {
            query = query.Where(q => q.ServiceId == serviceId.Value);
        }

        var queue = await query.OrderByDescending(q => q.CreatedAt).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("No queue found for today.", "QUEUE_NOT_FOUND");

        return await MapQueueDtoAsync(queue, cancellationToken);
    }

    public async Task<QueueDto> OpenQueueAsync(Guid businessId, OpenQueueRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        await _businessService.EnsureOwnerOrStaffAccessAsync(businessId, userId, cancellationToken);

        var service = await _context.ServicesSet.FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.BusinessId == businessId && s.IsActive, cancellationToken)
            ?? throw new NotFoundException("Service not found.");

        var today = _dateTimeProvider.TodayInAmman;
        var existing = await _context.QueuesSet.FirstOrDefaultAsync(q => q.BusinessId == businessId && q.ServiceId == request.ServiceId && q.QueueDate == today, cancellationToken);

        if (existing is not null)
        {
            if (existing.Status == QueueStatus.Closed)
            {
                throw new AppException("Queue is closed for today.", "QUEUE_CLOSED");
            }

            existing.Status = QueueStatus.Open;
            _context.Update(existing);
            await _context.SaveChangesAsync(cancellationToken);
            existing.Service = service;
            return await MapQueueDtoAsync(existing, cancellationToken);
        }

        var queue = new Queue
        {
            Id = Guid.NewGuid(),
            BusinessId = businessId,
            ServiceId = request.ServiceId,
            QueueDate = today,
            Status = QueueStatus.Open,
            CurrentTicketNumber = 0,
            CreatedAt = _dateTimeProvider.UtcNow
        };

        await _context.AddAsync(queue, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        queue.Service = service;
        return await MapQueueDtoAsync(queue, cancellationToken);
    }

    public async Task<QueueDto> PauseQueueAsync(Guid queueId, Guid userId, CancellationToken cancellationToken = default)
    {
        var queue = await GetQueueForStaffAsync(queueId, userId, cancellationToken);
        queue.Status = QueueStatus.Paused;
        _context.Update(queue);
        await _context.SaveChangesAsync(cancellationToken);
        return await MapQueueDtoAsync(queue, cancellationToken);
    }

    public async Task<QueueDto> ResumeQueueAsync(Guid queueId, Guid userId, CancellationToken cancellationToken = default)
    {
        var queue = await GetQueueForStaffAsync(queueId, userId, cancellationToken);
        queue.Status = QueueStatus.Open;
        _context.Update(queue);
        await _context.SaveChangesAsync(cancellationToken);
        return await MapQueueDtoAsync(queue, cancellationToken);
    }

    public async Task<TicketDto> JoinQueueAsync(Guid businessId, JoinQueueRequest request, Guid customerId, CancellationToken cancellationToken = default)
    {
        var business = await _context.BusinessesSet.FirstOrDefaultAsync(b => b.Id == businessId && b.IsActive, cancellationToken)
            ?? throw new NotFoundException("Business not found.");

        var service = await _context.ServicesSet.FirstOrDefaultAsync(s => s.Id == request.ServiceId && s.BusinessId == businessId && s.IsActive, cancellationToken)
            ?? throw new NotFoundException("Service not found.");

        var today = _dateTimeProvider.TodayInAmman;

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var queue = await _context.QueuesSet
            .FromSqlInterpolated($"SELECT * FROM Queues WITH (UPDLOCK, ROWLOCK) WHERE BusinessId = {businessId} AND ServiceId = {request.ServiceId} AND QueueDate = {today}")
            .Include(q => q.Service)
            .FirstOrDefaultAsync(cancellationToken);

        if (queue is null)
        {
            queue = new Queue
            {
                Id = Guid.NewGuid(),
                BusinessId = businessId,
                ServiceId = request.ServiceId,
                QueueDate = today,
                Status = QueueStatus.Open,
                CurrentTicketNumber = 0,
                CreatedAt = _dateTimeProvider.UtcNow
            };
            await _context.QueuesSet.AddAsync(queue, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            queue = await _context.QueuesSet
                .FromSqlInterpolated($"SELECT * FROM Queues WITH (UPDLOCK, ROWLOCK) WHERE Id = {queue.Id}")
                .Include(q => q.Service)
                .FirstAsync(cancellationToken);
        }

        if (queue.Status == QueueStatus.Closed)
        {
            throw new AppException("The queue is currently closed.", "QUEUE_CLOSED");
        }

        if (queue.Status == QueueStatus.Paused)
        {
            throw new AppException("The queue is currently paused.", "QUEUE_PAUSED");
        }

        var hasActiveTicket = await _context.QueueTicketsSet.AnyAsync(
            t => t.QueueId == queue.Id && t.CustomerId == customerId &&
                 (t.Status == TicketStatus.Waiting || t.Status == TicketStatus.Called || t.Status == TicketStatus.Serving),
            cancellationToken);

        if (hasActiveTicket)
        {
            throw new ConflictException("You already have an active ticket in this queue.", "ACTIVE_TICKET_EXISTS");
        }

        var waitingCount = await _context.QueueTicketsSet.CountAsync(t => t.QueueId == queue.Id && t.Status == TicketStatus.Waiting, cancellationToken);
        var estimatedWait = QueueCalculator.CalculateEstimatedWait(waitingCount, service.AverageServiceMinutes);

        queue.CurrentTicketNumber++;
        var ticketNumber = TicketNumberFormatter.Format(queue.CurrentTicketNumber);

        var ticket = new QueueTicket
        {
            Id = Guid.NewGuid(),
            QueueId = queue.Id,
            CustomerId = customerId,
            TicketNumber = ticketNumber,
            Status = TicketStatus.Waiting,
            JoinedAt = _dateTimeProvider.UtcNow,
            EstimatedWaitMinutes = estimatedWait
        };

        _context.Update(queue);
        await _context.QueueTicketsSet.AddAsync(ticket, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _notificationService.CreateInAppNotificationAsync(
            customerId, ticket.Id, NotificationType.QueueJoined.ToString(),
            "Queue joined", $"Your ticket number is {ticketNumber}.", cancellationToken);

        return await MapTicketDtoAsync(ticket, cancellationToken);
    }

    public async Task<QueueDto> GetQueueAsync(Guid queueId, CancellationToken cancellationToken = default)
    {
        var queue = await _context.QueuesSet.Include(q => q.Service).FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken)
            ?? throw new NotFoundException("Queue not found.");
        return await MapQueueDtoAsync(queue, cancellationToken);
    }

    public async Task<QueuePositionDto> GetQueuePositionAsync(Guid queueId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.QueueTicketsSet.FirstOrDefaultAsync(t => t.QueueId == queueId && t.CustomerId == customerId &&
            (t.Status == TicketStatus.Waiting || t.Status == TicketStatus.Called || t.Status == TicketStatus.Serving), cancellationToken)
            ?? throw new NotFoundException("Active ticket not found in this queue.");

        var tickets = await GetTicketSnapshotsAsync(queueId, cancellationToken);
        var position = QueueCalculator.CalculatePosition(tickets, ticket.TicketNumber);
        var nowServing = QueueCalculator.GetNowServing(tickets);
        var queue = await _context.QueuesSet.Include(q => q.Service).FirstAsync(q => q.Id == queueId, cancellationToken);
        var estimatedWait = QueueCalculator.CalculateEstimatedWait(position, queue.Service.AverageServiceMinutes);

        return new QueuePositionDto(ticket.Id, ticket.TicketNumber, position, estimatedWait, nowServing, ticket.Status);
    }

    public async Task<TicketDto> GetTicketAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.QueueTicketsSet.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken)
            ?? throw new NotFoundException("Ticket not found.");

        if (ticket.CustomerId != userId)
        {
            var queue = await _context.QueuesSet.FirstAsync(q => q.Id == ticket.QueueId, cancellationToken);
            await _businessService.EnsureOwnerOrStaffAccessAsync(queue.BusinessId, userId, cancellationToken);
        }

        return await MapTicketDtoAsync(ticket, cancellationToken);
    }

    public async Task<IReadOnlyList<TicketDto>> GetMyTicketsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var tickets = await _context.QueueTicketsSet
            .Where(t => t.CustomerId == customerId)
            .OrderByDescending(t => t.JoinedAt)
            .Take(50)
            .ToListAsync(cancellationToken);

        var result = new List<TicketDto>();
        foreach (var ticket in tickets)
        {
            result.Add(await MapTicketDtoAsync(ticket, cancellationToken));
        }

        return result;
    }

    public async Task<TicketDto> CallNextAsync(Guid queueId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _businessService.EnsureOwnerOrStaffAccessAsync(
            (await _context.QueuesSet.FirstAsync(q => q.Id == queueId, cancellationToken)).BusinessId,
            userId, cancellationToken);

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var queue = await _context.QueuesSet
            .FromSqlInterpolated($"SELECT * FROM Queues WITH (UPDLOCK, ROWLOCK) WHERE Id = {queueId}")
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Queue not found.");

        if (queue.Status != QueueStatus.Open)
        {
            throw new AppException("Queue is not accepting customers.", queue.Status == QueueStatus.Paused ? "QUEUE_PAUSED" : "QUEUE_CLOSED");
        }

        var ticket = await _context.QueueTicketsSet
            .FromSqlInterpolated($"""
                SELECT TOP 1 * FROM QueueTickets WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE QueueId = {queueId} AND Status = {(int)TicketStatus.Waiting}
                ORDER BY TicketNumber
                """)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AppException("No waiting customers.", "NO_WAITING_CUSTOMERS");

        ticket.Status = TicketStatus.Called;
        ticket.CalledAt = _dateTimeProvider.UtcNow;
        _context.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await _notificationService.CreateInAppNotificationAsync(
            ticket.CustomerId, ticket.Id, NotificationType.TurnNow.ToString(),
            "Your turn!", "Please proceed to the service counter.", cancellationToken);

        return await MapTicketDtoAsync(ticket, cancellationToken);
    }

    public async Task<TicketDto> ServeTicketAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default)
    {
        var ticket = await GetTicketForStaffAsync(ticketId, userId, cancellationToken);

        if (ticket.Status is not (TicketStatus.Called or TicketStatus.Serving))
        {
            throw new AppException("Ticket must be called before serving.", "INVALID_TICKET_STATE");
        }

        ticket.Status = TicketStatus.Served;
        ticket.ServedAt = _dateTimeProvider.UtcNow;
        _context.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapTicketDtoAsync(ticket, cancellationToken);
    }

    public async Task<TicketDto> SkipTicketAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken = default)
    {
        var ticket = await GetTicketForStaffAsync(ticketId, userId, cancellationToken);

        if (ticket.Status is TicketStatus.Served or TicketStatus.Cancelled or TicketStatus.Skipped)
        {
            throw new AppException("Ticket cannot be skipped.", "INVALID_TICKET_STATE");
        }

        ticket.Status = TicketStatus.Skipped;
        _context.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateInAppNotificationAsync(
            ticket.CustomerId, ticket.Id, NotificationType.TicketSkipped.ToString(),
            "Ticket skipped", "Your ticket was skipped. Please rejoin if needed.", cancellationToken);

        return await MapTicketDtoAsync(ticket, cancellationToken);
    }

    public async Task<TicketDto> CancelTicketAsync(Guid ticketId, Guid userId, bool isStaff, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.QueueTicketsSet.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken)
            ?? throw new NotFoundException("Ticket not found.");

        if (isStaff)
        {
            await GetTicketForStaffAsync(ticketId, userId, cancellationToken);
        }
        else
        {
            if (ticket.CustomerId != userId)
            {
                throw new UnauthorizedException("You can only cancel your own ticket.");
            }
        }

        if (ticket.Status is not TicketStatus.Waiting)
        {
            throw new AppException("Only waiting tickets can be cancelled.", "INVALID_TICKET_STATE");
        }

        ticket.Status = TicketStatus.Cancelled;
        ticket.CancelledAt = _dateTimeProvider.UtcNow;
        _context.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);

        return await MapTicketDtoAsync(ticket, cancellationToken);
    }

    public async Task<IReadOnlyList<TicketDto>> GetQueueTicketsAsync(Guid queueId, Guid userId, CancellationToken cancellationToken = default)
    {
        await GetQueueForStaffAsync(queueId, userId, cancellationToken);

        var tickets = await _context.QueueTicketsSet
            .Where(t => t.QueueId == queueId)
            .OrderBy(t => t.TicketNumber)
            .ToListAsync(cancellationToken);

        var result = new List<TicketDto>();
        foreach (var ticket in tickets)
        {
            result.Add(await MapTicketDtoAsync(ticket, cancellationToken));
        }

        return result;
    }

    public async Task<DailyStatsDto> GetDailyStatsAsync(Guid businessId, Guid userId, CancellationToken cancellationToken = default)
    {
        await _businessService.EnsureOwnerOrStaffAccessAsync(businessId, userId, cancellationToken);

        var today = _dateTimeProvider.TodayInAmman;
        var queueIds = await _context.QueuesSet
            .Where(q => q.BusinessId == businessId && q.QueueDate == today)
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);

        var tickets = await _context.QueueTicketsSet
            .Where(t => queueIds.Contains(t.QueueId))
            .ToListAsync(cancellationToken);

        var waiting = tickets.Count(t => t.Status == TicketStatus.Waiting);
        var serving = tickets.Count(t => t.Status is TicketStatus.Called or TicketStatus.Serving);
        var served = tickets.Count(t => t.Status == TicketStatus.Served);

        var servedTickets = tickets.Where(t => t.Status == TicketStatus.Served && t.CalledAt.HasValue).ToList();
        var avgWait = servedTickets.Count == 0
            ? 0
            : servedTickets.Average(t => (t.CalledAt!.Value - t.JoinedAt).TotalMinutes);

        var avgService = servedTickets.Count == 0
            ? 0
            : servedTickets.Where(t => t.ServedAt.HasValue)
                .Average(t => (t.ServedAt!.Value - t.CalledAt!.Value).TotalMinutes);

        return new DailyStatsDto(waiting, serving, served, Math.Round(avgWait, 1), Math.Round(avgService, 1));
    }

    private async Task<Queue> GetQueueForStaffAsync(Guid queueId, Guid userId, CancellationToken cancellationToken)
    {
        var queue = await _context.QueuesSet.Include(q => q.Service).FirstOrDefaultAsync(q => q.Id == queueId, cancellationToken)
            ?? throw new NotFoundException("Queue not found.");

        await _businessService.EnsureOwnerOrStaffAccessAsync(queue.BusinessId, userId, cancellationToken);
        return queue;
    }

    private async Task<QueueTicket> GetTicketForStaffAsync(Guid ticketId, Guid userId, CancellationToken cancellationToken)
    {
        var ticket = await _context.QueueTicketsSet.FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken)
            ?? throw new NotFoundException("Ticket not found.");

        var queue = await _context.QueuesSet.FirstAsync(q => q.Id == ticket.QueueId, cancellationToken);
        await _businessService.EnsureOwnerOrStaffAccessAsync(queue.BusinessId, userId, cancellationToken);
        return ticket;
    }

    private async Task<QueueDto> MapQueueDtoAsync(Queue queue, CancellationToken cancellationToken)
    {
        var tickets = await GetTicketSnapshotsAsync(queue.Id, cancellationToken);
        var waitingCount = tickets.Count(t => t.Status == TicketStatus.Waiting);
        var nowServing = QueueCalculator.GetNowServing(tickets);
        var estimatedWait = QueueCalculator.CalculateEstimatedWait(waitingCount, queue.Service.AverageServiceMinutes);

        return new QueueDto(
            queue.Id, queue.BusinessId, queue.ServiceId, queue.Service.NameEnglish,
            queue.QueueDate, queue.Status, waitingCount, nowServing, estimatedWait);
    }

    private async Task<TicketDto> MapTicketDtoAsync(QueueTicket ticket, CancellationToken cancellationToken)
    {
        var queue = await _context.QueuesSet.Include(q => q.Service).Include(q => q.Business).FirstAsync(q => q.Id == ticket.QueueId, cancellationToken);
        var tickets = await GetTicketSnapshotsAsync(queue.Id, cancellationToken);
        var position = ticket.Status == TicketStatus.Waiting
            ? QueueCalculator.CalculatePosition(tickets, ticket.TicketNumber)
            : 0;
        var nowServing = QueueCalculator.GetNowServing(tickets);
        var estimatedWait = QueueCalculator.CalculateEstimatedWait(position, queue.Service.AverageServiceMinutes);

        return new TicketDto(
            ticket.Id, queue.Id, queue.BusinessId, queue.Business.NameEnglish, queue.Service.NameEnglish,
            ticket.TicketNumber, ticket.Status, position, estimatedWait, nowServing,
            ticket.JoinedAt, ticket.CalledAt, ticket.ServedAt, ticket.CancelledAt);
    }

    private async Task<List<(string TicketNumber, TicketStatus Status)>> GetTicketSnapshotsAsync(Guid queueId, CancellationToken cancellationToken)
    {
        return await _context.QueueTicketsSet
            .Where(t => t.QueueId == queueId)
            .Select(t => new ValueTuple<string, TicketStatus>(t.TicketNumber, t.Status))
            .ToListAsync(cancellationToken);
    }
}

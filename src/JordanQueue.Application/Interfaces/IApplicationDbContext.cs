using JordanQueue.Domain.Entities;

namespace JordanQueue.Application.Interfaces;

public interface IApplicationDbContext
{
    IQueryable<User> Users { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<UserRole> UserRoles { get; }
    IQueryable<Business> Businesses { get; }
    IQueryable<BusinessStaff> BusinessStaff { get; }
    IQueryable<Service> Services { get; }
    IQueryable<BusinessWorkingHours> BusinessWorkingHours { get; }
    IQueryable<Queue> Queues { get; }
    IQueryable<QueueTicket> QueueTickets { get; }
    IQueryable<Notification> Notifications { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }

    Task AddEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class;
    void UpdateEntity<TEntity>(TEntity entity) where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

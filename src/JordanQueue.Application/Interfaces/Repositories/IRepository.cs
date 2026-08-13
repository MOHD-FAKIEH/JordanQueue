using JordanQueue.Application.Interfaces;
using JordanQueue.Domain.Entities;

namespace JordanQueue.Application.Interfaces.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}

public interface IUnitOfWork
{
    IRepository<User> Users { get; }
    IRepository<Business> Businesses { get; }
    IRepository<Queue> Queues { get; }
    IRepository<QueueTicket> QueueTickets { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

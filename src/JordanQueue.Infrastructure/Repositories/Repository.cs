using JordanQueue.Application.Interfaces.Repositories;
using JordanQueue.Domain.Entities;
using JordanQueue.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace JordanQueue.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await DbSet.FindAsync([id], cancellationToken);

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Remove(TEntity entity) => DbSet.Remove(entity);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new Repository<User>(context);
        Businesses = new Repository<Business>(context);
        Queues = new Repository<Queue>(context);
        QueueTickets = new Repository<QueueTicket>(context);
    }

    public IRepository<User> Users { get; }
    public IRepository<Business> Businesses { get; }
    public IRepository<Queue> Queues { get; }
    public IRepository<QueueTicket> QueueTickets { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}

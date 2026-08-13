using JordanQueue.Application.Interfaces;
using JordanQueue.Domain.Entities;
using JordanQueue.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace JordanQueue.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly AuditableEntityInterceptor _auditableEntityInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor auditableEntityInterceptor)
        : base(options)
    {
        _auditableEntityInterceptor = auditableEntityInterceptor;
    }

    public DbSet<User> UsersSet => Set<User>();
    public DbSet<Role> RolesSet => Set<Role>();
    public DbSet<UserRole> UserRolesSet => Set<UserRole>();
    public DbSet<Business> BusinessesSet => Set<Business>();
    public DbSet<BusinessStaff> BusinessStaffSet => Set<BusinessStaff>();
    public DbSet<Service> ServicesSet => Set<Service>();
    public DbSet<BusinessWorkingHours> BusinessWorkingHoursSet => Set<BusinessWorkingHours>();
    public DbSet<Queue> QueuesSet => Set<Queue>();
    public DbSet<QueueTicket> QueueTicketsSet => Set<QueueTicket>();
    public DbSet<Notification> NotificationsSet => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokensSet => Set<RefreshToken>();

    IQueryable<User> IApplicationDbContext.Users => UsersSet.AsQueryable();
    IQueryable<Role> IApplicationDbContext.Roles => RolesSet.AsQueryable();
    IQueryable<UserRole> IApplicationDbContext.UserRoles => UserRolesSet.AsQueryable();
    IQueryable<Business> IApplicationDbContext.Businesses => BusinessesSet.AsQueryable();
    IQueryable<BusinessStaff> IApplicationDbContext.BusinessStaff => BusinessStaffSet.AsQueryable();
    IQueryable<Service> IApplicationDbContext.Services => ServicesSet.AsQueryable();
    IQueryable<BusinessWorkingHours> IApplicationDbContext.BusinessWorkingHours => BusinessWorkingHoursSet.AsQueryable();
    IQueryable<Queue> IApplicationDbContext.Queues => QueuesSet.AsQueryable();
    IQueryable<QueueTicket> IApplicationDbContext.QueueTickets => QueueTicketsSet.AsQueryable();
    IQueryable<Notification> IApplicationDbContext.Notifications => NotificationsSet.AsQueryable();
    IQueryable<RefreshToken> IApplicationDbContext.RefreshTokens => RefreshTokensSet.AsQueryable();

    Task IApplicationDbContext.AddEntityAsync<TEntity>(TEntity entity, CancellationToken cancellationToken) =>
        Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();

    void IApplicationDbContext.UpdateEntity<TEntity>(TEntity entity) =>
        Set<TEntity>().Update(entity);

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntityInterceptor);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

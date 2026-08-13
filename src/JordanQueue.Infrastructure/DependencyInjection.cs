using JordanQueue.Application.Common;
using JordanQueue.Application.Interfaces;
using JordanQueue.Application.Interfaces.Auth;
using JordanQueue.Application.Interfaces.Notifications;
using JordanQueue.Application.Interfaces.Queues;
using JordanQueue.Application.Interfaces.Repositories;
using JordanQueue.Infrastructure.Notifications;
using JordanQueue.Infrastructure.Persistence;
using JordanQueue.Infrastructure.Persistence.Interceptors;
using JordanQueue.Infrastructure.Persistence.Seed;
using JordanQueue.Infrastructure.Queues;
using JordanQueue.Infrastructure.Repositories;
using JordanQueue.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JordanQueue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<AuditableEntityInterceptor>();

        if (environment?.IsEnvironment("Testing") != true)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sqlOptions.EnableRetryOnFailure();
                }));
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IQueueService, QueueService>();
        services.AddScoped<INotificationService, InAppNotificationService>();
        services.AddScoped<INotificationQueryService, NotificationQueryService>();
        services.AddScoped<INotificationSender, LoggingNotificationSender>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await context.Database.MigrateAsync();
        await DatabaseSeeder.SeedAsync(context, logger);
    }
}

using JordanQueue.Application;
using JordanQueue.Infrastructure.Persistence;
using JordanQueue.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace JordanQueue.Infrastructure.Persistence;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../JordanQueue.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        return new ApplicationDbContext(optionsBuilder.Options, new AuditableEntityInterceptor(new DateTimeProvider()));
    }
}

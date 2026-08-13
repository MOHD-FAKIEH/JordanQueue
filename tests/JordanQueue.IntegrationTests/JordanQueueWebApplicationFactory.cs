using JordanQueue.Application.Interfaces;
using JordanQueue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace JordanQueue.IntegrationTests;

public class JordanQueueWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"JordanQueueTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
            services.RemoveAll(typeof(ApplicationDbContext));
            services.RemoveAll(typeof(IApplicationDbContext));
            services.RemoveAll(typeof(IHealthCheck));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        });
    }
}

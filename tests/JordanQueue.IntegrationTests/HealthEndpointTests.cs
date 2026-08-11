using System.Net;
using System.Net.Http.Json;
using JordanQueue.Application.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JordanQueue.IntegrationTests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(DbContextOptions<JordanQueue.Infrastructure.Persistence.ApplicationDbContext>));
                services.AddDbContext<JordanQueue.Infrastructure.Persistence.ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase("JordanQueueTests"));
            });
        });
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ApiHealthController_ReturnsSuccessResponse()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<Dictionary<string, object>>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
    }
}

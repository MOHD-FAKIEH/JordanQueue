using System.Net;
using System.Net.Http.Json;
using JordanQueue.Application.Common;
using JordanQueue.Application.DTOs.Auth;
using JordanQueue.Domain.Constants;
using JordanQueue.Domain.Entities;
using JordanQueue.Infrastructure.Persistence;
using JordanQueue.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JordanQueue.IntegrationTests;

public class HealthEndpointTests : IClassFixture<JordanQueueWebApplicationFactory>
{
    private readonly JordanQueueWebApplicationFactory _factory;

    public HealthEndpointTests(JordanQueueWebApplicationFactory factory)
    {
        _factory = factory;
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

public class AuthIntegrationTests : IClassFixture<JordanQueueWebApplicationFactory>
{
    private readonly JordanQueueWebApplicationFactory _factory;

    public AuthIntegrationTests(JordanQueueWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithSeedCustomer_ReturnsToken()
    {
        await SeedRolesAndCustomerAsync();

        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("customer1@jordanqueue.dev", "Customer123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.False(string.IsNullOrWhiteSpace(payload.Data!.AccessToken));
        Assert.Contains(RoleNames.Customer, payload.Data.Roles);
    }

    [Fact]
    public async Task Register_NewCustomer_ReturnsToken()
    {
        await SeedRolesAsync();

        var suffix = Guid.NewGuid().ToString("N")[..8];
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(
            "Test", "User", $"+96279{suffix}", $"user{suffix}@test.com", "Password123!", RoleNames.Customer));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();
        Assert.NotNull(payload?.Data?.AccessToken);
    }

    private async Task SeedRolesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        if (await context.RolesSet.AnyAsync(r => r.Name == RoleNames.Customer))
        {
            return;
        }

        context.RolesSet.AddRange(
            new Role { Id = Guid.NewGuid(), Name = RoleNames.Customer, Description = "Customer" },
            new Role { Id = Guid.NewGuid(), Name = RoleNames.BusinessOwner, Description = "Owner" },
            new Role { Id = Guid.NewGuid(), Name = RoleNames.Staff, Description = "Staff" });

        await context.SaveChangesAsync();
    }

    private async Task SeedRolesAndCustomerAsync()
    {
        await SeedRolesAsync();

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (await context.UsersSet.AnyAsync(u => u.Email == "customer1@jordanqueue.dev"))
        {
            return;
        }

        var customerRole = await context.RolesSet.FirstOrDefaultAsync(r => r.Name == RoleNames.Customer);
        if (customerRole is null)
        {
            throw new InvalidOperationException("Customer role was not seeded.");
        }
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Customer",
            Email = "customer1@jordanqueue.dev",
            MobileNumber = "+962790000006",
            PasswordHash = DatabaseSeeder.HashPassword("Customer123!"),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.UsersSet.Add(user);
        context.UserRolesSet.Add(new UserRole { UserId = user.Id, RoleId = customerRole.Id });
        await context.SaveChangesAsync();
    }
}

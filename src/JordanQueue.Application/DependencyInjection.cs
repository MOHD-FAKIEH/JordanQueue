using System.Reflection;
using FluentValidation;
using JordanQueue.Application.Interfaces;
using JordanQueue.Application.Interfaces.Auth;
using JordanQueue.Application.Interfaces.Businesses;
using JordanQueue.Application.Interfaces.Services;
using JordanQueue.Application.Interfaces.Staff;
using JordanQueue.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace JordanQueue.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBusinessService, BusinessService>();
        services.AddScoped<IServiceManagementService, ServiceManagementService>();
        services.AddScoped<IStaffService, StaffService>();
        return services;
    }
}

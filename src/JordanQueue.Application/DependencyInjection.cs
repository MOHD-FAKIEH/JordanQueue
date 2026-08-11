using System.Reflection;
using FluentValidation;
using JordanQueue.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace JordanQueue.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        return services;
    }
}

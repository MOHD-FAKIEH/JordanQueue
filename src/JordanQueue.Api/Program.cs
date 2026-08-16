using System.Net;
using System.Text.Json;
using JordanQueue.Api;
using JordanQueue.Application;
using JordanQueue.Application.Common;
using JordanQueue.Application.Exceptions;
using JordanQueue.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddHttpContextAccessor();
builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new ApiError
                {
                    Code = "VALIDATION_ERROR",
                    Message = e.ErrorMessage
                }))
                .ToList();

            var response = ApiResponse<object>.Fail("Validation failed.", errors.ToArray());
            return new BadRequestObjectResult(response);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiSwagger();

var healthChecksBuilder = builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

if (!builder.Environment.IsEnvironment("Testing"))
{
    healthChecksBuilder.AddSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "sqlserver",
        tags: ["ready"]);
}

builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Clients", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:8081",
                "http://127.0.0.1:8081")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        var (statusCode, response) = exception switch
        {
            ValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                ApiResponse<object>.Fail(
                    validationException.Message,
                    validationException.ValidationErrors
                        .Select(e => new ApiError { Code = "VALIDATION_ERROR", Message = e })
                        .ToArray())),

            AppException appException => (
                appException.StatusCode,
                ApiResponse<object>.Fail(
                    appException.Message,
                    new ApiError { Code = appException.Code, Message = appException.Message })),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                ApiResponse<object>.Fail(
                    "An unexpected error occurred.",
                    new ApiError { Code = "INTERNAL_ERROR", Message = "An unexpected error occurred." }))
        };

        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Clients");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => !check.Tags.Contains("ready")
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

if (app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("Testing"))
{
    await app.Services.InitializeDatabaseAsync();
}

app.Run();

public partial class Program;

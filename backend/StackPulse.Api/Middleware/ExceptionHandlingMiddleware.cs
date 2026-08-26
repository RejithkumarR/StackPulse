using System.Net;
using System.Text.Json;
using StackPulse.Api.Data;
using StackPulse.Api.Models.Mongo;

namespace StackPulse.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly MongoStackPulseContext _mongoContext;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, MongoStackPulseContext mongoContext)
    {
        _next = next;
        _logger = logger;
        _mongoContext = mongoContext;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {RequestPath}", context.Request.Path);
            await WriteMongoLogAsync(context, ex);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                InvalidOperationException => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.InternalServerError,
            };

            var payload = new
            {
                success = false,
                message = context.Response.StatusCode == 500 ? "An unexpected error occurred." : ex.Message,
                errors = new[] { ex.Message }
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }

    private async Task WriteMongoLogAsync(HttpContext context, Exception exception)
    {
        if (!_mongoContext.IsConfigured)
        {
            return;
        }

        try
        {
            await _mongoContext.ApplicationLogs.InsertOneAsync(new ApplicationLogEntry
            {
                Level = "Error",
                Category = "UnhandledException",
                Message = $"{context.Request.Method} {context.Request.Path}: {exception.Message}",
                TraceId = context.TraceIdentifier,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception logException)
        {
            _logger.LogWarning(logException, "Unable to write exception log to MongoDB");
        }
    }
}

using StackPulse.Api.Data;
using StackPulse.Api.Models.Mongo;

namespace StackPulse.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly MongoStackPulseContext _mongoContext;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, MongoStackPulseContext mongoContext)
    {
        _next = next;
        _logger = logger;
        _mongoContext = mongoContext;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Request {Method} {Path}", context.Request.Method, context.Request.Path);

        await _next(context);

        _logger.LogInformation("Response {Method} {Path} -> {StatusCode}", context.Request.Method, context.Request.Path, context.Response.StatusCode);
        await WriteMongoLogAsync(context);
    }

    private async Task WriteMongoLogAsync(HttpContext context)
    {
        if (!_mongoContext.IsConfigured)
        {
            return;
        }

        try
        {
            await _mongoContext.ApplicationLogs.InsertOneAsync(new ApplicationLogEntry
            {
                Level = "Information",
                Category = "HttpRequest",
                Message = $"{context.Request.Method} {context.Request.Path} -> {context.Response.StatusCode}",
                TraceId = context.TraceIdentifier,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Unable to write application log to MongoDB");
        }
    }
}

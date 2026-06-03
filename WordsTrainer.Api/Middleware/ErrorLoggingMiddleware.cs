using System.Security.Claims;
using WordsTrainer.Core.Entities;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Api.Middleware;

public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ErrorLoggingMiddleware> _logger;

    public ErrorLoggingMiddleware(
        RequestDelegate next,
        IServiceScopeFactory scopeFactory,
        ILogger<ErrorLoggingMiddleware> logger)
    {
        _next = next;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled API exception. TraceId={TraceId}", context.TraceIdentifier);

            await TryWriteErrorLogAsync(context, ex);

            if (context.Response.HasStarted)
                throw;

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Unexpected server error.",
                traceId = context.TraceIdentifier
            });
        }
    }

    private async Task TryWriteErrorLogAsync(HttpContext context, Exception exception)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.ErrorLogs.Add(new ErrorLog
            {
                Id = Guid.NewGuid(),
                CreatedAtUtc = DateTime.UtcNow,
                Level = "Error",
                Message = exception.Message,
                ExceptionType = exception.GetType().FullName ?? exception.GetType().Name,
                StackTrace = exception.ToString(),
                RequestMethod = context.Request.Method,
                RequestPath = context.Request.Path.Value,
                QueryString = context.Request.QueryString.HasValue
                    ? context.Request.QueryString.Value
                    : null,
                UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier),
                RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                UserAgent = context.Request.Headers.UserAgent.ToString(),
                TraceId = context.TraceIdentifier
            });

            await db.SaveChangesAsync(context.RequestAborted);
        }
        catch (Exception logException)
        {
            _logger.LogError(logException, "Failed to write API error log. Original TraceId={TraceId}", context.TraceIdentifier);
        }
    }
}

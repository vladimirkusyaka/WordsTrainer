using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using WordsTrainer.Api.Middleware;
using WordsTrainer.Infrastructure.Data;

namespace WordsTrainer.Tests;

public class ErrorLoggingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenUnhandledExceptionOccurs_WritesErrorLogAndReturnsGeneric500()
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        await using var serviceProvider = services.BuildServiceProvider();
        var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();

        var middleware = new ErrorLoggingMiddleware(
            _ => throw new InvalidOperationException("Boom from test"),
            scopeFactory,
            NullLogger<ErrorLoggingMiddleware>.Instance);

        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        context.TraceIdentifier = "trace-123";
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/api/training/answer";
        context.Request.QueryString = new QueryString("?debug=true");
        context.Request.Headers.UserAgent = "test-agent";
        context.Response.Body = new MemoryStream();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "user-123")],
            authenticationType: "Test"));

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.Contains("Unexpected server error", body);
        Assert.Contains("trace-123", body);

        await using var scope = serviceProvider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var errorLog = await db.ErrorLogs.SingleAsync();

        Assert.Equal("Error", errorLog.Level);
        Assert.Equal("Boom from test", errorLog.Message);
        Assert.Equal(typeof(InvalidOperationException).FullName, errorLog.ExceptionType);
        Assert.Contains("Boom from test", errorLog.StackTrace);
        Assert.Equal(HttpMethods.Post, errorLog.RequestMethod);
        Assert.Equal("/api/training/answer", errorLog.RequestPath);
        Assert.Equal("?debug=true", errorLog.QueryString);
        Assert.Equal("user-123", errorLog.UserId);
        Assert.Equal("test-agent", errorLog.UserAgent);
        Assert.Equal("trace-123", errorLog.TraceId);
    }
}

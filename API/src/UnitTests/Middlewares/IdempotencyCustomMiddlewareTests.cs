using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Exceptions;
using Web.Api.Middlewares;
using Xunit;

namespace Application.UnitTests.Middlewares;

public class IdempotencyCustomMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_PostWithoutIdempotencyKey_ThrowsMissingKeyException()
    {
        var middleware = new IdempotencyCustomMiddleware(_ => Task.CompletedTask, Mock.Of<ILogger<IdempotencyCustomMiddleware>>());
        var context = HttpContext("POST", "/api/orders");

        await Assert.ThrowsAsync<IdempotencyKeyMissingException>(() => middleware.InvokeAsync(context));
    }

    [Theory]
    [InlineData("GET", "/api/orders")]
    [InlineData("POST", "/Hub/chatHub")]
    [InlineData("POST", "/api/notifications/stream")]
    public async Task InvokeAsync_NonMutatingOrExcludedPath_CallsNextWithoutKey(string method, string path)
    {
        var called = false;
        var middleware = new IdempotencyCustomMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        }, Mock.Of<ILogger<IdempotencyCustomMiddleware>>());
        var context = HttpContext(method, path);

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task InvokeAsync_PostWithKey_CopiesSuccessfulResponseBody()
    {
        var middleware = new IdempotencyCustomMiddleware(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status201Created;
            await context.Response.WriteAsync("created");
        }, Mock.Of<ILogger<IdempotencyCustomMiddleware>>());
        var context = HttpContext("POST", "/api/orders");
        context.Request.Headers["IdempotencyKey"] = "key-1";

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();

        Assert.Equal(StatusCodes.Status201Created, context.Response.StatusCode);
        Assert.Equal("created", body);
    }

    [Fact]
    public async Task InvokeAsync_PostWithDuplicateIdempotencyResponse_ThrowsDuplicateException()
    {
        var middleware = new IdempotencyCustomMiddleware(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsync("Duplicate Idempotency Key");
        }, Mock.Of<ILogger<IdempotencyCustomMiddleware>>());
        var context = HttpContext("POST", "/api/orders");
        context.Request.Headers["IdempotencyKey"] = "key-1";

        await Assert.ThrowsAsync<IdempotencyKeyDuplicateException>(() => middleware.InvokeAsync(context));
    }

    private static DefaultHttpContext HttpContext(string method, string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        return context;
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Exceptions;
using System.Text.Json;
using Web.Api.Middlewares;
using Xunit;

namespace Application.UnitTests.Middlewares;

public class CustomExceptionHandlerMiddlewareTests
{
    [Theory]
    [InlineData(typeof(NotFoundException), 404, "NotFound")]
    [InlineData(typeof(UserAlreadyExistException), 409, "AlreadyExists")]
    [InlineData(typeof(RateLimitExceededException), 429, "LimitExceeded")]
    [InlineData(typeof(UnauthorizedException), 401, "Unauthorized")]
    [InlineData(typeof(UnprocessableContentException), 422, "BusinessRule")]
    [InlineData(typeof(IdempotencyKeyDuplicateException), 406, "Conflict")]
    [InlineData(typeof(IdempotencyKeyMissingException), 422, "Validation")]
    [InlineData(typeof(BadRequestException), 400, "Validation")]
    [InlineData(typeof(GeminiException), 422, "ExternalService")]
    public async Task InvokeAsync_KnownException_MapsToExpectedJsonError(Type exceptionType, int expectedStatus, string expectedErrorType)
    {
        var middleware = new CustomExceptionHandlerMiddleware(
            _ => throw CreateException(exceptionType),
            Mock.Of<ILogger<CustomExceptionHandlerMiddleware>>());
        var context = HttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(expectedStatus, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);
        Assert.False(document.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("errorCode").GetInt32());
        Assert.Equal(expectedErrorType, document.RootElement.GetProperty("errorType").GetString());
    }

    [Fact]
    public async Task InvokeAsync_UnknownException_MapsToInternalServerError()
    {
        var middleware = new CustomExceptionHandlerMiddleware(
            _ => throw new InvalidOperationException("boom"),
            Mock.Of<ILogger<CustomExceptionHandlerMiddleware>>());
        var context = HttpContext();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal("Unexpected", document.RootElement.GetProperty("errorType").GetString());
    }

    private static DefaultHttpContext HttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static Exception CreateException(Type exceptionType)
    {
        if (exceptionType == typeof(BadRequestException))
            return new BadRequestException(["bad request"]);

        if (exceptionType == typeof(UnprocessableContentException))
            return new UnprocessableContentException(["unprocessable"]);

        return exceptionType == typeof(GeminiException)
            ? new GeminiException("external failed")
            : (Exception)Activator.CreateInstance(exceptionType, "test message")!;
    }
}

using System.Net;
using System.Text.Json;
using Shared.Exceptions;
using IdempotentAPI.Core;
using Microsoft.AspNetCore.Mvc;

namespace Web.Api.Middlewares
{
    public class CustomExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<CustomExceptionHandlerMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/problem+json";

            var problem = ex switch
            {
                // ── Infrastructure / External Exceptions ──────────────────
                UserAlreadyExistException => Problem(409, "Conflict", ex.Message, "USER_ALREADY_EXISTS"),
                NotFoundException => Problem(404, "Not Found", ex.Message, "NOT_FOUND"),
                RateLimitExceededException => Problem(429, "Too Many Requests", ex.Message, "RATE_LIMIT_EXCEEDED"),
                UnauthorizedException => Problem(401, "Unauthorized", ex.Message, "UNAUTHORIZED"),
                UnprocessableContentException => Problem(422, "Unprocessable Entity", ex.Message, "UNPROCESSABLE"),
                IdempotencyKeyDuplicateException => Problem(406, "Not Acceptable", ex.Message, "IDEMPOTENCY_DUPLICATE"),
                IdempotencyKeyMissingException => Problem(422, "Unprocessable Entity", ex.Message, "IDEMPOTENCY_MISSING"),
                BadRequestException => Problem(400, "Bad Request", ex.Message, "BAD_REQUEST"),
                GeminiException => Problem(422, "Unprocessable Entity", ex.Message, "GEMINI_ERROR"),

                // ── Fallback ──────────────────────────────────────────────
                _ => Problem(500, "Internal Server Error", "An unexpected error occurred.", "UNEXPECTED")
            };

            context.Response.StatusCode = problem.Status!.Value;
            await context.Response.WriteAsJsonAsync(problem);
        }

        private static ProblemDetails Problem(int status, string title, string detail, string code) => new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Extensions = { ["code"] = code }
        };
    }
}

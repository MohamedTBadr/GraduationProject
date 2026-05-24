using System.Net;
using System.Text.Json;
using Shared.Exceptions;
using IdempotentAPI.Core;
using Microsoft.AspNetCore.Mvc;
using Application;

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
                logger.LogError(ex, "Unhandled exception: {Message} ,For User: {User}", ex.Message, context.User?.Identity?.Name);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, errorResponse) = ex switch
            {
                // ── Infrastructure / External Exceptions ──────────────────
                UserAlreadyExistException => (409, ErrorResponse(ErrorType.AlreadyExists, 409, ex.Message)),
                NotFoundException => (404, ErrorResponse(ErrorType.NotFound, 404, ex.Message)),
                RateLimitExceededException => (429, ErrorResponse(ErrorType.LimitExceeded, 429, ex.Message)),
                UnauthorizedException => (401, ErrorResponse(ErrorType.Unauthorized, 401, ex.Message)),
                UnauthorizedAccessException => (403, ErrorResponse(ErrorType.Unauthorized, 403, ex.Message)),
                KeyNotFoundException => (404, ErrorResponse(ErrorType.NotFound, 404, ex.Message)),
                UnprocessableContentException => (422, ErrorResponse(ErrorType.BusinessRule, 422, ex.Message)),
                IdempotencyKeyDuplicateException => (406, ErrorResponse(ErrorType.Conflict, 406, ex.Message)),
                IdempotencyKeyMissingException => (422, ErrorResponse(ErrorType.Validation, 422, ex.Message)),
                BadRequestException => (400, ErrorResponse(ErrorType.Validation, 400, ex.Message)),
                GeminiException => (422, ErrorResponse(ErrorType.ExternalService, 422, ex.Message)),

                // ── Fallback ──────────────────────────────────────────────
                _ => (500, ErrorResponse(ErrorType.Unexpected, 500, ex.Message))
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(errorResponse);
        }

        private static object ErrorResponse(ErrorType type, int code, string description) => new
        {
            IsSuccess = false,
            ErrorCode = code,
            ErrorType = type.ToString(),
            ErrorDescription = description
        };
    }
}

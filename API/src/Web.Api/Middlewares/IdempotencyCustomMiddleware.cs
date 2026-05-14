using Shared.Exceptions;
using System.Net;
using System.Text.Json;

namespace Web.Api.Middlewares
{
    public class IdempotencyCustomMiddleware(RequestDelegate next, ILogger<IdempotencyCustomMiddleware> _logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            if (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method))
            {
                if (context.Request.Path.StartsWithSegments("/Hub") || context.Request.Path.StartsWithSegments("/api/notifications/stream"))
                {
                    await next(context);
                    return;
                }

                if (!context.Request.Headers.ContainsKey("IdempotencyKey"))
                {
                    throw new IdempotencyKeyMissingException("Missing Idempotency Key header.");
                }

                // Buffer the response so we can inspect it if it's an error
                var originalBodyStream = context.Response.Body;
                using var responseBody = new MemoryStream();
                context.Response.Body = responseBody;

                try
                {
                    await next(context);

                    // Check for idempotency errors (400, 409, or 406)
                    if (context.Response.StatusCode == StatusCodes.Status400BadRequest || 
                        context.Response.StatusCode == StatusCodes.Status409Conflict ||
                        context.Response.StatusCode == StatusCodes.Status406NotAcceptable)
                    {
                        responseBody.Seek(0, SeekOrigin.Begin);
                        var body = await new StreamReader(responseBody).ReadToEndAsync();

                        // Detect messages from IdempotentAPI library
                        if (body.Contains("The Idempotency header key value") || 
                            body.Contains("was used in a different request") ||
                            body.Contains("Duplicate Idempotency Key"))
                        {
                            // Clear and throw so CustomExceptionHandlerMiddleware can format it properly
                            context.Response.Clear();
                            throw new IdempotencyKeyDuplicateException(body);
                        }

                        // Not an idempotency error, copy back
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                    else
                    {
                        // Success or other status, copy back
                        responseBody.Seek(0, SeekOrigin.Begin);
                        await responseBody.CopyToAsync(originalBodyStream);
                    }
                }
                finally
                {
                    context.Response.Body = originalBodyStream;
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}

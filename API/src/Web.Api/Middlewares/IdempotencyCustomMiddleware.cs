using Shared.Exceptions;
using System.Net;
using System.Text.Json;

namespace Web.Api.Middlewares
{
    public class IdempotencyCustomMiddleware(RequestDelegate next, ILogger<IdempotencyCustomMiddleware> _logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
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
                    await next(context);
                    if (context.Response.StatusCode == StatusCodes.Status406NotAcceptable)
                    {
                        throw new IdempotencyKeyDuplicateException("Duplicate Idempotency Key Header");
                    }

                    

                }
                else
                {
                    await next(context);
                }
            }
            catch (IdempotencyException ex)
            {

                _logger.LogError(ex, "Something Wrong");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex switch
                {
                    IdempotencyKeyMissingException => (int)HttpStatusCode.BadRequest,
                    IdempotencyKeyDuplicateException => (int)HttpStatusCode.Conflict,
                    _ => (int)HttpStatusCode.InternalServerError
                };
                var response =new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    ErrorMessage = ex.Message
                };
               await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}

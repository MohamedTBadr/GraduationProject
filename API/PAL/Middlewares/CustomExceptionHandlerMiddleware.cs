
using System.Net;
using System.Text.Json;
using Common.Exceptions;
using IdempotentAPI.Core;
using Microsoft.AspNetCore.Http.HttpResults;
namespace PAL.Middlewares
{
    public class CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> _logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                    
                _logger.LogError(ex, "Something Wrong");

                await HandleExceptionAsync(context, ex);

            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            //set content type -> app/json
            context.Response.ContentType = "application/json";

            //set status code to 500 if internal,etc
            //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.StatusCode = ex switch
            {
                UserAlreadyExistException =>(int)HttpStatusCode.Conflict,
                NotFoundException => (int)HttpStatusCode.NotFound,
                RateLimitExceededException => (int)HttpStatusCode.TooManyRequests,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                UnprocessableContentException => (int)HttpStatusCode.UnprocessableEntity,
                IdempotencyKeyDuplicateException =>(int)HttpStatusCode.NotAcceptable,
                IdempotencyKeyMissingException => (int)HttpStatusCode.UnprocessableEntity,
                BadRequestException => (int)HttpStatusCode.BadRequest,
                GeminiException => (int)HttpStatusCode.UnprocessableEntity,
                _ => (int)HttpStatusCode.InternalServerError
            };

            //return standard response 
            var response = new ErrorDetails//C# object 
            {
                StatusCode = context.Response.StatusCode,
                ErrorMessage = ex.Message
            };
            var response2 = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(response2);

        }
    }
}


using System.Net;
using System.Text.Json;
using Common.Exceptions;
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
                NotFoundException => (int)HttpStatusCode.NotFound,
                RateLimitExceededException => (int)HttpStatusCode.TooManyRequests,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
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

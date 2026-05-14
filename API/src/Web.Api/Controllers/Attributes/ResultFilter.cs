// PAL/Filters/ResultFilter.cs
using Application;
using BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Web.Api.Controllers.Attributes
{
    public class ResultFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is not ObjectResult { Value: { } value })
                return;

            var type = value.GetType();
            if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Result<>))
                return;

            var isSuccess = (bool)type.GetProperty("IsSuccess")!.GetValue(value)!;

            if (isSuccess)
            {
                var successCode = context.ActionDescriptor.EndpointMetadata
                    .OfType<SuccessStatusCodeAttribute>()
                    .FirstOrDefault()?.StatusCode;

                if (successCode.HasValue)
                {
                    var innerValue = type.GetProperty("Value")!.GetValue(value);
                    context.Result = new ObjectResult(innerValue) { StatusCode = successCode.Value };
                }
                return;
            }

            var error = (Error?)type.GetProperty("Error")!.GetValue(value);
            var (statusCode, errorResponse) = MapError(error);
            context.Result = new ObjectResult(errorResponse) { StatusCode = statusCode };
        }

        private static (int StatusCode, object ErrorResponse) MapError(Error? error)
        {
            var statusCode = error?.Type switch
            {
                ErrorType.Validation => 422,
                ErrorType.NotFound => 404,
                ErrorType.Conflict => 409,
                ErrorType.Unauthorized => 401,
                ErrorType.Forbidden => 403,
                ErrorType.BusinessRule => 400,
                ErrorType.InvalidOperation => 400,
                ErrorType.LimitExceeded => 429,
                ErrorType.ExternalService => 502,
                ErrorType.Unavailable => 503,
                _ => 500
            };

            return (statusCode, new
            {
                IsSuccess = false,
                ErrorCode = error?.Code ?? 500,
                ErrorType = error?.Type.ToString() ?? "Unexpected",
                ErrorDescription = error?.Description ?? "An unexpected error occurred."
            });
        }
    }
}
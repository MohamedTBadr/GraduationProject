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

            var errorType = (ErrorType?)type.GetProperty("ErrorType")!.GetValue(value);
            var errorMessage = (string?)type.GetProperty("ErrorMessage")!.GetValue(value);
            var problem = MapError(errorType, errorMessage);
            context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
        }

        private static ProblemDetails MapError(ErrorType? errorType, string? message)
        {
            var statusCode = errorType switch
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

            return new ProblemDetails
            {
                Status = statusCode,
                Title = errorType?.ToString() ?? "Error",
                Detail = message
            };
        }
        private static ProblemDetails MapError(Error error)
        {
            var statusCode = error.Type switch
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

            return new ProblemDetails
            {
                Status = statusCode,
                Title = error.Type.ToString(),
                Detail = error.Description,
                Extensions = { ["code"] = error.Code }
            };
        }
    }
}
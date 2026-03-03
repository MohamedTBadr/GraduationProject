// PAL/Filters/ResultFilter.cs
using BLL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PAL.Controllers.Attributes
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
            if (isSuccess) return;

            var error = (Error)type.GetProperty("Error")!.GetValue(value)!;

            var problem = MapError(error);
            context.Result = new ObjectResult(problem) { StatusCode = problem.Status };
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
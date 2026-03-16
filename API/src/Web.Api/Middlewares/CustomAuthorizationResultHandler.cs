namespace Web.Api.Middlewares
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Policy;
    using Microsoft.AspNetCore.Mvc;

    public class CustomAuthorizationResultHandler
        : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Forbidden)
            {
                var problem = new ProblemDetails
                {
                    Status = 403,
                    Title = "Forbidden",
                    Detail = "You are not allowed to access this resource."
                };

                problem.Extensions["code"] = "FORBIDDEN";

                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problem);
                return;
            }

            if (authorizeResult.Challenged)
            {
                var problem = new ProblemDetails
                {
                    Status = 401,
                    Title = "Unauthorized",
                    Detail = "Authentication is required."
                };

                problem.Extensions["code"] = "UNAUTHORIZED";

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/problem+json";

                await context.Response.WriteAsJsonAsync(problem);
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}

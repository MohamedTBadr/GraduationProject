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
                var errorResponse = new
                {
                    IsSuccess = false,
                    ErrorCode = 403,
                    ErrorType = "Forbidden",
                    ErrorDescription = "You are not allowed to access this resource."
                };

                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(errorResponse);
                return;
            }

            if (authorizeResult.Challenged)
            {
                var errorResponse = new
                {
                    IsSuccess = false,
                    ErrorCode = 401,
                    ErrorType = "Unauthorized",
                    ErrorDescription = "Authentication is required."
                };

                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(errorResponse);
                return;
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}

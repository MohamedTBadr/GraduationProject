using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Hybrid;
using System.Security.Claims;

[AttributeUsage(AttributeTargets.Method)]
public class InvalidateCacheAttribute : ActionFilterAttribute
{
    public string[] Tags { get; }

    public InvalidateCacheAttribute(params string[] tags)
    {
        Tags = tags;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        // Don't invalidate on exceptions
        if (executedContext.Exception != null) return;

        var statusCode = executedContext.Result switch
        {
            ObjectResult obj => obj.StatusCode,
            StatusCodeResult sc => sc.StatusCode,
            _ => null
        };

        // Standardize: any successful response (200-299) should trigger cache invalidation
        if (statusCode is >= 200 and < 300)
        {
            var cache = executedContext.HttpContext.RequestServices.GetRequiredService<HybridCache>();

            var user = executedContext.HttpContext.User;
            var userId = user?.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = user?.FindFirstValue(ClaimTypes.Role) ?? user?.FindAll(ClaimTypes.Role).Select(c => c.Value).FirstOrDefault();

            var resolvedTags = Tags.Select(tag =>
            {
                var processedTag = tag;

                // 1. Resolve Route values
                foreach (var rv in executedContext.RouteData.Values)
                {
                    processedTag = processedTag.Replace($"{{{rv.Key}}}", rv.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
                }

                // 2. Resolve Claim-based placeholders
                if (userId != null)
                {
                    processedTag = processedTag.Replace("{UserId}", userId, StringComparison.OrdinalIgnoreCase);
                }
                if (userRole != null)
                {
                    processedTag = processedTag.Replace("{UserRole}", userRole, StringComparison.OrdinalIgnoreCase);
                }

                return processedTag;
            }).ToList();

            foreach (var tag in resolvedTags)
                await cache.RemoveByTagAsync(tag);
        }
    }
}
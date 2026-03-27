using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Hybrid;

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

        if (statusCode is 201 or 204)
        {
            var cache = executedContext.HttpContext.RequestServices.GetRequiredService<HybridCache>();

            var resolvedTags = Tags.Select(tag =>
                executedContext.RouteData.Values.Aggregate(tag, (current, rv) =>
                    current.Replace($"{{{rv.Key}}}", rv.Value?.ToString()))
            ).ToList();

            foreach (var tag in resolvedTags)
                await cache.RemoveByTagAsync(tag);
        }
    }
}
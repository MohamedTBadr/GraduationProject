using Azure.Core;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Hybrid;
using System.Text;
using System.Text.Json;

namespace PAL.Controllers.Attributes
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.Extensions.Caching.Hybrid;
    using System.Text.Json;

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RedisCacheAttribute : ActionFilterAttribute
    {
        public int DurationInSeconds { get; }

        public RedisCacheAttribute(int durationInSeconds)
        {
            DurationInSeconds = durationInSeconds;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Only GET requests are cached
            if (!HttpMethods.IsGet(context.HttpContext.Request.Method))
            {
                await next();
                return;
            }

            var hybridCache = context.HttpContext.RequestServices.GetRequiredService<HybridCache>();

            // Build a unique cache key (path + query + optional user)
            var key = BuildCacheKey(context);

            // Use GetOrCreateAsync to handle both L1 and L2 caches
            var cachedValue = await hybridCache.GetOrCreateAsync(
                key,
                async ct =>
                {
                    // Execute the controller action
                    var executedContext = await next();

                    if (executedContext.Result is ObjectResult obj && obj.StatusCode == 200)
                    {
                        // Serialize object to JSON string for caching
                        return JsonSerializer.Serialize(obj.Value);
                    }

                    // Nothing to cache
                    return null;
                },
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(DurationInSeconds),       // L2: Redis
                    LocalCacheExpiration = TimeSpan.FromSeconds(DurationInSeconds / 2) // L1: Memory
                }
            );

            // If we got cached data, short-circuit the request
            if (cachedValue != null)
            {
                context.Result = new ContentResult
                {
                    Content = cachedValue,
                    ContentType = "application/json",
                    StatusCode = 200
                };
            }
        }

        private string BuildCacheKey(ActionExecutingContext context)
        {
            var routeValues = context.ActionDescriptor.RouteValues;
            var controller = routeValues["controller"] ?? "unknownController";
            var action = routeValues["action"] ?? "unknownAction";

            var key = $"{controller}:{action}";

            // Include query parameters in key
            foreach (var q in context.HttpContext.Request.Query.OrderBy(x => x.Key))
            {
                key += $"|{q.Key}:{q.Value}";
            }


            //// Optional: include user identity to make cache per-user
            //var user = context.HttpContext.User?.Identity?.Name ?? "anonymous";
            //key += $"|user:{user}";

            return key;
        }
    }

}


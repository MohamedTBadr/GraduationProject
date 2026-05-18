using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Hybrid;
using System.Security.Claims;
using System.Text.Json;

namespace Web.Api.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class HybridCacheAttribute : ActionFilterAttribute
    {
        public int DurationInSeconds { get; }
        public string[] Tags { get; }
        public bool CachePostRequest { get; set; } = false;

        public HybridCacheAttribute(int durationInSeconds, params string[] tags)
        {
            DurationInSeconds = durationInSeconds;
            Tags = tags;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {


            // 1. Only GET and explicitly opted-in POST requests should be cached
            bool isGet = HttpMethods.IsGet(context.HttpContext.Request.Method);
            bool isPost = HttpMethods.IsPost(context.HttpContext.Request.Method);

            if (!isGet && !(isPost && CachePostRequest))
            {
                await next();
                return;
            }

            var hybridCache = context.HttpContext.RequestServices.GetRequiredService<HybridCache>();

            // 2. Build a unique cache key (Path + Query + UserID)
            var key = BuildCacheKey(context);

            // 3. Resolve dynamic tags (e.g., replaces "{id}" with the actual GUID from the URL)
            var resolvedTags = ResolveTags(context);

            // 4. Execute GetOrCreateAsync
            var cachedValue = await hybridCache.GetOrCreateAsync(
                key,
                async ct =>
                {
                    // This runs only on a cache miss
                    var executedContext = await next();

                    if (executedContext.Result is ObjectResult obj && (obj.StatusCode == 200 || obj.StatusCode == null))
                    {
                        // Store the actual object (HybridCache handles serialization to Redis)
                        return obj.Value;
                    }

                    return null;
                },
                new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromSeconds(DurationInSeconds),
                    LocalCacheExpiration = TimeSpan.FromSeconds(DurationInSeconds / 2)
                },
                tags: resolvedTags
            );

            // 5. Short-circuit the request if we have a result
            if (cachedValue != null && context.Result == null)
            {
                context.Result = new OkObjectResult(cachedValue);
            }
        }

        private string[] ResolveTags(ActionExecutingContext context)
        {
            if (Tags == null || Tags.Length == 0) return Array.Empty<string>();

            var resolved = new List<string>();
            var userId = context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            foreach (var tag in Tags)
            {
                var processedTag = tag;
                // Replace placeholders like {id} with actual route values
                foreach (var routeValue in context.RouteData.Values)
                {
                    var placeholder = $"{{{routeValue.Key}}}";
                    if (processedTag.Contains(placeholder, StringComparison.OrdinalIgnoreCase))
                    {
                        processedTag = processedTag.Replace(placeholder, routeValue.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
                    }
                }

                // Replace {UserId} placeholder
                if (userId != null && processedTag.Contains("{UserId}", StringComparison.OrdinalIgnoreCase))
                {
                    processedTag = processedTag.Replace("{UserId}", userId, StringComparison.OrdinalIgnoreCase);
                }

                resolved.Add(processedTag);
            }
            return resolved.ToArray();
        }

        private string BuildCacheKey(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            // Crucial: Use NameIdentifier (Unique ID) because your Usernames are not unique
            var userId = context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

            // Sort query parameters so ?a=1&b=2 is the same as ?b=2&a=1
            var query = request.Query
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}")
                .ToList();

            var bodyString = string.Empty;
            if (HttpMethods.IsPost(request.Method) && context.ActionArguments.Any())
            {
                bodyString = ":" + JsonSerializer.Serialize(context.ActionArguments);
            }

            return $"hcache:{userId}:{request.Path}:{string.Join("&", query)}{bodyString}";
        }
    }
}
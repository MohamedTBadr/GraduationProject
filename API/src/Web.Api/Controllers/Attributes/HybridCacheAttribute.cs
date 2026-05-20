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

        /// <summary>
        /// Allow POST requests to be cached. Default: false.
        /// </summary>
        public bool CachePostRequest { get; set; } = false;

        /// <summary>
        /// Cache a separate entry per individual user ID.
        /// Use for personal data (cart, profile, orders).
        /// Default: false.
        /// </summary>
        public bool PerUser { get; set; } = false;

        /// <summary>
        /// Cache a separate entry per role (or role combination).
        /// Use when different roles receive different responses for the same endpoint (e.g. admin vs user).
        /// Default: false.
        /// </summary>
        public bool PerRole { get; set; } = false;

        /// <summary>
        /// The claim type used to resolve the user's role(s).
        /// Override if your app uses a custom role claim. Default: ClaimTypes.Role.
        /// </summary>
        public string RoleClaim { get; set; } = ClaimTypes.Role;

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

            // 2. Build a unique cache key based on caching mode (shared / per-role / per-user)
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

            // 5. Short-circuit the request if we have a cached result
            if (cachedValue != null && context.Result == null)
            {
                context.Result = new OkObjectResult(cachedValue);
            }
        }

        /// <summary>
        /// Resolves the cache key segment based on the caching mode:
        /// - PerUser  → unique user ID         (e.g. "abc-123")
        /// - PerRole  → sorted role(s)          (e.g. "admin" or "admin|manager")
        /// - Neither  → shared across all users (e.g. "shared")
        /// </summary>
        private string ResolveSegment(ClaimsPrincipal? user)
        {
            if (PerUser)
                return user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

            if (PerRole)
            {
                // Collect ALL roles, sort for consistency so "admin|manager" == "manager|admin"
                var roles = user?.FindAll(RoleClaim)
                    .Select(c => c.Value.ToLowerInvariant())
                    .OrderBy(r => r)
                    .ToList() ?? [];

                return roles.Count > 0 ? string.Join("|", roles) : "anonymous";
            }

            // Default: one shared cache entry for everyone
            return "shared";
        }

        private string BuildCacheKey(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            // Sort query parameters so ?a=1&b=2 and ?b=2&a=1 produce the same key
            var query = request.Query
                .OrderBy(x => x.Key)
                .Select(x => $"{x.Key}={x.Value}")
                .ToList();

            var bodyString = string.Empty;
            if (HttpMethods.IsPost(request.Method) && context.ActionArguments.Any())
                bodyString = ":" + JsonSerializer.Serialize(context.ActionArguments);

            var segment = ResolveSegment(context.HttpContext.User);

            return $"hcache:{segment}:{request.Path}:{string.Join("&", query)}{bodyString}";
        }

        private string[] ResolveTags(ActionExecutingContext context)
        {
            if (Tags == null || Tags.Length == 0) return Array.Empty<string>();

            var resolved = new List<string>();

            // Only resolve userId when it's actually needed (PerUser mode or {UserId} tag placeholder)
            var userId = PerUser
                ? context.HttpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                : null;

            foreach (var tag in Tags)
            {
                var processedTag = tag;

                // Replace route placeholders like {id}, {productId}, etc.
                foreach (var routeValue in context.RouteData.Values)
                {
                    var placeholder = $"{{{routeValue.Key}}}";
                    if (processedTag.Contains(placeholder, StringComparison.OrdinalIgnoreCase))
                    {
                        processedTag = processedTag.Replace(
                            placeholder,
                            routeValue.Value?.ToString(),
                            StringComparison.OrdinalIgnoreCase);
                    }
                }

                // Replace {UserId} placeholder (only meaningful in PerUser mode)
                if (userId != null && processedTag.Contains("{UserId}", StringComparison.OrdinalIgnoreCase))
                {
                    processedTag = processedTag.Replace("{UserId}", userId, StringComparison.OrdinalIgnoreCase);
                }

                resolved.Add(processedTag);
            }

            return resolved.ToArray();
        }
    }
}
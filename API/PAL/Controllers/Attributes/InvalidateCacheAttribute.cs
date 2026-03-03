using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Hybrid;

namespace PAL.Controllers.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InvalidateCacheAttribute:ActionFilterAttribute
    {
        private readonly string[] _keysToInvalidate;

        public InvalidateCacheAttribute(params string[] keysToInvalidate)
        {
            _keysToInvalidate = keysToInvalidate;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var hybridCache = context.HttpContext.RequestServices.GetRequiredService<HybridCache>();

            var executedContext = await next();

            // Only invalidate on success
            if (executedContext.Result is ObjectResult obj && obj.StatusCode >= 200 && obj.StatusCode < 300)
            {
                foreach (var key in _keysToInvalidate)
                {
                    await hybridCache.RemoveAsync(key);
                }
            }
        }
    }
}

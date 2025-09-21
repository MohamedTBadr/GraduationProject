using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using System.Text.Json;

namespace PAL.Controllers.Attributes
{
    public class RedisCacheAttribute(int DurationInSeconds):ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var cacheService = context.HttpContext.RequestServices.GetRequiredService<ICacheService>();
            //Next->set Valuer->goto end point
            string cacheKey = GenerateKey(context.HttpContext.Request);
            var result = await cacheService.GetCachedValueAsync(cacheKey);
            if (result != null)
            {
                context.Result = new ContentResult
                {
                    Content = result,
                    ContentType = "application/json",
                    StatusCode = 200,
                };
                return;
            }
            var resultContext = await next.Invoke();
            if (resultContext.Result is OkObjectResult objectResult)
            {
                var serialized = JsonSerializer.Serialize(objectResult.Value);
                await cacheService.SetCacheValueAsync(cacheKey, serialized, TimeSpan.FromSeconds(DurationInSeconds));
            }


        }

        private string GenerateKey(HttpRequest request)
        {
            var key = new StringBuilder();
            //request path
            //request query
            key.Append(request.Path);
            foreach (var item in request.Query.OrderBy(x => x.Key))
            {
                key.Append($"{item.Key}-{item.Value}");

            }
            return key.ToString();
        }
    }
}


using Common.Exceptions;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Core;
using IdempotentAPI.Extensions.DependencyInjection;
using System.Threading.RateLimiting;

namespace PAL
{
    public static class PresentationRegistrationService
    {

        public async static Task<IServiceCollection>AddPresentationRegistrationServices(IServiceCollection services ,IConfiguration configuration)
        {
          
            #region RateLimiter
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 20,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.OnRejected = async (context, token) =>
                {
                    throw new RateLimitExceededException();
                };
            });
            #endregion


            #region Idompotent API
            // 1) Register an IDistributedCache implementation first
            //    (in dev: in-memory; in prod: use StackExchange.Redis)
            // 1) Register Redis as IDistributedCache
            services.AddStackExchangeRedisCache(options =>
            {
                // Your Redis connection string should be in appsettings.json
                // e.g. "Redis": "localhost:6379"
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "MyApp_"; // optional prefix for Redis keys
            });

            // 2) Create Idempotency options and register the core with them
            var idempotencyOptions = new IdempotencyOptions
            {
                // prefer ExpiresInMilliseconds (ExpireHours is marked obsolete in README)
                ExpiresInMilliseconds = TimeSpan.FromSeconds(240).TotalMilliseconds,
                HeaderKeyName = "IdempotencyKey",
                DistributedCacheKeysPrefix = "IdempAPI_",
                CacheOnlySuccessResponses = true,
                DistributedLockTimeoutMilli = 2000 // ms (only required if using distributed locks)


            };

            // Register core idempotency using the options (controller-based)
            services.AddIdempotentAPI(idempotencyOptions);

            // 3) Register the library's DistributedCache implementation
            //    (this extension lives in the IdempotentAPI.Cache.DistributedCache package)
            services.AddIdempotentAPIUsingDistributedCache(); // README shows this usage. :contentReference[oaicite:2]{index=2}
            #endregion

            return services;
        }
    }
}

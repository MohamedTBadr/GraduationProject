using Application.Hubs;
using Application.Interfaces;
using Application.Services;
using Application.Services.Helpers;
using Domain.Entities;
using Google.GenAI;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Core;
using IdempotentAPI.Extensions.DependencyInjection;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.IdentityModel.Tokens;
using Shared.Exceptions;
//using PAL.Notifications;
using System.IO.Compression;
using System.Text;
using System.Threading.RateLimiting;
using Web.Api.Controllers.Attributes;
using Web.Api.Services;
//using PAL.Notifications;

namespace Web.Api
{
    public static class WebRegistrationService
    {

        public async static Task<IServiceCollection> AddWebsRegistrationServices(IServiceCollection services, IConfiguration configuration)
        {
            //services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();

            services.AddControllers(options =>
            {
                options.Filters.Add<ResultFilter>(); // ← applies to all controllers
            });
            #region RateLimiter
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unkown",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 20,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.OnRejected = async (_, _) =>
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


            #region SignalR

            services.AddSignalR();


            #endregion

            #region Identity
            ////Configure Identity with your ApplicationUser (use Guid keys)
            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();



            // Configure Identity options (AFTER AddIdentity)
            services.Configure<IdentityOptions>(options =>
            {
                // ✅ Allow letters, digits, and spaces
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
                // Optional: require unique email
                options.User.RequireUniqueEmail = true;

                // Optional: tweak password settings
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            });



            #endregion


            #region GeminiAI
            //Gemini Configuration
            services.AddSingleton(provider =>
            {
                // Retrieve the API Key securely from configuration, environment variables, etc.
                // For this example, we'll try an environment variable first.

                var apiKey = configuration["Gemini:ApiKey"];



                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Gemini API Key is not configured.");
                }

                return new Client(apiKey: apiKey);
            });
            services.AddScoped<GeminiService>();
            #endregion



            #region Hybrid Cache



            services.AddHybridCache(options =>
            {
                options.MaximumKeyLength = 512;
                options.MaximumPayloadBytes = 1024 * 1024 * 10;

                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(30),
                    LocalCacheExpiration = TimeSpan.FromMinutes(30)
                };
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "HybridCache_";
            });
            #endregion


            #region Enhancment 

            // Enable compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true; // compress HTTPS responses
                options.Providers.Add<GzipCompressionProvider>();
                // options.Providers.Add<BrotliCompressionProvider>(); // optional, more efficient
            });

            // Configure compression levels
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest; // or Optimal
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();

            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Graduation Project V1", Version = "v1" });

            //    // Define the security scheme
            //    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //    {
            //        Name = "Authorization",
            //        Type = SecuritySchemeType.Http,
            //        Scheme = "Bearer",
            //        BearerFormat = "JWT",
            //        In = ParameterLocation.Header,
            //        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer abc123\""
            //    });

            //    // Apply the security globally to all endpoints
            //    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //          {
            //            new OpenApiSecurityScheme
            //            {
            //               Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            //             },
            //             new string[] {}
            //           }
            //    });
            //});


            #endregion
            services.AddScoped<IChatNotificationService, ChatNotificationService>();
            services.AddScoped<IChatService, ChatService>();
            return services;
        }
    }
}
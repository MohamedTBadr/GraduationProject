using Application.DTOs.PaymobDTOs;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Serilog;
using Shared.Exceptions;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using Web.Api.Controllers.Attributes;
using Web.Api.Middlewares;
using Web.Api.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Hangfire;
using Hangfire.SqlServer;

namespace Web.Api
{
    public static class WebRegistrationService
    {

        public async static Task<IServiceCollection> AddWebsRegistrationServices(IServiceCollection Services, IConfiguration configuration)
        {
            //Services.AddScoped<INotificationPublisher, SignalRNotificationPublisher>();

            Services.AddControllers(options =>
            {
                options.Filters.Add<ResultFilter>(); // ← applies to all controllers
            });
            #region RateLimiter
            Services.AddRateLimiter(options =>
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
            Services.AddStackExchangeRedisCache(options =>
            {
                // Your Redis connection string should be in appsettings.json
                // e.g. "Redis": "localhost:6379"
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "MyApp_"; // optional prefix for Redis keys
            });

            // 2) Create Idempotency options and register the core with them
            var idempotencyOptions = new IdempotencyOptions
            {
                
                SerializerSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    // Handles objects with no parameterless constructor (like Result<T>)
                    ConstructorHandling = Newtonsoft.Json.ConstructorHandling.AllowNonPublicDefaultConstructor,

                    // Handles circular references
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,

                    // Ignore null values to reduce cache size
                    NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,

                    // Preserves type info for polymorphic types
                    TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,

                    // Handles missing members gracefully
                    MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,

                    // Makes deserialization use private setters
                    ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                    {
                        NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy()
                    }
                },

                // prefer ExpiresInMilliseconds (ExpireHours is marked obsolete in README)
                ExpiresInMilliseconds = TimeSpan.FromSeconds(500).TotalMilliseconds,
                HeaderKeyName = "IdempotencyKey",
                DistributedCacheKeysPrefix = "IdempAPI_",
                CacheOnlySuccessResponses = true,
                DistributedLockTimeoutMilli = 500 // ms (only required if using distributed locks)
                

            };

            // Register core idempotency using the options (controller-based)
            Services.AddIdempotentAPI(idempotencyOptions);

            // 3) Register the library's DistributedCache implementation
            //    (this extension lives in the IdempotentAPI.Cache.DistributedCache package)
            Services.AddIdempotentAPIUsingDistributedCache(); // README shows this usage. :contentReference[oaicite:2]{index=2}
            #endregion


            #region SignalR

            Services.AddSignalR();


            #endregion

            #region Identity
            ////Configure Identity with your ApplicationUser (use Guid keys)
            Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();



            // Configure Identity options (AFTER AddIdentity)
            Services.Configure<IdentityOptions>(options =>
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


            Services.AddAuthorization(options =>
            {
                options.AddPolicy("DashboardAccess", policy =>
                    policy.RequireRole("Admin", "Vendor"));
            });
            #endregion


            #region GeminiAI
            //Gemini Configuration
            Services.AddSingleton(provider =>
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
            Services.AddScoped<LlamaService>();
            #endregion



            #region Hybrid Cache



            Services.AddHybridCache(options =>
            {
                options.MaximumKeyLength = 512;
                options.MaximumPayloadBytes = 1024 * 1024 * 10;

                options.DefaultEntryOptions = new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(30),
                    LocalCacheExpiration = TimeSpan.FromMinutes(30)
                };
            });

            Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "HybridCache_";
            });
            #endregion


            #region Enhancment 

            // Enable compression
            Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true; // compress HTTPS responses
                options.Providers.Add<GzipCompressionProvider>();
                // options.Providers.Add<BrotliCompressionProvider>(); // optional, more efficient
            });

            // Configure compression levels
            Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest; // or Optimal
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            Services.AddEndpointsApiExplorer();

            //Services.AddSwaggerGen(c =>
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
            Services.AddScoped<IChatNotificationService, ChatNotificationService>();
            Services.AddScoped<IChatService, ChatService>();
            Services.AddSingleton<
    IAuthorizationMiddlewareResultHandler,
    CustomAuthorizationResultHandler>();
         


            #region Resilience 
            #region Db Resiliency
            Services.AddResiliencePipeline("db-pipeline", builder =>
            {
                builder
                    .AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        Delay = TimeSpan.FromMilliseconds(500),
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true
                    })
                    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.5,
                        MinimumThroughput = 5,
                        BreakDuration = TimeSpan.FromSeconds(10)
                    })
                    .AddTimeout(TimeSpan.FromSeconds(3));
            });
            #endregion

            #region External API Resiliency
           Services.AddResiliencePipeline("storage-pipeline", builder =>
            {
                builder
                    // 1. Total Timeout: The whole operation shouldn't exceed 30s
                    .AddTimeout(TimeSpan.FromSeconds(30))

                    // 2. Retry: Only 2 attempts (AWS SDK does some retries internally)
                    .AddRetry(new RetryStrategyOptions
                    {
                        MaxRetryAttempts = 2,
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        Delay = TimeSpan.FromSeconds(1)
                    })

                    // 3. Circuit Breaker: If S3 is unreachable, stop trying for 30s
                    .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                    {
                        FailureRatio = 0.2, // Break if 20% of calls fail
                        MinimumThroughput = 10,
                        BreakDuration = TimeSpan.FromSeconds(30)
                    });
            });


            // Registering the Paymob Client with a built-in Resilience Pipeline
            Services.AddHttpClient("PaymobClient", (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<PaymobOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddStandardResilienceHandler(options =>
            {
                // Payment-specific tweaks
                options.Retry.MaxRetryAttempts = 2; // Be conservative with payments
                options.Retry.Delay = TimeSpan.FromSeconds(2);
                options.Retry.BackoffType = DelayBackoffType.Exponential;

                // Total timeout for the combined operations
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
            });
            #endregion






            #endregion




            Services.AddHttpContextAccessor();

            Services.AddOpenTelemetry()
                .WithTracing(t => t
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyApi"))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("MyApi")
                .AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"))
                    )
                .WithMetrics(m => m
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyApi"))
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
    .AddMeter("MyApi")
        .AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"))
            );


            #region Hangfire
            Services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));

            Services.AddHangfireServer();
            #endregion



            return Services;
        }
    }
}
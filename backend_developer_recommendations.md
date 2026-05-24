# 🛠️ Critical Backend Recommendations for Eventora
---

## 1. ⚠️ Prevent Server Crash on Startup (Gemini API Configuration)

### The Issue:
In `WebRegistrationService.cs` (under `Web.Api/WebRegistrationService.cs`), the Google Gemini AI `Client` is registered as a singleton. If the `"Gemini:ApiKey"` is not configured in `appsettings.json`, it throws a strict `InvalidOperationException`:
```csharp
if (string.IsNullOrEmpty(apiKey))
{
    throw new InvalidOperationException("Gemini API Key is not configured.");
}
```
Because the active AI event planning features utilize **Groq Llama-3** via `LlamaService` rather than Gemini, this strict throwing causes the entire Web API container to **crash on startup** for any developer running without a local Gemini key.

### Recommendation:
Make the Gemini configuration optional so local development environments can boot without it. Wrap the registration in a safe check, logging a warning rather than throwing, or return null/a dummy client:
```csharp
Services.AddSingleton(provider =>
{
    var apiKey = configuration["Gemini:ApiKey"];
    if (string.IsNullOrEmpty(apiKey))
    {
        var logger = provider.GetRequiredService<ILogger<WebRegistrationService>>();
        logger.LogWarning("Gemini API Key is not configured. Google GenAI features will be unavailable.");
        return null!; // Return null rather than crashing
    }
    return new Client(apiKey: apiKey);
});
```

---

## 2. ⚡ Optimize Cold-Start AI Candidate Filtering (`AIFilterAsync`)

### The Issue:
In `ServiceRepository.cs` (under `Infrastructure/Repositories/ServiceRepository.cs`), the fallback/cold-start AI filtering query fetches all services in the database that are cheaper than the budget:
```csharp
public async Task<List<Service>> AIFilterAsync(AIRequest AIRequest, CancellationToken cancellationToken)
{
    return 
        await _context.Services
            .Where(p => p.Price < AIRequest.Budget && p.Price > 0)
            .Include(p => p.Vendor).Include(p => p.ServiceType).Include(p => p.ServiceImages)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}
```
As the database grows, this raw `Where` filter without taxonomy classification will pull **thousands of services** into memory, creating massive CPU and RAM bottlenecks during AI package generation.

### Recommendation:
Leverage the Lucene Search Index (`SearchServicesAsync`) as the primary path as is currently designed in the Application layer, and optimize this database-level fallback to filter by `EventTypeId` or category keywords directly in SQL to restrict the candidate count to a maximum of 50-100 high-quality options:
```csharp
public async Task<List<Service>> AIFilterAsync(AIRequest AIRequest, CancellationToken cancellationToken)
{
    return await _context.Services
        .Where(p => p.Price < AIRequest.Budget && p.Price > 0)
        .Where(p => p.IsHidden == false) // Filter hidden items
        .OrderByDescending(p => p.Rating) // Prioritize highest rated
        .Take(50) // Cap the database transfer
        .Include(p => p.Vendor)
        .Include(p => p.ServiceType)
        .Include(p => p.ServiceImages)
        .AsNoTracking()
        .ToListAsync(cancellationToken);
}
```

---

## 3. 🎯 Standardize JSON Serialization in Recommendations API

### The Issue:
In `IPlanningAIService.cs` (under `Application/Interfaces/IPlanningAIService.cs`), `RecommendationItem` has exact attributes specified via `[JsonPropertyName]`:
```csharp
public class RecommendationItem
{
    [JsonPropertyName("ServiceId")]
    public Guid ServiceId { get; set; }

    [JsonPropertyName("ServiceName")]
    public string ServiceName { get; set; }

    [JsonPropertyName("VendorName")]
    public string VendorName { get; set; }

    [JsonPropertyName("Reasoning")]
    public string Reasoning { get; set; }
}
```
Because these are serialized with exact uppercase PascalCase prefixes (unlike standard CamelCase used elsewhere), the JSON output has keys like `"ServiceId"` and `"ServiceName"`.

### Recommendation:
Ensure this behavior is intentional. On the frontend, we have implemented support for these exact formats so they display beautifully, but keeping them camelCase matching the rest of the API is usually best practice.

---

## 4. 🔑 Fix 401 Unauthorized on SignalR (ChatHub) & SSE (Notifications Stream)

### The Issue:
In local development, the frontend gets `401 Unauthorized` errors when attempting to open a WebSocket connection to `ws://localhost:5000/Hub/chatHub` or an EventSource stream to `http://localhost:5000/api/notifications/stream`.

This is caused by two factors:
1. **Strict Path Checks**: The custom `OnMessageReceived` logic in `ApplicationRegistrationService.cs` checks the request path using `StartsWithSegments("/Hub/chatHub")` and `StartsWithSegments("/api/notifications/stream")`. In some configurations (such as lowercase paths or negotiation fallbacks), these strict segment checks fail, preventing the query string `access_token` from being extracted.
2. **Local HTTP Development Blocking**: The JWT Bearer middleware defaults to `RequireHttpsMetadata = true`. If you run/test the backend over HTTP (`http://localhost:5000`), the authentication handler blocks the requests.

### Recommendation:
Update the `ConfigureJWT` method in `ApplicationRegistrationService.cs` to allow HTTP connections locally, use a case-insensitive robust path substring match, and optionally add Console log listeners so you can see validation success/errors directly in the console:

```csharp
public static void ConfigureJWT(this IServiceCollection Services, IConfiguration configuration)
{
    var jwt = configuration.GetSection("JWTOptions").Get<JWTOptions>();

    Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JWTOptions:Issuer"],
            ValidAudience = configuration["JWTOptions:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["JWTOptions:SecretKey"]!))
        };

        options.RequireHttpsMetadata = false; // 👈 CRITICAL: Allow HTTP in local development

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // 👈 Robust case-insensitive check for query token extraction
                if (!string.IsNullOrEmpty(accessToken) &&
                    ((path.Value?.Contains("/chatHub", StringComparison.OrdinalIgnoreCase) ?? false) || 
                     (path.Value?.Contains("/notifications/stream", StringComparison.OrdinalIgnoreCase) ?? false)))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[JwtBearer] Auth failed for {context.Request.Path}: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"[JwtBearer] Auth succeeded for {context.Request.Path}");
                return Task.CompletedTask;
            }
        };
    });
}
```

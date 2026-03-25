

using Application;
using Application.Services.Helpers;
using Domain.Contracts;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Hybrid;
using Serilog;
using System.Reflection;
using Web.Api;

using Web.Api.Middlewares;


namespace Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add Services to the container.

            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });


           await InfrastructureRegistrationService.AddInfrastructureServices(builder.Services, builder.Configuration);
            await WebRegistrationService.AddWebsRegistrationServices(builder.Services, builder.Configuration);
            await ApplicationRegistrationService.AddApplicationServices(builder.Services, builder.Configuration);
     

           
            builder.Host.UseSerilog((ctx, config) => config
                    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));
            //builder.Services.AddOpenApi();
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });

            builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Fastest;
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy
                        .WithOrigins("http://localhost:4200") // your frontend
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // ← must for SignalR
                           
                });
            });



            var app = builder.Build();
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                //app.MapOpenApi();          // serves /openapi/v1.json
                //app.MapScalarApiReference(); // serves modern UI at /scalar/v1
                //app.UseSwagger();
                //app.UseSwaggerUI(options =>
                //{
                //    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Graduation Project V1");
                //    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                //    options.DocumentTitle = "Graduation Project V1";
                //    options.EnablePersistAuthorization();
                //    options.DisplayRequestDuration();
                //    //options.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
                //    //options.InjectStylesheet("/swagger-ui/style.css");

                //});
            }

            await SeedingScope(app);


            //app.Logger.LogInformation("Application Starting Up");
            app.MapPost("/AI-Chat", async ([FromServices] GeminiService geminiService) =>
            {
                var budget = 3000;
                var Services = new[]
                {
        new { Name = "Service A", Price = 1000 },
        new { Name = "Service B", Price = 1500 },
        new { Name = "Service C", Price = 800 },
        new { Name = "Service D", Price = 1200 },
        new { Name = "Service E", Price = 500 }
        };

                // Step 1: Compute all valid combinations locally
                var affordableServices = Services.Where(p => p.Price <= budget).ToArray();
                var combinations = new List<object>();

                int n = affordableServices.Length;
                for (int i = 1; i < (1 << n); i++)
                {
                    var combo = new List<string>();
                    int total = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if ((i & (1 << j)) != 0)
                        {
                            combo.Add(affordableServices[j].Name);
                            total += affordableServices[j].Price;
                        }
                    }

                    if (total <= budget)
                        combinations.Add(new
                        {
                            Services = combo,
                            total_price = total,
                            remaining_budget = budget - total
                        });
                }

                combinations = combinations.OrderBy(c => (int)c.GetType().GetProperty("total_price").GetValue(c)).ToList();

                // Step 2: Prepare top N combinations for AI enrichment
                var topCombos = combinations.Take(5).Select(c => ((dynamic)c).Services).ToList();
                var prompt = $@"
You are an API that returns ONLY valid JSON. Do not include markdown or extra text.
Given the top Service combinations under a budget of {budget} and their names:

{string.Join("\n", topCombos.Select(c => string.Join(" + ", c)))}

Return JSON with the following schema for each combination:
{{
  ""Services"": [string],
  ""total_price"": number,
  ""remaining_budget"": number,
  ""rank"": number,         // rank from best to worst
  ""description"": string,   // short recommendation
  ""image_url"": string      // optional image for Service combination
}}

Return an array named ""enriched_recommendations"". Only JSON.
";

                // Step 3: Call AI
                var aiResponse = await geminiService.SendMessageAsync(prompt);

                // Step 4: Merge AI response with full deterministic combinations
                var jsonResponse = new
                {
                    budget,
                    Services_available = affordableServices.ToDictionary(p => p.Name, p => p.Price),
                    recommendations = combinations, // full list
                    enriched_recommendations = aiResponse // AI-enhanced top combos
                };

                return Results.Json(jsonResponse);
            });



            app.UseCors("CorsPolicy");                          // ← First
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseMiddleware<IdempotencyCustomMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHub<ChatHub>("/Hub/chatHub");
            //app.Run($"https://localhost:{builder.Configuration["PORT"]}");
            await app.RunAsync();
          
        }

        private static async Task SeedingScope(WebApplication app)
        {
            // ✅ Run DB seeders at startup
            using (var scope = app.Services.CreateScope())
            {
                var Services = scope.ServiceProvider;
                try
                {
                    var initializer = Services.GetRequiredService<IDbIntialize>();
                    await initializer.IntializeAsync(); // seeds Vendors, Roles, Categories
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Seeding failed: {ex.Message}");
                }
            }
        }
    }
}
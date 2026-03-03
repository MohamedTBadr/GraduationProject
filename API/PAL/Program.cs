
using BLL;
using BLL.Services.Helpers;
using DAL;
using DAL.Context;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Hybrid;
using PAL.Hubs;
using PAL.Middlewares;

namespace PAL
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                });


            await DataLayerRegistrationService.AddDataLayerRegistrationService(builder.Services, builder.Configuration);
            await BusiniessLayerRegistrationService.AddBusinessLayerServices(builder.Services, builder.Configuration);
            await PresentationRegistrationService.AddPresentationRegistrationServices(builder.Services, builder.Configuration);

//            builder.Services
//.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
//.AddEntityFrameworkStores<ApplicationDbContext>();






            var app = builder.Build();
            app.UseStaticFiles();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || true)
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Graduation Project V1");
                    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                    options.DocumentTitle = "Graduation Project V1";
                    options.EnablePersistAuthorization();
                    options.DisplayRequestDuration();
                    //options.RoutePrefix = string.Empty; // Serve Swagger UI at the app's root
                    //options.InjectStylesheet("/swagger-ui/style.css");

                });
            }

            await SeedingScope(app);


            //app.Logger.LogInformation("Application Starting Up");



            app.UseMiddleware<IdempotencyCustomMiddleware>();
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseCompression();

            app.UseHttpsRedirection();
            app.UseRateLimiter();


            //app.UseStaticFiles();

            app.MapPost("/AI-Chat", async ([FromServices] GeminiService geminiService) =>
            {
                var budget = 3000;
                var products = new[]
                {
        new { Name = "Product A", Price = 1000 },
        new { Name = "Product B", Price = 1500 },
        new { Name = "Product C", Price = 800 },
        new { Name = "Product D", Price = 1200 },
        new { Name = "Product E", Price = 500 }
    };

                // Step 1: Compute all valid combinations locally
                var affordableProducts = products.Where(p => p.Price <= budget).ToArray();
                var combinations = new List<object>();

                int n = affordableProducts.Length;
                for (int i = 1; i < (1 << n); i++)
                {
                    var combo = new List<string>();
                    int total = 0;
                    for (int j = 0; j < n; j++)
                    {
                        if ((i & (1 << j)) != 0)
                        {
                            combo.Add(affordableProducts[j].Name);
                            total += affordableProducts[j].Price;
                        }
                    }

                    if (total <= budget)
                        combinations.Add(new
                        {
                            products = combo,
                            total_price = total,
                            remaining_budget = budget - total
                        });
                }

                combinations = combinations.OrderBy(c => (int)c.GetType().GetProperty("total_price").GetValue(c)).ToList();

                // Step 2: Prepare top N combinations for AI enrichment
                var topCombos = combinations.Take(5).Select(c => ((dynamic)c).products).ToList();
                var prompt = $@"
You are an API that returns ONLY valid JSON. Do not include markdown or extra text.
Given the top product combinations under a budget of {budget} and their names:

{string.Join("\n", topCombos.Select(c => string.Join(" + ", c)))}

Return JSON with the following schema for each combination:
{{
  ""products"": [string],
  ""total_price"": number,
  ""remaining_budget"": number,
  ""rank"": number,         // rank from best to worst
  ""description"": string,   // short recommendation
  ""image_url"": string      // optional image for product combination
}}

Return an array named ""enriched_recommendations"". Only JSON.
";

                // Step 3: Call AI
                var aiResponse = await geminiService.SendMessageAsync(prompt);

                // Step 4: Merge AI response with full deterministic combinations
                var jsonResponse = new
                {
                    budget,
                    products_available = affordableProducts.ToDictionary(p => p.Name, p => p.Price),
                    recommendations = combinations, // full list
                    enriched_recommendations = aiResponse // AI-enhanced top combos
                };

                return Results.Json(jsonResponse);
            });

            app.MapControllers();
            app.MapHub<ChatHub>("Hub /chatHub");
            //app.MapHub<NotificationHub>("Hub/notifications");

            //app.Run($"https://localhost:{builder.Configuration["PORT"]}");
            await app.RunAsync();
        }

        private static async Task SeedingScope(WebApplication app)
        {
            // ✅ Run DB seeders at startup
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var initializer = services.GetRequiredService<IDbIntialize>();
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

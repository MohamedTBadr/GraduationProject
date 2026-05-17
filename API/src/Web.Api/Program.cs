

using Application;
using Application.Services.Helpers;
using Domain.Contracts;
using Hangfire;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Caching.Hybrid;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
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
     

           
            builder.Host.UseSerilog((ctx, config) =>
            {
                config.ReadFrom.Configuration(ctx.Configuration)
                      .Enrich.FromLogContext()
                      
                      .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "logs/app-json-.log", rollingInterval: RollingInterval.Day)
                      .WriteTo.File("logs/app-text-.log", rollingInterval: RollingInterval.Day);
            });
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

            // Program.cs — add one line to logging
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.SetResourceBuilder(
                    ResourceBuilder.CreateDefault().AddService("GraduationProject-API"));

                logging.AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri(
                        Environment.GetEnvironmentVariable("Telemetry__Endpoint")
                        ?? "http://localhost:18889");
                });
            });

            var app = builder.Build();
              

            await SeedingScope(app);





            app.UseCors("CorsPolicy");                          // ← First
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseTelemetry();
            app.UseSerilogRequestLogging();
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseMiddleware<IdempotencyCustomMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHangfireDashboard();
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
                    Console.WriteLine($"Seeding failed: {ex}");
                }
            }
        }
    }
}
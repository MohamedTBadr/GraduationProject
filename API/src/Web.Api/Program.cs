using Application;
using Domain.Contracts;
using Hangfire;
using Infrastructure;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Serilog;
using Web.Api;
using Web.Api.Middlewares;

namespace Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

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

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    var allowedOrigins = builder.Configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>() ?? ["http://localhost:4200"];

                    policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

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

            app.UseCors("CorsPolicy");
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseTelemetry();
            app.UseSerilogRequestLogging();
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseMiddleware<IdempotencyCustomMiddleware>();
            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseAuthorization();
            app.UseHangfireDashboard("/hangfire", new Hangfire.DashboardOptions
            {
                Authorization = new[] { new HangfireAdminAuthorizationFilter() }
            });

            var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<Infrastructure.Search.LuceneSyncJob>(
                "lucene-daily-sync",
                job => job.SyncIndexAsync(),
                Cron.Daily);

            recurringJobManager.AddOrUpdate<Infrastructure.Jobs.ScheduledReportJob>(
                "monthly-vendor-reports",
                job => job.SendMonthlyVendorReportsAsync(CancellationToken.None),
                Cron.Monthly(1, 6));

            var adminReportEmail = builder.Configuration["AdminReports:Email"];
            if (!string.IsNullOrWhiteSpace(adminReportEmail))
            {
                recurringJobManager.AddOrUpdate<Infrastructure.Jobs.ScheduledReportJob>(
                    "monthly-admin-report",
                    job => job.SendAdminMonthlyReportAsync(adminReportEmail, CancellationToken.None),
                    Cron.Monthly(1, 7));
            }
            else
            {
                app.Logger.LogWarning("Monthly admin report job was not registered because AdminReports:Email is not configured.");
            }

            app.MapControllers();
            app.MapHub<ChatHub>("/Hub/chatHub");
            await app.RunAsync();
        }

        private static async Task SeedingScope(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var initializer = services.GetRequiredService<IDbIntialize>();
                await initializer.IntializeAsync();
            }
            catch (Exception ex)
            {
                app.Logger.LogCritical(ex, "Database seeding failed.");
                throw;
            }
        }
    }
}

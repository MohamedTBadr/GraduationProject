using Application;
using Domain.Contracts;
using Hangfire;
using Infrastructure;
using Infrastructure.Search;
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

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseTelemetry();
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseMiddleware<IdempotencyCustomMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHangfireDashboard("/hangfire", new Hangfire.DashboardOptions
            {
                Authorization = new[] { new HangfireAdminAuthorizationFilter() }
            });

            var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<LuceneSyncJob>(
           "lucene-daily-sync",
           job => job.SyncIndexAsync(),
           Cron.Daily);

            // Trigger immediately on startup
            BackgroundJob.Enqueue<LuceneSyncJob>(job => job.SyncIndexAsync());

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
                Console.WriteLine("Seeding database...");
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

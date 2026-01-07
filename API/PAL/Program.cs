
using BLL;
using DAL;
using Microsoft.AspNetCore.Identity;
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


            //app.Logger.LogInformation("Application Starting Up");



            app.UseMiddleware<IdempotencyCustomMiddleware>();
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseCompression();

            app.UseHttpsRedirection();
            app.UseRateLimiter();


            //app.UseStaticFiles();


            app.MapControllers();
            app.MapHub<ChatHub>("Hub/chatHub");
            app.MapHub<NotificationHub>("Hub/notifications");

            //app.Run($"https://localhost:{builder.Configuration["PORT"]}");
            await app.RunAsync();
        }
    }
}


using BLL;
using BLL.DTOs.AuthenticationDTOs;
using Common.Exceptions;
using DAL;
using DAL.Context;
using DAL.Entities;
using Google.GenAI;
using IdempotentAPI.Cache.DistributedCache.Extensions.DependencyInjection;
using IdempotentAPI.Core;
using IdempotentAPI.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PAL.Hubs;
using PAL.Middlewares;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Text;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

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
            await DataLayerRegistrationService.AddDataLayerRegistrationService(builder.Services,builder.Configuration);
            await    BusiniessLayerRegistrationService.AddBusinessLayerServices(builder.Services,builder.Configuration);
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




   

            app.UseMiddleware<IdempotencyCustomMiddleware>();
            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseResponseCompression();

            app.UseHttpsRedirection();
            app.UseRateLimiter();


            //app.UseStaticFiles();
            

            app.MapControllers();
            app.MapHub<ChatHub>("/chatHub");

            //app.Run($"https://localhost:{builder.Configuration["PORT"]}");
            await app.RunAsync();
        }
    }
}


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
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PAL.Hubs;
using PAL.Middlewares;
using System.IdentityModel.Tokens.Jwt;
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


      

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Graduation Project V1", Version = "v1" });

                // Define the security scheme
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer abc123\""
                });

                // Apply the security globally to all endpoints
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                      {
                        new OpenApiSecurityScheme
                        {
                           Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                         },
                         new string[] {}
                       }
                });
             });


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




            app.MapWhen(
                      context => (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method))&&(context.Request.Path.StartsWithSegments("/api/register")
//       || the rest coming soon
                            ),
                            builder =>
                            {
                                builder.UseMiddleware<IdempotencyCustomMiddleware>();
                            });

            app.UseMiddleware<CustomExceptionHandlerMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

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

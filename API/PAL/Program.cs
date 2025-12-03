
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


            //Configure Identity with your ApplicationUser
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


          
            // Configure Identity options (AFTER AddIdentity)
            builder.Services.Configure<IdentityOptions>(options =>
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


            //Gemini Configuration
            builder.Services.AddSingleton(provider =>
            {
                // Retrieve the API Key securely from configuration, environment variables, etc.
                // For this example, we'll try an environment variable first.

                var apiKey = builder.Configuration["Gemini:ApiKey"];

             

                if (string.IsNullOrEmpty(apiKey))
                {
                    throw new InvalidOperationException("Gemini API Key is not configured.");
                }

                return new Client(apiKey: apiKey);
            });


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || true)
            {
                app.UseSwagger();
                app.UseSwaggerUI();
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

            //app.Run($"https://localhost:{builder.Configuration["PORT"]}");
            await app.RunAsync();
        }
    }
}

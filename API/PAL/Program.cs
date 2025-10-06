
using BLL;
using BLL.DTOs.AuthenticationDTOs;
using Common.Exceptions;
using DAL;
using DAL.Context;
using DAL.Entities;
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





    


            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapWhen(
    context => (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method))&&(
    
        context.Request.Path.StartsWithSegments("/api/register")

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


            app.UseStaticFiles();


            app.MapControllers();

            app.Run();
        }
    }
}

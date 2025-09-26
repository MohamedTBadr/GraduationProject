
using BLL;
using BLL.DTOs.AuthenticationDTOs;
using Common.Exceptions;
using DAL;
using DAL.Context;
using DAL.Entities;
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

            builder.Services.AddControllers();
           await DataLayerRegistrationService.AddDataLayerRegistrationService(builder.Services,builder.Configuration);
            await    BusiniessLayerRegistrationService.AddBusinessLayerServices(builder.Services,builder.Configuration);
 

            //Configure Identity with your ApplicationUser
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();




            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 20,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.OnRejected = async (context, token) =>
                {
                    throw new RateLimitExceededException();
                };
            });


            
     

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

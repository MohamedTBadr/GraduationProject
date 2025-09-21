
using BLL;
using Common.Exceptions;
using DAL;
using PAL.Middlewares;
using System.Threading.RateLimiting;

namespace PAL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            DataLayerRegistrationService.AddDataLayerRegistrationService(builder.Services,builder.Configuration);
            BusiniessLayerRegistrationService.AddBusinessLayerServices(builder.Services);




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

            app.UseHttpsRedirection();
            app.UseRateLimiter();

            app.UseAuthorization();
            app.UseAuthentication();
            app.UseStaticFiles();


            app.MapControllers();

            app.Run();
        }
    }
}

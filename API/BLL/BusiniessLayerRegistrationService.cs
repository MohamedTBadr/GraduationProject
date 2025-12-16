using BLL.DTOs.AuthenticationDTOs;
using BLL.DTOs.PaymobDTOs;
using BLL.Services;
using BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class BusiniessLayerRegistrationService
    {
        public async static Task<IServiceCollection> AddBusinessLayerServices(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped<IEmailSender, EmailSenderService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IServiceManager, ServiceManager>();



            services.Configure<JWTOptions>(
  configuration.GetSection("JWTOptions"));
         ConfigureJWT(services, configuration);



            services.Configure<PaymobOptions>(
    configuration.GetSection("Paymob"));

            services.AddHttpClient<PaymobService>();



            return services;
        }

        public  static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            var jwt = configuration.GetSection("JWTOptions").Get<JWTOptions>();
            services.AddAuthentication(config =>
            {
                config.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(config =>
            {
                config.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt!.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,


                    ValidateLifetime=true

                    
                    ,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey))
                };
            });

        }
    }
}

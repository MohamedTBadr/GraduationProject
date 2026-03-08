using Amazon.S3;
using Application.DTOs.AuthenticationDTOs;
using Application.DTOs.PaymobDTOs;
using Application.Interfaces;
using Application.Services;
using Application.Services.Helpers;
using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application
{
    public static class ApplicationRegistrationService
    {
        public static async Task<IServiceCollection> AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IEmailSender, EmailSenderService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAttachmentService, AttachmentService>();
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<IVendorService, VendorService>();
            services.AddScoped<IServiceTypeService, ServiceTypeService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IEventItemService, EventItemService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();






            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMapperService>();

            });


            services.Configure<JWTOptions>(
  configuration.GetSection("JWTOptions"));
            ConfigureJWT(services, configuration);




            services.Configure<AwsSettings>(
    configuration.GetSection("AWS"));

            services.AddSingleton<IAmazonS3>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<AwsSettings>>().Value;

                return new AmazonS3Client(
                    config.AccessKey,
                    config.SecretKey,
                    Amazon.RegionEndpoint.GetBySystemName(config.Region)
                );
            });


            services.Configure<PaymobOptions>(
    configuration.GetSection("Paymob"));

            services.AddHttpClient<PaymobService>();



            return services;
        }

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
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


                    ValidateLifetime = true


                    ,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey))
                };
            });
        }

    
    }
}

using Amazon.S3;
using Application.DTOs;
using Application.DTOs.AuthenticationDTOs;
using Application.DTOs.PaymobDTOs;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;
using Application.Services.Helpers;
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
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddSingleton<SseConnectionManager>();
            services.AddScoped<NotificationService>();
            services.AddScoped<IFileService, FileService>();

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
                       // ❌ Missing this is a very common cause of 401
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // ✅ Add this

            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWTOptions:Issuer"],
                    ValidAudience = configuration["JWTOptions:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JWTOptions:SecretKey"]!))
                };
                // ✅ Add this block
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["accessToken"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) &&
                            path.StartsWithSegments("/Hub/chatHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        }

    
    }
}

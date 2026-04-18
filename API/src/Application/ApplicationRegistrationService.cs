using Amazon.S3;
using Application.DTOs;
using Application.DTOs.AuthenticationDTOs;
using Application.DTOs.PaymobDTOs;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;
using Application.Services.Helpers;
using Infrastructure.Payments;
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
        public static async Task<IServiceCollection> AddApplicationServices(this IServiceCollection Services, IConfiguration configuration)
        {
            Services.AddScoped<IEmailSender, EmailSenderService>();
            Services.AddScoped<IAuthenticationService, AuthenticationService>();
            Services.AddScoped<IAttachmentService, AttachmentService>();
            Services.AddScoped<IServiceManager, ServiceManager>();
            Services.AddScoped<IVendorService, VendorService>();
            Services.AddScoped<IServiceTypeService, ServiceTypeService>();
            Services.AddScoped<ICategoryService, CategoryService>();
            Services.AddScoped<IEventService, EventService>();
            Services.AddScoped<IEventItemService, EventItemService>();
            Services.AddScoped<IAuthenticationService, AuthenticationService>();
            Services.AddScoped<IChatService, ChatService>();
            Services.AddScoped<IServiceService, ServiceService>();
            Services.AddScoped<IOrderService, OrderService>();
            Services.AddSingleton<SseConnectionManager>();
            Services.AddScoped<NotificationService>();
            Services.AddScoped<IFileService, FileService>();
            Services.AddScoped<IEventTypeService, EventTypeService>();
            Services.AddScoped<ICompanyInquiryService, CompanyInquiryService>();
            //Services.AddScoped<IUs
            //Services.AddScoped<Ime>
            Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMapperService>();

            });


            Services.Configure<JWTOptions>(
  configuration.GetSection("JWTOptions"));
            ConfigureJWT(Services, configuration);




            Services.Configure<AwsSettings>(
    configuration.GetSection("AWS"));

            Services.AddSingleton<IAmazonS3>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<AwsSettings>>().Value;

                return new AmazonS3Client(
                    config.AccessKey,
                    config.SecretKey,
                    Amazon.RegionEndpoint.GetBySystemName(config.Region)
                );
            });


            Services.Configure<PaymobOptions>(
    configuration.GetSection("Paymob"));

            Services.AddHttpClient<PaymobService>();



            return Services;
        }

        public static void ConfigureJWT(this IServiceCollection Services, IConfiguration configuration)
        {
            var jwt = configuration.GetSection("JWTOptions").Get<JWTOptions>();
                       // ❌ Missing this is a very common cause of 401
            Services.AddAuthentication(options =>
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

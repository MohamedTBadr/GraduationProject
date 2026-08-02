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
using OpenAI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;

namespace Application
{
    public static class ApplicationRegistrationService
    {
        public static async Task<IServiceCollection> AddApplicationServices(this IServiceCollection Services, IConfiguration configuration)
        {
            Services.AddScoped<EmailSenderService>();
            Services.AddScoped<IEmailSender, HangfireEmailSender>();
            Services.AddScoped<IAuthenticationService, AuthenticationService>();
            Services.AddScoped<IAttachmentService, AttachmentService>();
            Services.AddScoped<IServiceManager, ServiceManager>();
            Services.AddScoped<IVendorService, VendorService>();
            Services.AddScoped<IServiceTypeService, ServiceTypeService>();
            Services.AddScoped<IEventService, EventService>();
            Services.AddScoped<IEventItemService, EventItemService>();
            Services.AddScoped<IChatService, ChatService>();
            Services.AddScoped<IServiceService, ServiceService>();
            Services.AddScoped<IOrderService, OrderService>();
            Services.AddSingleton<SseConnectionManager>();
            Services.AddScoped<NotificationService>();
            Services.AddScoped<IFileService, FileService>();
            Services.AddScoped<IEventTypeService, EventTypeService>();
            Services.AddScoped<IVendorTypeService, VendorTypeService>();
            Services.AddScoped<ICompanyInquiryService, CompanyInquiryService>();
            Services.AddScoped<ISupportTicketService, SupportTicketService>();
            Services.AddScoped<IVoucherService, VoucherService>();
            Services.AddScoped<IPlanningAIService, PlanningAIService>();

            // ── Paymob: named HttpClient + normal service registration ──────────
            Services.AddHttpClient("PaymobClient", (sp, client) =>
            {
                var paymobOptions = sp.GetRequiredService<IOptions<PaymobOptions>>().Value;

                if (!string.IsNullOrWhiteSpace(paymobOptions.BaseUrl))
                {
                    client.BaseAddress = new Uri(paymobOptions.BaseUrl);
                }
            });

            Services.AddScoped<IPaymobService, PaymobService>();
            // ──────────────────────────────────────────────────────────────────

            Services.AddScoped<Application.Contracts.IEmailService, HangfireEmailSender>();
            Services.AddScoped<IPackageService, PackageService>();
            Services.AddSingleton(sp =>
            {
                var apiKey = configuration["Groq:ApiKey"]
                             ?? throw new InvalidOperationException("Groq API key not set.");

                var openAIClient = new OpenAIClient(
                    new ApiKeyCredential(apiKey),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri("https://api.groq.com/openai/v1")
                    }
                );

                return openAIClient.GetChatClient("llama-3.3-70b-versatile");
            });

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

            return Services;
        }

        public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

                // ONLY for local dev if needed
                options.RequireHttpsMetadata = false;

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path.Value ?? "";

                        if (!string.IsNullOrEmpty(accessToken) &&
                            (path.Contains("chathub", StringComparison.OrdinalIgnoreCase) ||
                             path.Contains("notifications/stream", StringComparison.OrdinalIgnoreCase)))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },

                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT FAILED: {context.Exception.Message} | PATH: {context.Request.Path}");
                        return Task.CompletedTask;
                    },

                    OnTokenValidated = context =>
                    {
                        Console.WriteLine($"JWT OK: {context.Request.Path}");
                        return Task.CompletedTask;
                    }
                };
            });
        }
    }
}
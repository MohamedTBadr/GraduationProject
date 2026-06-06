using Domain.Contracts;
using Domain.Contracts.Caching;
using Domain.Contracts.Caching.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Search;
using Application.Interfaces;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using Application.Contracts;
using Infrastructure.Reporting;

namespace Infrastructure
{
    public static class InfrastructureRegistrationService
    {
        public static async Task<IServiceCollection> AddInfrastructureServices(this IServiceCollection Services, IConfiguration configuration)
        {
            //Services.AddScoped<IDbIntialize, DbIntialize>();
            Services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
            Services.AddScoped<IVendorRepository, VendorRepository>();
            Services.AddScoped<IEventRepository, EventRepository>();
            Services.AddScoped<IEventItemRepository, EventItemRepository>();
            Services.AddScoped<IMessageRepository, MessageRepository>();
            Services.AddScoped<IDbIntialize, DbIntialize>();
            Services.AddScoped<IServiceRepository, ServiceRepository>();
            Services.AddScoped<NotificationRepository>();
            Services.AddScoped<IOrderRepository, OrderRepository>();
            Services.AddScoped<IEventTypeRepository, EventTypeRespository>();
            Services.AddScoped<INotificationRepository, NotificationRepository>();
            Services.AddScoped<IVendorTypeRepository, VendorTypeRepository>();
            Services.AddScoped<ICacheRepository, CacheRepository>();
            Services.AddScoped<IMemoryCacheRepository, MemoryCacheRepository>();
            Services.AddScoped<IUserRepository, UserRepository>();
            Services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
            Services.AddScoped<IPackageRepository, PackageRepository>();



            Services.AddScoped<IReportingService, ReportingService>();
            Services.AddSingleton<IConnectionMultiplexer>(s => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
            Services.AddScoped<ICompanyInquiryRepository, CompanyInquiryRepository>();
            // 1. Configure DbContext with SQL Server
            Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                   configuration.GetConnectionString("DefaultConnection"), sqloptions =>
                   {
                       sqloptions.EnableRetryOnFailure(
                         maxRetryCount: 5,
                         maxRetryDelay: TimeSpan.FromSeconds(10),
                         errorNumbersToAdd: null
                       );
                   }));
            Services.AddScoped<IVoucherRepository, VoucherRepository>();
            Services.AddScoped<IReportHistoryRepository, ReportHistoryRepository>();
            Services.AddSingleton<ISearchService, LuceneSearchService>();
            Services.AddTransient<LuceneSyncJob>();




            return Services;
        }

    }
}
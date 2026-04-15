using Domain.Contracts;
using Domain.Contracts.Caching;
using Domain.Contracts.Caching.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure
{
    public static class InfrastructureRegistrationService
    {
            public static async Task<IServiceCollection> AddInfrastructureServices(this IServiceCollection Services, IConfiguration configuration)
            {
            //Services.AddScoped<IDbIntialize, DbIntialize>();
            Services.AddScoped<ICategoryRepository, CategoryRepository>();
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
            Services.AddScoped<ICacheRepository, CacheRepository>();
            Services.AddScoped<IMemoryCacheRepository, MemoryCacheRepository>();
            Services.AddScoped<IUserRepository, UserRepository>();
            Services.AddSingleton<IConnectionMultiplexer>(s => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
            // 1. Configure DbContext with SQL Server
            Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                   configuration.GetConnectionString("DefaultConnection")));




            return Services;
        }

    }
}

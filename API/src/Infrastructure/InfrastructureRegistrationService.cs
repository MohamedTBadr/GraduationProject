using Domain.Contracts;
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
            public static async Task<IServiceCollection> AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
            {
            //services.AddScoped<IDbIntialize, DbIntialize>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
            services.AddScoped<IVendorRepository, VendorRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEventItemRepository, EventItemRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IDbIntialize, DbIntialize>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddSingleton<IConnectionMultiplexer>(s => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
            // 1. Configure DbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                   configuration.GetConnectionString("DefaultConnection")));




            return services;
        }

    }
}

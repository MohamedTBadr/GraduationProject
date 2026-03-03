using DAL.Context;
using DAL.Repositories;
using DAL.Repositories.Caching;
using DAL.Repositories.Caching.Interfaces;
using DAL.Repositories.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public static class DataLayerRegistrationService
    {
        public static async Task<IServiceCollection> AddDataLayerRegistrationService(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped<IDbIntialize, DbIntialize>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();

            services.AddSingleton<IConnectionMultiplexer>(s => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
            // 1. Configure DbContext with SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                   configuration.GetConnectionString("DefaultConnection")));







            return services;
        } 
    }
}

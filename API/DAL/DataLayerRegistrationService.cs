using DAL.Repositories;
using DAL.Repositories.Interfaces;
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
            services.AddScoped<ICacheRepository, CacheRepository>();
            services.AddSingleton<IConnectionMultiplexer>(s => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));

            return services;
        } 
    }
}

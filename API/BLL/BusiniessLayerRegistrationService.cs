using BLL.Services;
using BLL.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public static class BusiniessLayerRegistrationService
    {
        public static IServiceCollection AddBusinessLayerServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailSender, EmailSenderService>();
            services.AddScoped<ICacheService, CacheService>();
            return services;
        }
    }
}

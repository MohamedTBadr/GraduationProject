using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ServiceTypeRepository(ApplicationDbContext dbContext) : IServiceTypeRepository
    {

        public async Task<List<ServiceType>> GetAllServiceTypesAsync()
        {
            var serviceTypes = await dbContext.ServiceTypes.ToListAsync();
            return serviceTypes;
        }


        public async Task<ServiceType> GetServiceTypeByIdAsync(Guid id)
        {
            var serviceType = await dbContext.ServiceTypes.FindAsync(id);
            return serviceType;
        }


        public async Task DeleteTypeAsync(Guid id)
        {
            var serviceType = await dbContext.ServiceTypes.FindAsync(id);
            if (serviceType != null)
            {
                dbContext.ServiceTypes.Remove(serviceType);
                await dbContext.SaveChangesAsync();
            }
        }



        public async Task AddTypeAsync(ServiceType ServiceType)
        {
            dbContext.ServiceTypes.Add(ServiceType);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateTypeAsync(ServiceType ServiceType)
        {
            dbContext.ServiceTypes.Update(ServiceType);
            await dbContext.SaveChangesAsync();
        }
    }
}

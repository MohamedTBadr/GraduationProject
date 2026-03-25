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
            var ServiceTypes = await dbContext.ServiceTypes.ToListAsync();
            return ServiceTypes;
        }


        public async Task<ServiceType> GetServiceTypeByIdAsync(Guid id)
        {
            var ServiceType = await dbContext.ServiceTypes.FindAsync(id);
            return ServiceType;
        }


        public async Task DeleteTypeAsync(Guid id)
        {
            var ServiceType = await dbContext.ServiceTypes.FindAsync(id);
            if (ServiceType != null)
            {
                dbContext.ServiceTypes.Remove(ServiceType);
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

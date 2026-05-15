using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Infrastructure.Repositories
{
    public class ServiceTypeRepository(ApplicationDbContext dbContext) : IServiceTypeRepository
    {
        public async Task<List<ServiceType>> GetAllServiceTypesAsync(CancellationToken cancellationToken)
        {
            var ServiceTypes =  await dbContext.ServiceTypes.ToListAsync(cancellationToken);
            return ServiceTypes;
        }


        public async Task<ServiceType> GetServiceTypeByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var ServiceType = await dbContext.ServiceTypes.FindAsync([id], cancellationToken);
            return ServiceType;
        }


        public async Task DeleteTypeAsync(Guid id, CancellationToken cancellationToken)
        {
            var ServiceType = await dbContext.ServiceTypes.FindAsync([id], cancellationToken);
            if (ServiceType != null)
            {
                dbContext.ServiceTypes.Remove(ServiceType);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }



        public async Task AddTypeAsync(ServiceType ServiceType, CancellationToken cancellationToken)
        {
            await dbContext.ServiceTypes.AddAsync(ServiceType, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateTypeAsync(ServiceType ServiceType, CancellationToken cancellationToken)
        {
            dbContext.ServiceTypes.Update(ServiceType);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

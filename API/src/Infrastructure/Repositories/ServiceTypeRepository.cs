using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;

namespace Infrastructure.Repositories
{
    public class ServiceTypeRepository(ApplicationDbContext dbContext , ResiliencePipelineProvider<string> pipelineProvider) : IServiceTypeRepository
    {
         private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");
        public async Task<List<ServiceType>> GetAllServiceTypesAsync(CancellationToken cancellationToken)
        {
            var ServiceTypes = await _pipeline.ExecuteAsync(async token => await dbContext.ServiceTypes.ToListAsync(token), cancellationToken);
            return ServiceTypes;
        }


        public async Task<ServiceType> GetServiceTypeByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var ServiceType = await _pipeline.ExecuteAsync(async token => await dbContext.ServiceTypes.FindAsync([id], token), cancellationToken);
            return ServiceType;
        }


        public async Task DeleteTypeAsync(Guid id, CancellationToken cancellationToken)
        {
            var ServiceType = await _pipeline.ExecuteAsync(async token => await dbContext.ServiceTypes.FindAsync([id], token), cancellationToken);
            if (ServiceType != null)
            {
                dbContext.ServiceTypes.Remove(ServiceType);
                await _pipeline.ExecuteAsync(async token => await dbContext.SaveChangesAsync(token), cancellationToken);
            }
        }



        public async Task AddTypeAsync(ServiceType ServiceType, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token => await dbContext.ServiceTypes.AddAsync(ServiceType, token), cancellationToken);
            await _pipeline.ExecuteAsync(async token => await dbContext.SaveChangesAsync(token), cancellationToken);
        }

        public async Task UpdateTypeAsync(ServiceType ServiceType, CancellationToken cancellationToken)
        {
            dbContext.ServiceTypes.Update(ServiceType);
            await _pipeline.ExecuteAsync(async token => await dbContext.SaveChangesAsync(token), cancellationToken);
        }
    }
}

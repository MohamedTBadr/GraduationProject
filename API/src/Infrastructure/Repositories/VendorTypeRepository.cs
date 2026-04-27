using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class VendorTypeRepository(ApplicationDbContext dbContext, ResiliencePipelineProvider<string> pipelineProvider) : IVendorTypeRepository
    {

        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");

        public async Task AddVendorTypeAsync(VendorType vendorType, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async ct =>
            {
                dbContext.VendorTypes.Add(vendorType);
                await dbContext.SaveChangesAsync(ct);
            }, cancellationToken);
        }

        public async Task DeleteVendorTypeAsync(Guid id, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async ct =>
            {
                var vendorType = await dbContext.VendorTypes.FindAsync([id], ct);
                if (vendorType != null)
                {
                    dbContext.VendorTypes.Remove(vendorType);
                    await dbContext.SaveChangesAsync(ct);
                }
            }, cancellationToken);
        }

        public async Task<VendorType?> GetVendorTypeByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async ct =>
            {
                return await dbContext.VendorTypes.FindAsync(new object[] { id }, ct);
            }, cancellationToken);
        }

        public async Task<IReadOnlyList<VendorType>> GetVendorTypesAsync(CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async ct =>
            {
                return await dbContext.VendorTypes
                    .AsNoTracking()
                    .ToListAsync(ct);

            }, cancellationToken);
        }

        public async Task UpdateVendorTypeAsync(VendorType vendorType, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async ct =>
            {
                dbContext.VendorTypes.Update(vendorType);
                await dbContext.SaveChangesAsync(ct);
            }, cancellationToken);
        }
    }
}

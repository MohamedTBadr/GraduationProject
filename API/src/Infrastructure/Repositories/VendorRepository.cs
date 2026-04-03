using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;
namespace Infrastructure.Repositories
{
    public class VendorRepository(
     ApplicationDbContext dbContext,
     ResiliencePipelineProvider<string> pipelineProvider) : IVendorRepository
    {
        // Resolve the specific pipeline by name using the provider
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");
        public async Task<List<Vendor>> GetVendorsAsync(CancellationToken cancellationToken)
        {
            var vendors = await _pipeline.ExecuteAsync(async token =>  await dbContext.Vendors
                .Include(x=>x.User)
               .AsNoTracking().ToListAsync(token), cancellationToken);
            return vendors;
        }

        public async Task<Vendor?> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var vendor = await _pipeline.ExecuteAsync(async token => await dbContext.Vendors.Include(v => v.VendorRatings).Include(x=>x.Packages).Include(x=>x.Services).FirstOrDefaultAsync(v => v.UserId == id, token), cancellationToken);
            return vendor;
        }

        public async Task AddVendorAsync(Vendor vendor, CancellationToken cancellationToken)
        {
            // Tell EF the User already exists in DB, don't try to insert it again
            if (vendor.User != null)
                dbContext.Entry(vendor.User).State = EntityState.Unchanged;

            await _pipeline.ExecuteAsync(async token =>
            {
                await dbContext.Vendors.AddAsync(vendor, token);
                await dbContext.SaveChangesAsync(token);
            }, cancellationToken);
        }

        public async Task UpdateVendorAsync(Vendor vendor, CancellationToken cancellationToken)
        {
            dbContext.Vendors.Update(vendor);
            await _pipeline.ExecuteAsync(async token => await dbContext.SaveChangesAsync(token), cancellationToken);  
        }

        public async Task DeleteVendorAsync(Vendor vendor, CancellationToken cancellationToken)
        {
            dbContext.Vendors.Remove(vendor);

            await _pipeline.ExecuteAsync(async token => await dbContext.SaveChangesAsync(token), cancellationToken);
        }

    }
}

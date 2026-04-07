using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;
using Shared;
using System.Linq.Expressions;
namespace Infrastructure.Repositories
{
    public class VendorRepository(
     ApplicationDbContext dbContext,
     ResiliencePipelineProvider<string> pipelineProvider) : IVendorRepository
    {
        // Resolve the specific pipeline by name using the provider
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");
        public async Task<PaginatedResponse<Vendor>> GetVendorsAsync(PaginatedRequest request, Expression<Func<Vendor, bool>> visibilityFilter, CancellationToken cancellationToken)
        {
            // 1. Start with the IQueryable
            var query = dbContext.Vendors.Where(visibilityFilter)
                .Include(x => x.User)
                .Include(x => x.Services)
                    .ThenInclude(s => s.ServiceRatings)
                .AsNoTracking();

            // 2. Apply Search Filter
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                query = query.Where(v =>
                     (v.BusinessName ?? "").ToLower().Contains(search) ||
                     (v.Description ?? "").ToLower().Contains(search) ||
                     (v.User != null && (v.User.FirstName ?? "").ToLower().Contains(search)) ||
                     (v.User != null && (v.User.LastName ?? "").ToLower().Contains(search)));
            }

            // 3. Apply Ordering by Average Rating (Highest First)
            // We flatten all ratings from all services for that vendor and take the average
            query = query.OrderByDescending(v => v.Services
                .SelectMany(s => s.ServiceRatings)
                .Average(r => (decimal?)r.Rating) ?? 0); // Use nullable decimal to handle vendors with 0 ratings

            // 4. Get the total count of FILTERED records
            var totalRecords = await query.CountAsync(cancellationToken);

            // 5. Apply Pagination
            var data = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<Vendor>(data, totalRecords, request.PageIndex, request.PageSize);
        }

        public async Task<Vendor?> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var vendor = await _pipeline.ExecuteAsync(async token => await dbContext.Vendors.Include(x=>x.Services).ThenInclude(s => s.ServiceRatings).Include(x=>x.Packages).FirstOrDefaultAsync(v => v.UserId == id, token), cancellationToken);
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

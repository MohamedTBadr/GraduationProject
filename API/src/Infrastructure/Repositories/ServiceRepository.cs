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
    public class ServiceRepository(ApplicationDbContext _context, ResiliencePipelineProvider<string> pipelineProvider) : IServiceRepository
    {
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");

        public async Task<List<ServiceImage>> GetServiceImagesAsync(Guid serviceId, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.ServiceImages
                    .Where(i => i.ServiceId == serviceId)
                    .ToListAsync(token), cancellationToken);
        }

        public async Task<PaginatedResponse<Service>> GetAllAsync(
          PaginatedRequest request,
          Expression<Func<Service, bool>> visibilityFilter,
          CancellationToken ct)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var query = _context.Services.AsNoTracking().Where(visibilityFilter);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var search = request.SearchTerm.Trim();
                    query = query.Where(p =>
                        p.Name.Contains(search) ||
                        p.Description.Contains(search) ||
                        p.Vendor.BusinessName.Contains(search) ||
                        p.ServiceType.Name.Contains(search));
                }

                query = request.SortBy?.ToLower() switch
                {
                    "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "vendor" => request.IsDescending ? query.OrderByDescending(p => p.Vendor.BusinessName) : query.OrderBy(p => p.Vendor.BusinessName),
                    "servicetype" => request.IsDescending ? query.OrderByDescending(p => p.ServiceType.Name) : query.OrderBy(p => p.ServiceType.Name),
                    _ => query.OrderBy(p => p.Name)
                };

                var totalCount = await query.CountAsync(token);
                var items = await query
                 
                    .Include(p => p.Vendor)
                    .Include(p => p.ServiceType)
                    .Include(p => p.ServiceImages)
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(token);

                return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
            }, ct);
        }

        //public async Task<PaginatedResponse<Service>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request, Expression<Func<Service, bool>> visibilityFilter, CancellationToken cancellationToken)
        //{
        //    return await _pipeline.ExecuteAsync(async token =>
        //    {
        //        var query = _context.Services
                   
        //            .Include(p => p.Vendor)
        //            .Include(p => p.ServiceType)
        //            .Include(p => p.ServiceImages)
        //            .Where(p => p.CategoryId == categoryId)
        //            .Where(visibilityFilter)
        //            .AsNoTracking();

        //        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        //            query = query.Where(p => p.Name.Contains(request.SearchTerm) || p.Description.Contains(request.SearchTerm));

        //        var totalCount = await query.CountAsync(token);
        //        var items = await query
        //            .Skip((request.PageIndex - 1) * request.PageSize)
        //            .Take(request.PageSize)
        //            .ToListAsync(token);

        //        return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
        //    }, cancellationToken);
        //}

        public async Task<PaginatedResponse<Service>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request, Expression<Func<Service, bool>> visibilityFilter, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var query = _context.Services
                    .Where(p => p.VendorId == vendorId)
                    .Where(visibilityFilter)
                    .AsNoTracking();

                var totalCount = await query.CountAsync(token);
                var items = await query
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(token);

                return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
            }, cancellationToken);
        }

        public async Task<PaginatedResponse<Service>> GetByServiceTypeIdAsync(Guid ServiceTypeId, PaginatedRequest request, Expression<Func<Service, bool>> visibilityFilter, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var query = _context.Services.Where(p => p.ServiceTypeId == ServiceTypeId).Where(visibilityFilter).AsNoTracking();
                var totalCount = await query.CountAsync(token);
                var items = await query.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToListAsync(token);
                return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
            }, cancellationToken);
        }

        public async Task<Service> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Services
                    .Include(p => p.Vendor).Include(p => p.ServiceType).Include(p => p.ServiceImages)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id, token), cancellationToken);
        }

        public async Task<Service> CreateAsync(Service service, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                service.Id = Guid.NewGuid();
                await _context.Services.AddAsync(service, token);

                foreach (var img in service.ServiceImages)
                {
                    img.Id = Guid.NewGuid();
                    img.ServiceId = service.Id;
                }

                await _context.ServiceImages.AddRangeAsync(service.ServiceImages, token);
                await _context.SaveChangesAsync(token);
                return service;
            }, cancellationToken);
        }

        public async Task<Service> UpdateAsync(Service service, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                _context.Services.Update(service);
                if (service.ServiceImages?.Any() == true)
                {
                    foreach (var image in service.ServiceImages) image.ServiceId = service.Id;
                    await _context.ServiceImages.AddRangeAsync(service.ServiceImages, token);
                }
                await _context.SaveChangesAsync(token);
                return service;
            }, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                var service = await _context.Services.FindAsync([id], token);
                if (service is not null)
                {
                    var images = _context.ServiceImages.Where(pi => pi.ServiceId == id);
                    _context.ServiceImages.RemoveRange(images);
                    _context.Services.Remove(service);
                    await _context.SaveChangesAsync(token);
                }
            }, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Services.AnyAsync(p => p.Id == id, token), cancellationToken);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, bool isActive, CancellationToken ct)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var rowsAffected = await _context.Services
                    .Where(s => s.Id == id)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.IsHidden, isActive), token);
                return rowsAffected > 0;
            }, ct);
        }

        public async Task AddRatingAsync(ServiceRating rating, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                rating.Id = Guid.NewGuid();
                await _context.ServiceRatings.AddAsync(rating, token);
                await _context.SaveChangesAsync(token);
            }, cancellationToken);
        }

        public async Task<List<Service>> AIFilterAsync(AIRequest AIRequest, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Services
                    .Where(p => p.Price < AIRequest.Budget && p.Price > 0)
                    .Include(p => p.Vendor).Include(p => p.ServiceType).Include(p => p.ServiceImages)
                    .AsNoTracking()
                    .ToListAsync(token), cancellationToken);
        }

        public async Task DeleteServiceImagesAsync(Guid serviceId, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                var oldImages = await _context.ServiceImages.Where(i => i.ServiceId == serviceId).ToListAsync(token);
                _context.ServiceImages.RemoveRange(oldImages);
                await _context.SaveChangesAsync(token);
            }, cancellationToken);
        }

        public async Task<bool> HasUserPurchasedAsync(Guid userId, Guid serviceId, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Orders
                    .AnyAsync(o => o.UserId == userId && o.OrderItems.Any(oi => oi.ServiceId == serviceId), token),
                cancellationToken);
        }
    }
}
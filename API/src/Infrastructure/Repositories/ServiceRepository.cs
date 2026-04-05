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
    public class ServiceRepository(ApplicationDbContext _context ,ResiliencePipelineProvider<string> pipelineProvider) : IServiceRepository
    {
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");

        public async Task<PaginatedResponse<Service>> GetAllAsync(
          PaginatedRequest request,
          Expression<Func<Service, bool>> visibilityFilter, // Dynamic filter passed from Service
          CancellationToken ct)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var query = _context.Services.AsNoTracking()
                    .Where(visibilityFilter); // Just apply what the Service decided



                // --- 2. SEARCH LOGIC ---
                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var search = request.SearchTerm.Trim();
                    query = query.Where(p =>
                        p.Name.Contains(search) ||
                        p.Description.Contains(search) ||
                        p.Category.Name.Contains(search) ||
                        p.Vendor.BusinessName.Contains(search) ||
                        p.ServiceType.Name.Contains(search));
                }

                // --- 3. SORTING LOGIC ---
                query = request.SortBy?.ToLower() switch
                {
                    "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "category" => request.IsDescending ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
                    "vendor" => request.IsDescending ? query.OrderByDescending(p => p.Vendor.BusinessName) : query.OrderBy(p => p.Vendor.BusinessName),
                    "servicetype" => request.IsDescending ? query.OrderByDescending(p => p.ServiceType.Name) : query.OrderBy(p => p.ServiceType.Name),
                    _ => query.OrderBy(p => p.Name) 
                };

                // --- 4. EXECUTION ---
                var totalCount = await query.CountAsync(token);

                var items = await query
                    .Include(p => p.Category)
                    .Include(p => p.Vendor)
                    .Include(p => p.ServiceType)
                    .Include(p => p.ServiceImages)
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(token);

                return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
            }, ct);
        }


        public async Task<PaginatedResponse<Service>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request, CancellationToken cancellationToken)
        {
            var query = _context.Services
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .Where(p => p.CategoryId == categoryId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(p =>
                    p.Name.Contains(request.SearchTerm) ||
                    p.Description.Contains(request.SearchTerm) ||
                    p.Vendor.BusinessName.Contains(request.SearchTerm) ||
                    p.ServiceType.Name.Contains(request.SearchTerm));

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "vendor" => request.IsDescending ? query.OrderByDescending(p => p.Vendor.BusinessName) : query.OrderBy(p => p.Vendor.BusinessName),
                "Servicetype" => request.IsDescending ? query.OrderByDescending(p => p.ServiceType.Name) : query.OrderBy(p => p.ServiceType.Name),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<PaginatedResponse<Service>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request, CancellationToken cancellationToken)
        {
            var query = _context.Services
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .Where(p => p.VendorId == vendorId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(p =>
                    p.Name.Contains(request.SearchTerm) ||
                    p.Description.Contains(request.SearchTerm) ||
                    p.Category.Name.Contains(request.SearchTerm) ||
                    p.ServiceType.Name.Contains(request.SearchTerm));

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "category" => request.IsDescending ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
                "Servicetype" => request.IsDescending ? query.OrderByDescending(p => p.ServiceType.Name) : query.OrderBy(p => p.ServiceType.Name),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<PaginatedResponse<Service>> GetByServiceTypeIdAsync(Guid ServiceTypeId, PaginatedRequest request, CancellationToken cancellationToken)
        {
            var query = _context.Services
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .Where(p => p.ServiceTypeId == ServiceTypeId)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(p =>
                    p.Name.Contains(request.SearchTerm) ||
                    p.Description.Contains(request.SearchTerm) ||
                    p.Category.Name.Contains(request.SearchTerm) ||
                    p.Vendor.BusinessName.Contains(request.SearchTerm));

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "category" => request.IsDescending ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
                "vendor" => request.IsDescending ? query.OrderByDescending(p => p.Vendor.BusinessName) : query.OrderBy(p => p.Vendor.BusinessName),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
        }
        public async Task<Service> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Services
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
        public async Task<Service> CreateAsync(Service Service, CancellationToken cancellationToken)
        {
            Service.Id = Guid.NewGuid();
            await _context.Services.AddAsync(Service, cancellationToken);
            await _context.ServiceImages.AddRangeAsync(Service.ServiceImages.Select(pi =>
            {
                pi.Id = Guid.NewGuid();
                pi.ServiceId = Service.Id;
                pi.ImagePath = $"{pi.ImagePath}";
                return pi;
            }));
            await _context.SaveChangesAsync(cancellationToken);
            return Service;
        }

        public async Task<Service> UpdateAsync(Service Service, CancellationToken cancellationToken)
        {
            _context.Services.Update(Service);
            if (Service.ServiceImages != null && Service.ServiceImages.Count > 0)
            {
                foreach (var image in Service.ServiceImages)
                {
                    image.ServiceId = Service.Id;
                    image.ImagePath = $"/images/Services/{image.ImagePath}.jpg";
                }
                _context.ServiceImages.UpdateRange(Service.ServiceImages);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Service;
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                // 1. Fetch the entity
                var service = await _context.Services.FindAsync([id], token);

                if (service is not null)
                {
                    // 2. Remove related images (In-memory tracking)
                    var images = _context.ServiceImages.Where(pi => pi.ServiceId == id);
                    _context.ServiceImages.RemoveRange(images);

                    // 3. Remove the service
                    _context.Services.Remove(service);

                    // 4. Save everything in ONE transaction
                    await _context.SaveChangesAsync(token);
                }
            }, cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Services.AnyAsync(p => p.Id == id, cancellationToken);
        }


        public async Task<bool> UpdateStatusAsync(Guid id, bool isActive, CancellationToken ct)
        {
            var rowsAffected = await _context.Services
                .Where(s => s.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.IsHidden, isActive), ct);

            return rowsAffected > 0;
        }


        public Task<List<Service>> AIFilterAsync(AIRequest AIRequest, CancellationToken cancellationToken)
        {
            var query = _context.Services
                .Where(p => p.Price < AIRequest.Budget && p.Price > 0)
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(AIRequest.CategoryName))
                query = query.Where(p => p.Category.Name == AIRequest.CategoryName);

            return query.ToListAsync(cancellationToken);
        }
    }
}

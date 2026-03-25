using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Domain.Contracts;
using Shared;
using Domain.Entities;
namespace Infrastructure.Repositories
{
    public class ServiceRepository(ApplicationDbContext _context) : IServiceRepository
    {


        public async Task<PaginatedResponse<Service>> GetAllAsync(PaginatedRequest request)
        {
            var query = _context.Services
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .Include(p=>p.ServiceImages)
                .AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(p =>
                    p.Name.Contains(request.SearchTerm) ||
                    p.Description.Contains(request.SearchTerm) ||
                    p.Category.Name.Contains(request.SearchTerm) ||
                    p.Vendor.BusinessName.Contains(request.SearchTerm) ||
                    p.ServiceType.Name.Contains(request.SearchTerm));

            // Sort
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "category" => request.IsDescending ? query.OrderByDescending(p => p.Category.Name) : query.OrderBy(p => p.Category.Name),
                "vendor" => request.IsDescending ? query.OrderByDescending(p => p.Vendor.BusinessName) : query.OrderBy(p => p.Vendor.BusinessName),
                "Servicetype" => request.IsDescending ? query.OrderByDescending(p => p.ServiceType.Name) : query.OrderBy(p => p.ServiceType.Name),
                _ => query.OrderBy(p => p.Name) // default
            };

            // Replace PaginatedResult<Service> { Items = ..., TotalCount = ..., ... } with:
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
        }



        public async Task<PaginatedResponse<Service>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request)
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
                .ToListAsync();

            return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<PaginatedResponse<Service>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request)
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
                .ToListAsync();

            return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<PaginatedResponse<Service>> GetByServiceTypeIdAsync(Guid ServiceTypeId, PaginatedRequest request)
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
                .ToListAsync();

            return new PaginatedResponse<Service>(items, totalCount, request.PageIndex, request.PageSize);
        }
        public async Task<Service> GetByIdAsync(Guid id)
        {
            return await _context.Services
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Service> CreateAsync(Service Service)
        {
            Service.Id = Guid.NewGuid();
            await _context.Services.AddAsync(Service);
            await _context.ServiceImages.AddRangeAsync(Service.ServiceImages.Select(pi =>
            {
                pi.Id = Guid.NewGuid();
                pi.ServiceId = Service.Id;
                pi.ImagePath = $"{pi.ImagePath}";
                return pi;
            }));
            await _context.SaveChangesAsync();
            return Service;
        }

        public async Task<Service> UpdateAsync(Service Service)
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

            await _context.SaveChangesAsync();
            return Service;
        }

        public async Task DeleteAsync(Guid id)
        {
            var Service = await _context.Services.FindAsync(id);
            if (Service is not null)
            {
                _context.Services.Remove(Service);
                _context.ServiceImages.RemoveRange(_context.ServiceImages.Where(pi => pi.ServiceId == id));
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Services.AnyAsync(p => p.Id == id);
        }

        public Task<List<Service>> AIFilterAsync(AIRequest AIRequest)
        {
            var query = _context.Services
                .Where(p => p.Price < AIRequest.Budget && p.Price > 0)
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(AIRequest.CategoryName))
                query = query.Where(p => p.Category.Name == AIRequest.CategoryName);

            return query.ToListAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Domain.Contracts;
using Shared;
using Domain.Entities;
namespace Infrastructure.Repositories
{
    public class ProductRepository(ApplicationDbContext _context) : IProductRepository
    {


        public async Task<PaginatedResponse<Product>> GetAllAsync(PaginatedRequest request)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
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
                "servicetype" => request.IsDescending ? query.OrderByDescending(p => p.ServiceType.Name) : query.OrderBy(p => p.ServiceType.Name),
                _ => query.OrderBy(p => p.Name) // default
            };

            // Replace PaginatedResult<Product> { Items = ..., TotalCount = ..., ... } with:
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResponse<Product>(items, totalCount, request.PageIndex, request.PageSize);
        }



        public async Task<PaginatedResponse<Product>> GetByCategoryIdAsync(Guid categoryId, PaginatedRequest request)
        {
            var query = _context.Products
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
                "servicetype" => request.IsDescending ? query.OrderByDescending(p => p.ServiceType.Name) : query.OrderBy(p => p.ServiceType.Name),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResponse<Product>(items, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<PaginatedResponse<Product>> GetByVendorIdAsync(Guid vendorId, PaginatedRequest request)
        {
            var query = _context.Products
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
                "servicetype" => request.IsDescending ? query.OrderByDescending(p => p.ServiceType.Name) : query.OrderBy(p => p.ServiceType.Name),
                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResponse<Product>(items, totalCount, request.PageIndex, request.PageSize);
        }

        public async Task<PaginatedResponse<Product>> GetByServiceTypeIdAsync(Guid serviceTypeId, PaginatedRequest request)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .Where(p => p.ServiceTypeId == serviceTypeId)
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
                .Skip((request. PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResponse<Product>(items, totalCount, request.PageIndex, request.PageSize);
        }
        public async Task<Product> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Vendor)
                .Include(p => p.ServiceType)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Product> CreateAsync(Product product)
        {
            product.Id = Guid.NewGuid();
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is not null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Products.AnyAsync(p => p.Id == id);
        }

    }
}

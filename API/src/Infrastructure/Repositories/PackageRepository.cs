using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PackageRepository(ApplicationDbContext _context) : IPackageRepository
    {
        public async Task<Package> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var package = await _context.Packages
                .Include(p => p.Vendor)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (package == null)
                return null;

            package.Services = await _context.Services
                .Where(s => package.ServiceIds.Contains(s.Id))
                .ToListAsync(cancellationToken);

            return package;
        }

        public async Task<PaginatedResponse<Package>> GetAllAsync(
     PaginatedRequest request,
     Expression<Func<Package, bool>> visibilityFilter,
     CancellationToken ct)
        {
            var query = _context.Packages
                .AsNoTracking()
                .Include(p => p.Vendor)
                .Where(visibilityFilter);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.Trim();

                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search) ||
                    p.Vendor.BusinessName.Contains(search));
            }

            if (request.VendorId.HasValue)
                query = query.Where(p => p.VendorId == request.VendorId.Value);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice.Value);

            query = request.SortBy?.ToLower() switch
            {
                "name" => request.IsDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),

                "vendor" => request.IsDescending
                    ? query.OrderByDescending(p => p.Vendor.BusinessName)
                    : query.OrderBy(p => p.Vendor.BusinessName),

                "price" => request.IsDescending
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),

                _ => query.OrderBy(p => p.Name)
            };

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            // collect all ids once
            var allServiceIds = items
                .SelectMany(p => p.ServiceIds)
                .Distinct()
                .ToList();

            var services = await _context.Services
                .Where(s => allServiceIds.Contains(s.Id))
                .ToListAsync(ct);

            // map services to packages
            foreach (var package in items)
            {
                package.Services = services
                    .Where(s => package.ServiceIds.Contains(s.Id))
                    .ToList();
            }

            return new PaginatedResponse<Package>(
                items,
                totalCount,
                request.PageIndex,
                request.PageSize);
        }

        public async Task<Package> CreateAsync(Package package, CancellationToken cancellationToken)
        {
            await _context.Packages.AddAsync(package, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return package;
        }

        public async Task<Package> UpdateAsync(Package package, CancellationToken cancellationToken)
        {
            _context.Packages.Update(package);
            await _context.SaveChangesAsync(cancellationToken);
            return await Task.FromResult(package);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var package = await _context.Packages.FindAsync(new object[] { id }, cancellationToken);
            if (package is not null)
            {
                _context.Packages.Remove(package);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Packages.AnyAsync(p => p.Id == id, cancellationToken);
        }
    }
}

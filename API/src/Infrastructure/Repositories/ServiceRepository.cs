using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;
using Shared;
using System.Drawing;
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

                if (request.VendorId.HasValue)
                    query = query.Where(p => p.VendorId == request.VendorId.Value);

                if (request.ServiceTypeId.HasValue)
                    query = query.Where(p => p.ServiceTypeId == request.ServiceTypeId.Value);

                if (request.MinPrice.HasValue)
                    query = query.Where(p => p.Price >= request.MinPrice.Value);

                if (request.MaxPrice.HasValue)
                    query = query.Where(p => p.Price <= request.MaxPrice.Value);

                query = request.SortBy?.ToLower() switch
                {
                    "name" => request.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "vendor" => request.IsDescending ? query.OrderByDescending(p => p.Vendor.BusinessName) : query.OrderBy(p => p.Vendor.BusinessName),
                    "servicetype" => request.IsDescending ? query.OrderByDescending(p => p.ServiceType.Name) : query.OrderBy(p => p.ServiceType.Name),
                    _ => query.OrderBy(p => p.Name)
                };

                // ✅ Phase 1 — SQL only (bounding box + city/region), stays IQueryable
                query = ApplyLocationSqlFilter(request, query);

                // ✅ Count runs on EF IQueryable — no ToList yet
                var totalCount = await query.CountAsync(token);

                // ✅ Paginate + fetch from DB
                var items = await query
                    .Include(p => p.Vendor)
                        .ThenInclude(v => v.ServiceAreas)   // 👈 needed for Haversine phase
                    .Include(p => p.ServiceType)
                    .Include(p => p.ServiceImages)
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(token);                    // ← only DB round-trip

                // ✅ Phase 2 — Haversine in-memory on paged items only
                var filtered = ApplyHaversineFilter(request, items);

                return new PaginatedResponse<Service>(filtered, totalCount, request.PageIndex, request.PageSize);
            }, ct);
        }

        // ─────────────────────────────────────────────────────────────
        // Phase 1 — SQL bounding box (IQueryable, EF translatable ✅)
        // ─────────────────────────────────────────────────────────────
        private static IQueryable<Service> ApplyLocationSqlFilter(PaginatedRequest request, IQueryable<Service> query)
        {
            if (request?.LocationFilter == null) return query;

            var hasCityOrRegion = !string.IsNullOrEmpty(request.LocationFilter.City) ||
                                  !string.IsNullOrEmpty(request.LocationFilter.Region);

            var hasCoords = request.LocationFilter.Latitude != 0 &&
                            request.LocationFilter.Longitude != 0;

            if (!hasCityOrRegion && !hasCoords) return query;

            if (hasCoords)
            {
                const double kmPerDegree = 111.0;
                double radiusKm = request.LocationFilter.RadiusKm;

                double lat = (double)request.LocationFilter.Latitude;
                double lon = (double)request.LocationFilter.Longitude;

                double latDelta = radiusKm / kmPerDegree;
                double lonDelta = radiusKm / (kmPerDegree * Math.Cos(lat * Math.PI / 180.0));

                decimal minLat = (decimal)(lat - latDelta), maxLat = (decimal)(lat + latDelta);
                decimal minLon = (decimal)(lon - lonDelta), maxLon = (decimal)(lon + lonDelta);

                return query
                    .Include(x => x.Vendor.ServiceAreas)
                    .Where(v => v.Vendor.ServiceAreas.Any(sa =>
                        (hasCityOrRegion && (
                            sa.City.ToLower() == request.LocationFilter.City.ToLower() ||
                            sa.Region.ToLower() == request.LocationFilter.Region.ToLower()
                        )) ||
                        (
                            sa.Latitude >= minLat && sa.Latitude <= maxLat &&
                            sa.Longitude >= minLon && sa.Longitude <= maxLon
                        )
                    ));
            }

            // City/Region only
            return query
                .Include(x => x.Vendor.ServiceAreas)
                .Where(v => v.Vendor.ServiceAreas.Any(sa =>
                    sa.City.ToLower() == request.LocationFilter.City.ToLower() ||
                    sa.Region.ToLower() == request.LocationFilter.Region.ToLower()
                ));
        }

        // ─────────────────────────────────────────────────────────────
        // Phase 2 — Haversine in-memory on already-fetched list ✅
        // ─────────────────────────────────────────────────────────────
        private static List<Service> ApplyHaversineFilter(PaginatedRequest request, List<Service> items)
        {
            if (request?.LocationFilter == null) return items;

            var hasCoords = request.LocationFilter.Latitude != 0 &&
                            request.LocationFilter.Longitude != 0;

            if (!hasCoords) return items;

            double userLat = (double)request.LocationFilter.Latitude;
            double userLon = (double)request.LocationFilter.Longitude;
            double radiusKm = request.LocationFilter.RadiusKm;

            return items.Where(v => v.Vendor.ServiceAreas.Any(sa =>
                (!string.IsNullOrEmpty(sa.City) && sa.City.ToLower() == request.LocationFilter.City?.ToLower()) ||
                (!string.IsNullOrEmpty(sa.Region) && sa.Region.ToLower() == request.LocationFilter.Region?.ToLower()) ||
                HaversineDistance((double)sa.Latitude, (double)sa.Longitude, userLat, userLon) <= radiusKm
            )).ToList();
        }

        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1 * Math.PI / 180.0) *
                       Math.Cos(lat2 * Math.PI / 180.0) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
        public async Task<PaginatedResponse<Service>> GetByEventTypeIdAsync(Guid eventTypeId, PaginatedRequest request, Expression<Func<Service, bool>> visibilityFilter, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var query = _context.Services

                    .Include(p => p.Vendor)
                    .Include(p => p.ServiceType)
                    .Include(p => p.ServiceImages)
                    .Where(p => p.EventTypes.Any(x => x.Id == eventTypeId))
                    .Where(visibilityFilter)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query = query.Where(p => p.Name.Contains(request.SearchTerm) || p.Description.Contains(request.SearchTerm));

                query = ApplyLocationSqlFilter(request, query);

                var totalCount = await query.CountAsync(token);
                var items = await query
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(token);

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
            return await _pipeline.ExecuteAsync(async token => {
                var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == serviceId, token);
                return await _context.Orders
                    .AnyAsync(o => o.UserId == userId && o.Event.EventItems.Any(oi => oi.ServiceName == service.Name), token);
            }, cancellationToken);
        }

        public async Task<List<Service>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken)
        {
            return await _pipeline.ExecuteAsync(async token =>
                await _context.Services
                    .Include(p => p.Vendor)
                    .Include(p => p.ServiceType)
                    .Include(p => p.ServiceImages)
                    .Where(s => ids.Contains(s.Id))
                    .ToListAsync(token),
                cancellationToken);
        }
    }
}
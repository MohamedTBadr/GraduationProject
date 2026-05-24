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
     ApplicationDbContext dbContext) : IVendorRepository
    {
        // Resolve the specific pipeline by name using the provider
        public async Task<PaginatedResponse<Vendor>> GetVendorsAsync(
      PaginatedRequest request,
      Expression<Func<Vendor, bool>> visibilityFilter,
      CancellationToken cancellationToken)
        {
            var query = dbContext.Vendors
                .Where(visibilityFilter)
                .AsNoTracking();

            // Search filter
            if (!string.IsNullOrWhiteSpace(request?.SearchTerm))
            {
                var search = request.SearchTerm.Trim().ToLower();
                query = query.Where(v =>
                    (v.BusinessName ?? "").ToLower().Contains(search) ||
                    (v.Description ?? "").ToLower().Contains(search) ||
                    (v.User != null && (v.User.FirstName ?? "").ToLower().Contains(search)) ||
                    (v.User != null && (v.User.LastName ?? "").ToLower().Contains(search)));
            }

            // ✅ Phase 1 — SQL only, stays IQueryable
            query = ApplyLocationSqlFilter(request, query);

            // Ordering
            query = query.OrderByDescending(v => v.Services
                .SelectMany(s => s.ServiceRatings)
                .Average(r => (decimal?)r.Rating) ?? 0);

            // ✅ CountAsync on EF IQueryable — no ToList yet
            var totalRecords = await query.CountAsync(cancellationToken);

            // ✅ Single DB round-trip with all includes
            var items = await query
                .Include(x => x.User)
                .Include(x => x.ServiceAreas)          // 👈 needed for Haversine phase
                .Include(x => x.Services)
                    .ThenInclude(s => s.ServiceRatings)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // ✅ Phase 2 — Haversine in-memory on paged list only
            var filtered = ApplyHaversineFilter(request, items);

            return new PaginatedResponse<Vendor>(filtered, totalRecords, request.PageIndex, request.PageSize);
        }

        // ─────────────────────────────────────────────────────────────
        // Phase 1 — SQL bounding box (pure IQueryable ✅)
        // ─────────────────────────────────────────────────────────────
        private static IQueryable<Vendor> ApplyLocationSqlFilter(PaginatedRequest request, IQueryable<Vendor> query)
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
                    .Include(x => x.ServiceAreas)
                    .Where(v => v.ServiceAreas.Any(sa =>
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
                .Include(x => x.ServiceAreas)
                .Where(v => v.ServiceAreas.Any(sa =>
                    sa.City.ToLower() == request.LocationFilter.City.ToLower() ||
                    sa.Region.ToLower() == request.LocationFilter.Region.ToLower()
                ));
        }

        // ─────────────────────────────────────────────────────────────
        // Phase 2 — Haversine in-memory on already-fetched list ✅
        // ─────────────────────────────────────────────────────────────
        private static List<Vendor> ApplyHaversineFilter(PaginatedRequest request, List<Vendor> items)
        {
            if (request?.LocationFilter == null) return items;

            var hasCoords = request.LocationFilter.Latitude != 0 &&
                            request.LocationFilter.Longitude != 0;

            if (!hasCoords) return items;

            double userLat = (double)request.LocationFilter.Latitude;
            double userLon = (double)request.LocationFilter.Longitude;
            double radiusKm = request.LocationFilter.RadiusKm;

            return items.Where(v => v.ServiceAreas.Any(sa =>
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
        public async Task<Vendor?> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var vendor =  await dbContext.Vendors
                .Include(x => x.Services)
                    .ThenInclude(s => s.ServiceRatings)
                        .ThenInclude(r => r.User)
               
                .Include(x => x.Services)
                    .ThenInclude(s => s.ServiceType)
                .Include(x => x.Services)
                    .ThenInclude(s => s.ServiceImages)
                .Include(x => x.Packages)
                .FirstOrDefaultAsync(v => v.UserId == id,cancellationToken);
            return vendor;
        }

        public async Task AddVendorAsync(Vendor vendor, CancellationToken cancellationToken)
        {
            // Tell EF the User already exists in DB, don't try to insert it again
            if (vendor.User != null)
                dbContext.Entry(vendor.User).State = EntityState.Unchanged;

            
                await dbContext.Vendors.AddAsync(vendor, cancellationToken);
                await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateVendorAsync(Vendor vendor, CancellationToken cancellationToken)
        {
            dbContext.Vendors.Update(vendor);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddServiceRatingAsync(ServiceRating rating, CancellationToken cancellationToken)
        {
            await dbContext.ServiceRatings.AddAsync(rating, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteVendorAsync(Vendor vendor, CancellationToken cancellationToken)
        {
            dbContext.Vendors.Remove(vendor);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<List<ServiceType>> GetServiceTypesByIdsAsync(List<Guid> ids, CancellationToken cancellationToken)
        {

            return dbContext.ServiceTypes.Where(st => ids.Contains(st.Id)).ToListAsync(cancellationToken);
        }
        public async Task<List<Vendor>> GetByIdsAsync(List<Guid> ids, CancellationToken cancellationToken)
        {
            return 
                await dbContext.Vendors
                    .Include(x => x.User)
                    .Include(x => x.VendorType)
                    .Include(x => x.Services)
                        .ThenInclude(s => s.ServiceRatings)
                    .Where(v => ids.Contains(v.UserId))
                    .ToListAsync(cancellationToken);
        }
    }
}

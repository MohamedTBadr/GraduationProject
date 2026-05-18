using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;

namespace Infrastructure.Search
{
    public class LuceneSyncJob
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ISearchService _searchService;
        private readonly ILogger<LuceneSyncJob> _logger;

        public LuceneSyncJob(ApplicationDbContext dbContext, ISearchService searchService, ILogger<LuceneSyncJob> logger)
        {
            _dbContext = dbContext;
            _searchService = searchService;
            _logger = logger;
        }

        public async Task SyncIndexAsync()
        {
            _logger.LogInformation("Starting Lucene index synchronization...");

            // 1. Clear current index
            await _searchService.ClearIndexAsync();

            int batchSize = 1000;

            // 2. Sync Vendors
            int vendorSkip = 0;
            while (true)
            {
                var vendorsBatch = await _dbContext.Vendors
                    .Include(v => v.VendorType)
                    .AsNoTracking()
                    .Skip(vendorSkip)
                    .Take(batchSize)
                    .ToListAsync();

                if (!vendorsBatch.Any())
                    break;

                await _searchService.IndexVendorsBatchAsync(vendorsBatch);
                vendorSkip += batchSize;
                _logger.LogInformation($"Synced {vendorSkip} vendors to Lucene.");
            }

            // 3. Sync Services
            int serviceSkip = 0;
            while (true)
            {
                var servicesBatch = await _dbContext.Services
                    .Include(s => s.Vendor)
                    .Include(s => s.ServiceType)
                    .AsNoTracking()
                    .Skip(serviceSkip)
                    .Take(batchSize)
                    .ToListAsync();

                if (!servicesBatch.Any())
                    break;

                await _searchService.IndexServicesBatchAsync(servicesBatch);
                serviceSkip += batchSize;
                _logger.LogInformation($"Synced {serviceSkip} services to Lucene.");
            }

            // 4. Sync User Profiles (Booking History)
            int userSkip = 0;
            while (true)
            {
                var usersBatch = await _dbContext.Users
                    .Include(u => u.Orders)
                        .ThenInclude(o => o.Event)
                            .ThenInclude(e => e.EventItems)
                    .AsNoTracking()
                    .Skip(userSkip)
                    .Take(batchSize)
                    .ToListAsync();

                if (!usersBatch.Any())
                    break;

                var userProfiles = usersBatch.Select(u => {
                    var allItems = u.Orders.SelectMany(o => o.Event?.EventItems ?? Enumerable.Empty<EventItem>()).ToList();
                    
                    var bookedVendorIds = string.Join(" ", allItems.Select(ei => ei.Service.VendorId.ToString()).Distinct());
                    var bookedCategories = string.Join(" ", allItems.Select(ei => ei.Service.Name?.Replace(" ", "")).Distinct());

                    return (u.Id, bookedVendorIds, bookedCategories);
                }).ToList();

                await _searchService.IndexUserProfilesBatchAsync(userProfiles);
                userSkip += batchSize;
                _logger.LogInformation($"Synced {userSkip} user profiles to Lucene.");
            }

            _logger.LogInformation("Lucene index synchronization completed successfully.");
        }
    }
}

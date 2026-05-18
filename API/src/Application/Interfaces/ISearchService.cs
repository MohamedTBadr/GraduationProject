using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISearchService
    {
        Task IndexVendorAsync(Vendor vendor);
        Task IndexServiceAsync(Service service);
        Task RemoveVendorAsync(Guid userId);
        Task RemoveServiceAsync(Guid serviceId);
        Task RebuildIndexAsync();
        Task ClearIndexAsync();
        Task IndexVendorsBatchAsync(IEnumerable<Vendor> vendors);
        Task IndexServicesBatchAsync(IEnumerable<Service> services);

        Task<IEnumerable<Guid>> SearchVendorsAsync(string query, string? category = null, string? location = null, bool includeUnverified = false);
        Task<IEnumerable<Guid>> SearchServicesAsync(string query, Guid? serviceTypeId = null, decimal? minPrice = null, decimal? maxPrice = null);

        Task IndexUserProfilesBatchAsync(IEnumerable<(Guid UserId, string BookedVendorIds, string BookedCategories)> userProfiles);
        Task<IEnumerable<Guid>> SearchSimilarUsersAsync(string vendorIds, string categories, int limit = 10);
    }
}

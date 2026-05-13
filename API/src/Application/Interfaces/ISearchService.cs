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

        Task<IEnumerable<Guid>> SearchVendorsAsync(string query, string? category = null, string? location = null, bool includeUnverified = false);
        Task<IEnumerable<Guid>> SearchServicesAsync(string query, Guid? serviceTypeId = null, decimal? minPrice = null, decimal? maxPrice = null);
    }
}

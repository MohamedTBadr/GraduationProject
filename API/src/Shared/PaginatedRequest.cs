using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public record PaginatedRequest
    {
        // 📍 Location filters (flattened for query string)
        public string? City { get; init; }
        public string? Region { get; init; }
        public decimal? Latitude { get; init; }
        public decimal? Longitude { get; init; }
        public double RadiusKm { get; init; } = 50;

        // 📄 Pagination
        public int PageIndex { get; init; } = 1;
        public int PageSize { get; init; } = 10;

        // 🔍 Search & Sort
        public string? SearchTerm { get; init; }
        public string? SortBy { get; init; }
        public bool IsDescending { get; init; } = false;
        public bool IncludeHidden { get; init; } = false;

        // 🎯 Advanced Filters
       
        public Guid? ServiceTypeId { get; init; }
        public Guid? VendorId { get; init; }
        public Guid? VendorTypeId { get; init; }
        public decimal? MinPrice { get; init; }
        public decimal? MaxPrice { get; init; }

        // 🔄 Map to LocationFilter for the helper
        public LocationFilter? LocationFilter =>
            City != null || Region != null || (Latitude.HasValue && Longitude.HasValue)
                ? new LocationFilter(
                    City,
                    Region,
                    Latitude ?? 0,
                    Longitude ?? 0,
                    RadiusKm)
                : null;
    }

    public record LocationFilter(
        string? City,
        string? Region,
        decimal Latitude,
        decimal Longitude,
        double RadiusKm = 50
    );
    // ✅ Change to class or non-positional record
    public record AIRequest
    {
        public decimal Budget { get; init; }
        public int GuestCount { get; init; }
        public string EventTypeName { get; init; }
    }
}

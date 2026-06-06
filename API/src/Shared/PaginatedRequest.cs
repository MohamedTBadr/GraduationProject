using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public record PaginatedRequest
    {
        // 📍 Location filters (flattened for query string)
        public string? City { get; set; }
        public string? Region { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public double RadiusKm { get; set; } = 50;

        // 📄 Pagination
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // 🔍 Search & Sort
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = false;
        public bool IncludeHidden { get; set; } = false;

        // 🎯 Advanced Filters
       
        public Guid? ServiceTypeId { get; set; }
        public Guid? VendorId { get; set; }
        public Guid? VendorTypeId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

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
        public decimal Budget { get; set; }
        public int GuestCount { get; set; }
        public string EventTypeName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Shared
{
    public record PaginatedRequest(int PageIndex = 1,
        int PageSize = 10,
        string? SearchTerm = null,
        string? SortBy = null,
        bool IsDescending = false
    );

    // ✅ Change to class or non-positional record
    public record AIRequest
    {
        public decimal Budget { get; init; }
        public int GuestCount { get; init; }
        public string ServiceTypeName { get; init; }
    }
}

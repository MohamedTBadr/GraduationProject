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
}

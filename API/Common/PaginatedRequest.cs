using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public record PaginatedRequest(int PageNumber = 1,
        int PageSize = 10,
        string? SearchTerm = null,
        string? SortBy = null,
        bool IsDescending = false
    );
}

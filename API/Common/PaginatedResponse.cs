using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public record PaginatedResponse<T>(IEnumerable<T> Items, int TotalCount, int PageNumber, int PageSize);
}

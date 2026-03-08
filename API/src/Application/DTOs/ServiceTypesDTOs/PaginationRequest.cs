using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ServiceTypesDTOs
{
    public class PaginationRequest
    {
                public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public string SearchTerm { get; set; }
        = string.Empty;

    }
}

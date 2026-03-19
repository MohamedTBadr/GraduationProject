using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.VendorDTOs
{
    public  record VendorRatingDTO(
        Guid Id,
        string VendorName,
        string UserName,
        int Rating,
        string Review,
        DateTime CreatedAt
    );
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.VendorDTOs
{
    public record RatingVendorRequest(
        
        Guid UserId,
        int RatingValue,
        string? Comment
    );
}

using Application.DTOs;
using Application.DTOs.VendorDTOs;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IVendorTypeService
    {
        Task<Result<VendorTypeDetailsDTO>> AddVendorTypeAsync(CreateOrUpdateVendorTypeRequest request, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteVendorTypeAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<VendorTypeDetailsDTO>> GetVendorTypeByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<IReadOnlyList<VendorTypeDetailsDTO>>> GetVendorTypesAsync(CancellationToken cancellationToken);
        Task<Result<VendorTypeDetailsDTO>> UpdateVendorTypeAsync(Guid id, CreateOrUpdateVendorTypeRequest request, CancellationToken cancellationToken);
     

    }
}

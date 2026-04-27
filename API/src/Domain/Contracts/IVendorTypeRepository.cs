using Domain.Entities;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface IVendorTypeRepository
    {
        Task AddVendorTypeAsync(VendorType vendorType, CancellationToken cancellationToken);
        Task DeleteVendorTypeAsync(Guid id, CancellationToken cancellationToken);
        Task<VendorType?> GetVendorTypeByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<IReadOnlyList<VendorType>> GetVendorTypesAsync(CancellationToken cancellationToken);
            Task UpdateVendorTypeAsync(VendorType vendorType, CancellationToken cancellationToken);
    }
}

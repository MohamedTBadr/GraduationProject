using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class VendorTypeRepository(ApplicationDbContext dbContext) : IVendorTypeRepository
    {


        public async Task AddVendorTypeAsync(VendorType vendorType, CancellationToken cancellationToken)
        {
            
                dbContext.VendorTypes.Add(vendorType);
                await dbContext.SaveChangesAsync(cancellationToken);
             
        }

        public async Task DeleteVendorTypeAsync(Guid id, CancellationToken cancellationToken)
        {
            
                var vendorType = await dbContext.VendorTypes.FindAsync([id], cancellationToken);
                if (vendorType != null)
                {
                    dbContext.VendorTypes.Remove(vendorType);
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
        }

        public async Task<VendorType?> GetVendorTypeByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            
                return await dbContext.VendorTypes.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IReadOnlyList<VendorType>> GetVendorTypesAsync(CancellationToken cancellationToken)
        {
            
                return await dbContext.VendorTypes
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);
        }

        public async Task UpdateVendorTypeAsync(VendorType vendorType, CancellationToken cancellationToken)
        {
           
                dbContext.VendorTypes.Update(vendorType);
                await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

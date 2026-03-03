using DAL.Context;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using DAL.Repositories.Contracts;
namespace DAL.Repositories
{
    public class VendorRepository(ApplicationDbContext dbContext) : IVendorRepository
    {
        public async Task<List<Vendor>> GetVendorsAsync()
        {
            var vendors = await dbContext.Vendors.AsNoTracking().ToListAsync();
            return vendors;
        }

        public async Task<Vendor?> GetVendorByIdAsync(Guid id)
        {
            var vendor = await dbContext.Vendors.FirstOrDefaultAsync(v => v.UserId == id);
            return vendor;
        }

        public async Task AddVendorAsync(Vendor vendor)
        {
            await dbContext.Vendors.AddAsync(vendor);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateVendorAsync(Vendor vendor)
        {
            dbContext.Vendors.Update(vendor);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteVendorAsync(Vendor vendor)
        {
            dbContext.Vendors.Remove(vendor);
            await dbContext.SaveChangesAsync();
        }

    }
}

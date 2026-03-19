using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Repositories
{
    public class VendorRepository(ApplicationDbContext dbContext) : IVendorRepository
    {
        public async Task<List<Vendor>> GetVendorsAsync()
        {
            var vendors = await dbContext.Vendors
                .Include(x=>x.User)
               .AsNoTracking().ToListAsync();
            return vendors;
        }

        public async Task<Vendor?> GetVendorByIdAsync(Guid id)
        {
            var vendor = await dbContext.Vendors.Include(v => v.VendorRatings).Include(x=>x.Packages).Include(x=>x.Products).FirstOrDefaultAsync(v => v.UserId == id);
            return vendor;
        }

        public async Task AddVendorAsync(Vendor vendor)
        {
            // Tell EF the User already exists in DB, don't try to insert it again
            if (vendor.User != null)
                dbContext.Entry(vendor.User).State = EntityState.Unchanged;

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

using Domain.Entities;

namespace Domain.Contracts { 
    public interface IVendorRepository
    {
        Task AddVendorAsync(Vendor vendor);
        Task DeleteVendorAsync(Vendor vendor);
        Task<Vendor?> GetVendorByIdAsync(Guid id);
        Task<List<Vendor>> GetVendorsAsync();
        Task UpdateVendorAsync(Vendor vendor);
    }
}
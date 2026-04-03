using Domain.Entities;

namespace Domain.Contracts { 
    public interface IVendorRepository
    {
        Task AddVendorAsync(Vendor vendor, CancellationToken cancellationToken);
        Task DeleteVendorAsync(Vendor vendor, CancellationToken cancellationToken);
        Task<Vendor?> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<Vendor>> GetVendorsAsync(CancellationToken cancellationToken);
        Task UpdateVendorAsync(Vendor vendor, CancellationToken cancellationToken);
    }
}
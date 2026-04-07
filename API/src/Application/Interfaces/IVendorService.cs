using Application.DTOs.VendorDTOs;
using Domain.Entities;
using Shared;
using System.Linq.Expressions;

namespace Application.Interfaces
{
    public interface IVendorService
    {
        Task<Result<VendorDetailsDTO>> AddVendorAsync(CreateVendorRequest request , CancellationToken cancellationToken);
        Task<Result<VendorDetailsDTO>> DeleteVendorAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<VendorDetailsDTO>> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<PaginatedResponse<VendorListDTO>>> GetVendorsAsync(PaginatedRequest paginatedRequest, bool isAdmin, CancellationToken cancellationToken);
        Task<Result<VendorDetailsDTO>> UpdateVendorAsync(Guid id, UpdateVendorRequest request, CancellationToken cancellationToken);
        Task<Result<VendorDetailsDTO>> ApproveVendorAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<VendorDetailsDTO>> RateVendorAsync(Guid id, RatingVendorRequest request, CancellationToken cancellationToken);
    }
}
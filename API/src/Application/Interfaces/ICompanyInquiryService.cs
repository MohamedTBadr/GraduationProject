using Application.DTOs.CompanyInquiryDTOs;
using Shared;

namespace Application.Interfaces
{
    public interface ICompanyInquiryService
    {
        Task AddAsync(CreateCompanyInquiryDto dto,CancellationToken ct);
        Task UpdateAsync(UpdateCompanyInquiryDto dto,CancellationToken ct);
        Task DeleteAsync(Guid id,CancellationToken ct);
        Task<CompanyInquiryDto> GetByIdAsync(Guid id, CancellationToken ct);
        Task<PaginatedResponse<CompanyInquiryDto>> GetAllAsync(PaginatedRequest request, CancellationToken ct   );
    }
}
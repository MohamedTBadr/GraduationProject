using Application.DTOs.CompanyInquiryDTOs;
using Shared;

namespace Application.Interfaces
{
    public interface ICompanyInquiryService
    {
        Task<Result<string>>     AddAsync(CreateCompanyInquiryDto dto,CancellationToken ct);
        Task<Result<string>> UpdateAsync(UpdateCompanyInquiryDto dto,CancellationToken ct);
        Task<Result<string>> DeleteAsync(Guid id,CancellationToken ct);
        Task<CompanyInquiryDto> GetByIdAsync(Guid id, CancellationToken ct);
        Task<PaginatedResponse<CompanyInquiryDto>> GetAllAsync(PaginatedRequest request, CancellationToken ct   );
    }
}
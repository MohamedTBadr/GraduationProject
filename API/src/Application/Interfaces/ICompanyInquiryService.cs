using Application.DTOs.CompanyInquiryDTOs;
using Shared;

namespace Application.Interfaces
{
    public interface ICompanyInquiryService
    {
        Task AddAsync(CreateCompanyInquiryDto dto);
        Task UpdateAsync(UpdateCompanyInquiryDto dto);
        Task DeleteAsync(Guid id);
        Task<CompanyInquiryDto> GetByIdAsync(Guid id);
        Task<PaginatedResponse<CompanyInquiryDto>> GetAllAsync(PaginatedRequest request);
    }
}
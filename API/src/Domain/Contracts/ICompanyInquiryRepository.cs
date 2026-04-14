using Domain.Entities;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface ICompanyInquiryRepository
    {
        Task AddCompanyInquiryAsync(CorporationInquiry inquiry);
        Task DeleteCompanyInquiryAsync(Guid Id);
        Task<CorporationInquiry> GetCompanyInquiryByIdAsync(Guid Id);

        Task<PaginatedResponse<CorporationInquiry>> GetAllCompanyInquiriesAsync(PaginatedRequest request);

        Task UpdateCompanyInquiryAsync(CorporationInquiry inquiry);
    }
}

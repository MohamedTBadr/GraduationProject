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
        Task AddCompanyInquiryAsync(CorporationInquiry inquiry,CancellationToken cancellationToken);
        Task DeleteCompanyInquiryAsync(Guid Id,CancellationToken cancellationToken);
        Task<CorporationInquiry> GetCompanyInquiryByIdAsync(Guid Id,CancellationToken cancellationToken);

        Task<PaginatedResponse<CorporationInquiry>> GetAllCompanyInquiriesAsync(PaginatedRequest request,CancellationToken cancellationToken);

        Task UpdateCompanyInquiryAsync(CorporationInquiry inquiry,CancellationToken cancellationToken);
    }
}

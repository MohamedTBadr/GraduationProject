using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;
using Shared;

namespace Infrastructure.Repositories
{
    public class CompanyInquiryRepository(ApplicationDbContext _context) : ICompanyInquiryRepository
    {

        public async Task AddCompanyInquiryAsync(CorporationInquiry inquiry,CancellationToken cancellationToken)
        {
          
                await _context.CorporationInquiries.AddAsync(inquiry, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            
        }

        public async Task DeleteCompanyInquiryAsync(Guid id,CancellationToken cancellationToken)
        {
            
                var inquiry = await _context.CorporationInquiries.FindAsync([id], cancellationToken: cancellationToken);

                if (inquiry is null)
                    throw new KeyNotFoundException($"Inquiry with ID {id} was not found.");

                _context.CorporationInquiries.Remove(inquiry);
                await _context.SaveChangesAsync(cancellationToken);
           
        }

        public async Task<CorporationInquiry> GetCompanyInquiryByIdAsync(Guid id,CancellationToken cancellationToken    )
        {
           
                var inquiry = await _context.CorporationInquiries
                    .Include(x => x.EventType)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

                if (inquiry is null)
                    throw new KeyNotFoundException($"Inquiry with ID {id} was not found.");

                return inquiry;
        }

        public async Task UpdateCompanyInquiryAsync(CorporationInquiry inquiry,CancellationToken cancellationToken)
        {
            
                var exists = await _context.CorporationInquiries.AnyAsync(x => x.Id == inquiry.Id, cancellationToken);

                if (!exists)
                    throw new KeyNotFoundException($"Inquiry with ID {inquiry.Id} was not found.");

                _context.CorporationInquiries.Update(inquiry);
                await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<PaginatedResponse<CorporationInquiry>> GetAllCompanyInquiriesAsync(PaginatedRequest request,CancellationToken cancellationToken)
        {
            
                var query = _context.CorporationInquiries
                    .Include(x => x.EventType)
                    .AsNoTracking();

                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                return new PaginatedResponse<CorporationInquiry>(
                    items,
                    totalCount,
                    request.PageIndex,
                    request.PageSize
                );
        }
    }
}
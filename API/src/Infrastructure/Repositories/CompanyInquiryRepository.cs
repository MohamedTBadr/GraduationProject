using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared;

namespace Infrastructure.Repositories
{
    public class CompanyInquiryRepository(ApplicationDbContext _context) : ICompanyInquiryRepository
    {

      

        public async Task AddCompanyInquiryAsync(CorporationInquiry inquiry)
        {
           

            await _context.CorporationInquiries.AddAsync(inquiry);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCompanyInquiryAsync(Guid id)
        {
            var inquiry = await _context.CorporationInquiries.FindAsync(id);

            if (inquiry is null)
                throw new KeyNotFoundException($"Inquiry with ID {id} was not found.");

            _context.CorporationInquiries.Remove(inquiry);
            await _context.SaveChangesAsync();
        }

        public async Task<CorporationInquiry> GetCompanyInquiryByIdAsync(Guid id)
        {
            var inquiry = await _context.CorporationInquiries.Include(x => x.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (inquiry is null)
                throw new KeyNotFoundException($"Inquiry with ID {id} was not found.");

            return inquiry;
        }

      

        public async Task UpdateCompanyInquiryAsync(CorporationInquiry inquiry)
        {
            var exists = await _context.CorporationInquiries.AnyAsync(x => x.Id == inquiry.Id);

            if (!exists)
                throw new KeyNotFoundException($"Inquiry with ID {inquiry.Id} was not found.");

            _context.CorporationInquiries.Update(inquiry);
            await _context.SaveChangesAsync();
        }

        public async Task<PaginatedResponse<CorporationInquiry>> GetAllCompanyInquiriesAsync(PaginatedRequest request)
        {
            var query = _context.CorporationInquiries.Include(x => x.Category)
                    .AsNoTracking()
                    ;

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PaginatedResponse<CorporationInquiry>(
      items,
      totalCount,
      request.PageIndex,
      request.PageSize
  );


        }
    }
}
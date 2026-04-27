using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Registry;
using Shared;

namespace Infrastructure.Repositories
{
    public class CompanyInquiryRepository(ApplicationDbContext _context, ResiliencePipelineProvider<string> pipelineProvider) : ICompanyInquiryRepository
    {
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");

        public async Task AddCompanyInquiryAsync(CorporationInquiry inquiry)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                await _context.CorporationInquiries.AddAsync(inquiry, token);
                await _context.SaveChangesAsync(token);
            });
        }

        public async Task DeleteCompanyInquiryAsync(Guid id)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                var inquiry = await _context.CorporationInquiries.FindAsync([id], cancellationToken: token);

                if (inquiry is null)
                    throw new KeyNotFoundException($"Inquiry with ID {id} was not found.");

                _context.CorporationInquiries.Remove(inquiry);
                await _context.SaveChangesAsync(token);
            });
        }

        public async Task<CorporationInquiry> GetCompanyInquiryByIdAsync(Guid id)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var inquiry = await _context.CorporationInquiries
                    .Include(x => x.EventType)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, token);

                if (inquiry is null)
                    throw new KeyNotFoundException($"Inquiry with ID {id} was not found.");

                return inquiry;
            });
        }

        public async Task UpdateCompanyInquiryAsync(CorporationInquiry inquiry)
        {
            await _pipeline.ExecuteAsync(async token =>
            {
                var exists = await _context.CorporationInquiries.AnyAsync(x => x.Id == inquiry.Id, token);

                if (!exists)
                    throw new KeyNotFoundException($"Inquiry with ID {inquiry.Id} was not found.");

                _context.CorporationInquiries.Update(inquiry);
                await _context.SaveChangesAsync(token);
            });
        }

        public async Task<PaginatedResponse<CorporationInquiry>> GetAllCompanyInquiriesAsync(PaginatedRequest request)
        {
            return await _pipeline.ExecuteAsync(async token =>
            {
                var query = _context.CorporationInquiries
                    .Include(x => x.EventType)
                    .AsNoTracking();

                var totalCount = await query.CountAsync(token);

                var items = await query
                    .Skip((request.PageIndex - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(token);

                return new PaginatedResponse<CorporationInquiry>(
                    items,
                    totalCount,
                    request.PageIndex,
                    request.PageSize
                );
            });
        }
    }
}
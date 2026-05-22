using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public sealed class ReportHistoryRepository : IReportHistoryRepository
    {
        private readonly ApplicationDbContext _db;

        public ReportHistoryRepository(ApplicationDbContext db) => _db = db;

        public async Task<ReportRecord> SaveAsync(ReportRecord record, CancellationToken ct = default)
        {
            _db.ReportRecords.Add(record);
            await _db.SaveChangesAsync(ct);
            return record;
        }

        public async Task<IReadOnlyList<ReportRecord>> GetByVendorAsync(
            Guid vendorId, CancellationToken ct = default) =>
            await _db.ReportRecords
                .AsNoTracking()
                .Where(r => r.VendorId == vendorId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<ReportRecord>> GetRecentAdminReportsAsync(
            int count, CancellationToken ct = default) =>
            await _db.ReportRecords
                .AsNoTracking()
                .Where(r => r.VendorId == null)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync(ct);
    }
}

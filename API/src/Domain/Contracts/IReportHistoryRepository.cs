using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Contracts
{
    public interface IReportHistoryRepository
    {
        Task<ReportRecord> SaveAsync(ReportRecord record, CancellationToken ct = default);
        Task<IReadOnlyList<ReportRecord>> GetByVendorAsync(Guid vendorId, CancellationToken ct = default);
        Task<IReadOnlyList<ReportRecord>> GetRecentAdminReportsAsync(int count, CancellationToken ct = default);
    }
}

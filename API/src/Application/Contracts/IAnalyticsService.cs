using Application.DTOs.Reports;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IAnalyticsService
    {
        Task<ExecutiveReportDto> BuildAdminReportAsync(CancellationToken ct = default);
        Task<ExecutiveReportDto> BuildVendorReportAsync(Guid vendorId, CancellationToken ct = default);
    }
}

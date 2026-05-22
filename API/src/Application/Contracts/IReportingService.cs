using Domain.Enums;
using Application.DTOs.Reports;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IReportingService
    {
        Task<ExecutiveReportDto> GenerateFullReportAsync(
            Guid? vendorId,
            ReportScope scope,
            CancellationToken ct = default);
    }
}

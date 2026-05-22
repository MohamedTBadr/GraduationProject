using Application.DTOs.Reports;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IPdfReportService
    {
        Task<byte[]> RenderAsync(ExecutiveReportDto report, CancellationToken ct = default);
    }
}

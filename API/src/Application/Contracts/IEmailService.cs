using Application.DTOs.Reports;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IEmailService
    {
        Task SendReportEmailAsync(
            string toEmail,
            string recipientName,
            ExecutiveReportDto report,
            byte[] pdfAttachment,
            CancellationToken ct = default);
    }
}

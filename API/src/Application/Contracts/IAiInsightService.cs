using Application.DTOs.Ai;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Contracts
{
    public interface IAiInsightService
    {
        Task<AiInsightResponseDto> GenerateInsightsAsync(
            AiInsightRequestDto request,
            CancellationToken ct = default);
    }
}

using System;
using System.Collections.Generic;

namespace Application.DTOs.Ai
{
    public sealed record AiInsightResponseDto
    {
        public string Summary { get; init; } = default!;
        public IReadOnlyList<string> Risks { get; init; } = [];
        public IReadOnlyList<string> Opportunities { get; init; } = [];
        public IReadOnlyList<string> Recommendations { get; init; } = [];
        public string Conclusion { get; init; } = default!;
        public string ModelUsed { get; init; } = default!;
        public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    }
}

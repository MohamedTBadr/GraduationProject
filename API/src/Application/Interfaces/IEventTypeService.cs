using Application.DTOs.EventTypesDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEventTypeService
    {
        Task<Result<IEnumerable<EventTypeResponseDto>>> GetAllAsync(CancellationToken ct);
        Task<Result<EventTypeResponseDto?>> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Result<EventTypeResponseDto>> CreateAsync(EventTypeCreateDto dto, CancellationToken ct);
        Task<Result<string>> UpdateAsync(EventTypeUpdateDto dto, CancellationToken ct);
        Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct);
    }
}

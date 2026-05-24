using Application.DTOs.EventTypesDTOs;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EventTypeService(IEventTypeRepository repository) : IEventTypeService
    {
        public async Task<Result<IEnumerable<EventTypeResponseDto>>> GetAllAsync(CancellationToken ct)
        {
            var types = await repository.GetAllAsync(ct);
            var dtos = types.Select(t => new EventTypeResponseDto(t.Id, t.Name));
            return Result<IEnumerable<EventTypeResponseDto>>.Success(dtos);
        }
        

        public async Task<Result<EventTypeResponseDto?>> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var type = await repository.GetByIdAsync(id, ct);
            if (type == null)
            {
                return Result<EventTypeResponseDto?>.NotFound(404, "Event type not found");
            }
            return Result<EventTypeResponseDto?>.Success(new EventTypeResponseDto(type.Id, type.Name));
        }

        public async Task<Result<EventTypeResponseDto>> CreateAsync(EventTypeCreateDto dto, CancellationToken ct)
        {
            var entity = new EventType { Id = Guid.NewGuid(), Name = dto.Name };
            await repository.CreateAsync(entity, ct);
            return Result<EventTypeResponseDto>.Success(new EventTypeResponseDto(entity.Id, entity.Name));
        }

        public async Task<Result<string>> UpdateAsync(EventTypeUpdateDto dto, CancellationToken ct)
        {
            var existing = await repository.GetByIdAsync(dto.Id, ct);
            if (existing != null)
            {
                existing.Name = dto.Name;
                await repository.UpdateAsync(existing, ct);
                return Result<string>.Success("Event type updated successfully");
            }
            return Result<string>.NotFound(404, "Event type not found");
        }

        public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct)
        {
            var entity = await repository.GetByIdAsync(id, ct);
            if (entity == null) return Result<bool>.NotFound(404, "Event type not found");

            await repository.DeleteAsync(entity, ct);
            return Result<bool>.Success(true);
        }
    }
}

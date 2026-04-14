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
        public async Task<IEnumerable<EventTypeResponseDto>> GetAllAsync(CancellationToken ct)
        {
            var types = await repository.GetAllAsync(ct);
            return types.Select(t => new EventTypeResponseDto(t.Id, t.Name));
        }

        public async Task<EventTypeResponseDto?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var type = await repository.GetByIdAsync(id, ct);
            return type == null ? null : new EventTypeResponseDto(type.Id, type.Name);
        }

        public async Task<EventTypeResponseDto> CreateAsync(EventTypeCreateDto dto, CancellationToken ct)
        {
            var entity = new EventType { Id = Guid.NewGuid(), Name = dto.Name };
            await repository.CreateAsync(entity, ct);
            return new EventTypeResponseDto(entity.Id, entity.Name);
        }

        public async Task UpdateAsync(EventTypeUpdateDto dto, CancellationToken ct)
        {
            var existing = await repository.GetByIdAsync(dto.Id, ct);
            if (existing != null)
            {
                existing.Name = dto.Name;
                await repository.UpdateAsync(existing, ct);
            }
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
        {
            var entity = await repository.GetByIdAsync(id, ct);
            if (entity == null) return false;

            await repository.DeleteAsync(entity, ct);
            return true;
        }
    }
}

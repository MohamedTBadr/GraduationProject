using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Entities;

namespace Application.Services
{
    public class EventItemService : IEventItemService
    {
        private readonly IEventItemRepository _itemRepo;
        private readonly IEventRepository     _eventRepo;

        public EventItemService(
            IEventItemRepository itemRepo,
            IEventRepository     eventRepo)
        {
            _itemRepo  = itemRepo;
            _eventRepo = eventRepo;
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<EventItemResponseDto> GetByIdAsync(Guid id)
        {
            var entity = await _itemRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"EventItem with id '{id}' was not found.");

            return MapToDto(entity);
        }

        public async Task<IEnumerable<EventItemResponseDto>> GetByEventIdAsync(Guid eventId)
        {
            if (!await _eventRepo.ExistsAsync(eventId))
                throw new KeyNotFoundException($"Event with id '{eventId}' was not found.");

            var entities = await _itemRepo.GetByEventIdAsync(eventId);
            return entities.Select(MapToDto);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<EventItemResponseDto> CreateAsync(CreateEventItemDto dto)
        {
            if (!await _eventRepo.ExistsAsync(dto.EventId))
                throw new KeyNotFoundException($"Event with id '{dto.EventId}' was not found.");

            var entity = MapFromCreateDto(dto);
            var created = await _itemRepo.CreateAsync(entity);
            
            return MapToDto(created);
        }

        public async Task<IEnumerable<EventItemResponseDto>> CreateRangeAsync(
            Guid eventId, IEnumerable<CreateEventItemDto> dtos)
        {
            if (!await _eventRepo.ExistsAsync(eventId))
                throw new KeyNotFoundException($"Event with id '{eventId}' was not found.");

            var entities = dtos.Select(d => { d.EventId = eventId; return MapFromCreateDto(d); });
            var created  = await _itemRepo.CreateRangeAsync(entities);
            return created.Select(MapToDto);
        }

        public async Task<EventItemResponseDto> UpdateAsync(Guid id, UpdateEventItemDto dto)
        {
            var entity = await _itemRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"EventItem with id '{id}' was not found.");

            entity.ProductImage = dto.ProductImage;
            entity.ProductName  = dto.ProductName;
            entity.Price        = dto.Price;
            entity.VendorName   = dto.VendorName;
            entity.Quantity     = dto.Quantity;

            var updated = await _itemRepo.UpdateAsync(entity);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _itemRepo.ExistsAsync(id))
                throw new KeyNotFoundException($"EventItem with id '{id}' was not found.");

            return await _itemRepo.DeleteAsync(id);
        }

        public async Task<bool> DeleteByEventIdAsync(Guid eventId)
        {
            if (!await _eventRepo.ExistsAsync(eventId))
                throw new KeyNotFoundException($"Event with id '{eventId}' was not found.");

            return await _itemRepo.DeleteByEventIdAsync(eventId);
        }

        // ── Mappers ───────────────────────────────────────────────

        private static EventItem MapFromCreateDto(CreateEventItemDto dto) => new()
        {
            EventId      = dto.EventId,
            ProductImage = dto.ProductImage,
            ProductName  = dto.ProductName,
            Price        = dto.Price,
            VendorName   = dto.VendorName,
            Quantity     = dto.Quantity
        };

        private static EventItemResponseDto MapToDto(EventItem i) => new()
        {
            Id           = i.Id,
            EventId      = i.EventId,
            ProductImage = i.ProductImage,
            ProductName  = i.ProductName,
            Price        = i.Price,
            VendorName   = i.VendorName,
            Quantity     = i.Quantity
        };
    }
}

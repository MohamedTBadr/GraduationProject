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
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepo;

        private static readonly HashSet<string> ValidStatuses =
            new() { "Planned", "Completed", "Cancelled" };

        public EventService(IEventRepository eventRepo)
        {
            _eventRepo = eventRepo;
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<EventResponseDto> GetByIdAsync(Guid id)
        {
            var entity = await _eventRepo.GetByIdWithItemsAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            return MapToResponseDto(entity);
        }

        public async Task<IEnumerable<EventSummaryDto>> GetAllAsync()
        {
            var entities = await _eventRepo.GetAllAsync();
            return entities.Select(MapToSummaryDto);
        }

        public async Task<IEnumerable<EventSummaryDto>> GetByUserIdAsync(Guid userId)
        {
            var entities = await _eventRepo.GetByUserIdAsync(userId);
            return entities.Select(MapToSummaryDto);
        }

        public async Task<IEnumerable<EventSummaryDto>> GetByUserIdAndStatusAsync(Guid userId, string status)
        {
            if (!ValidStatuses.Contains(status))
                throw new ArgumentException($"Invalid status '{status}'. Valid values: Planned, Completed, Cancelled.");

            var entities = await _eventRepo.GetByUserIdAsync(userId);
            return entities
                .Where(e => e.EventStatus == status)
                .Select(MapToSummaryDto);
        }

        public async Task<IEnumerable<EventSummaryDto>> GetByStatusAsync(string status)
        {
            if (!ValidStatuses.Contains(status))
                throw new ArgumentException($"Invalid status '{status}'. Valid values: Planned, Completed, Cancelled.");

            var entities = await _eventRepo.GetByStatusAsync(status);
            return entities.Select(MapToSummaryDto);
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<EventResponseDto> CreateAsync(CreateEventDto dto)
        {
            var entity = new Event
            {
                UserId = dto.UserId,
                Title = dto.Title,
                ServiceTypeId = dto.ServiceTypeId,
                EventDate = dto.EventDate,
                TotalBudget = dto.TotalBudget,
                GuestCount = dto.GuestCount,
                Notes = dto.Notes,
                EventStatus = "Planned",
                Location = dto.Location == null ? null : new Address
                {
                    Street = dto.Location.Street,
                    City = dto.Location.City,
                    State = dto.Location.State
                }
            };

            var created = await _eventRepo.CreateAsync(entity);
            return MapToResponseDto(created);
        }

        public async Task<EventResponseDto> UpdateAsync(Guid id, UpdateEventDto dto)
        {
            var entity = await _eventRepo.GetByIdWithItemsAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            if (!ValidStatuses.Contains(dto.EventStatus))
                throw new ArgumentException($"Invalid status '{dto.EventStatus}'.");

            entity.Title = dto.Title;
            entity.ServiceTypeId = dto.ServiceTypeId;
            entity.EventDate = dto.EventDate;
            entity.TotalBudget = dto.TotalBudget;
            entity.GuestCount = dto.GuestCount;
            entity.Notes = dto.Notes;
            entity.EventStatus = dto.EventStatus;

            if (dto.Location != null)
            {
                entity.Location ??= new Address();
                entity.Location.Street = dto.Location.Street;
                entity.Location.City = dto.Location.City;
                entity.Location.State = dto.Location.State;
            }

            var updated = await _eventRepo.UpdateAsync(entity);
            return MapToResponseDto(updated);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            if (!await _eventRepo.ExistsAsync(id))
                throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            return await _eventRepo.DeleteAsync(id);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, string status)
        {
            if (!ValidStatuses.Contains(status))
                throw new ArgumentException($"Invalid status '{status}'.");

            var entity = await _eventRepo.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Event with id '{id}' was not found.");

            entity.EventStatus = status;
            await _eventRepo.UpdateAsync(entity);
            return true;
        }

        // ── Mappers ───────────────────────────────────────────────

        private static EventResponseDto MapToResponseDto(Event e) => new()
        {
            Id = e.Id,
            UserId = e.UserId,
            UserName = e.User?.UserName,
            Title = e.Title,
            ServiceTypeName = e.ServiceType?.Name,
            EventDate = e.EventDate,
            TotalBudget = e.TotalBudget,
            GuestCount = e.GuestCount,
            Notes = e.Notes,
            EventStatus = e.EventStatus,
            Location = e.Location == null ? null : new AddressDto
            {
                Street = e.Location.Street,
                City = e.Location.City,
                State = e.Location.State
            },
            EventItems = e.EventItems?.Select(MapItemToDto).ToList() ?? new()
        };

        private static EventSummaryDto MapToSummaryDto(Event e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            EventDate = e.EventDate,
            EventStatus = e.EventStatus,
            TotalBudget = e.TotalBudget,
            ItemCount = e.EventItems?.Count ?? 0
        };

        private static EventItemResponseDto MapItemToDto(EventItem i) => new()
        {
            Id = i.Id,
            EventId = i.EventId,
            ProductImage = i.ProductImage,
            ProductName = i.ProductName,
            Price = i.Price,
            VendorName = i.VendorName,
            Quantity = i.Quantity
        };
    }
}
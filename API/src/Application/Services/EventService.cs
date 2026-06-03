using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Application.Services.Helpers;
using Domain.Contracts;
using Domain.Entities;

namespace Application.Services
{
    public class EventService : IEventService
    {
        private readonly IEventRepository _eventRepo;
        private readonly IEventTypeRepository _eventTypeRepo;
        private readonly IUserRepository _userRepo;
        private readonly IEmailSender _emailSender;
        private readonly NotificationService _notificationService;


        private static readonly HashSet<string> ValidStatuses =
            new() { "Planned", "Approved", "Completed", "Cancelled" };

        public EventService(IEventRepository eventRepo, IEventTypeRepository eventTypeRepo, NotificationService notificationService, IUserRepository userRepo, IEmailSender emailSender)
        {
            _eventRepo = eventRepo;
            _eventTypeRepo = eventTypeRepo;
            _notificationService = notificationService;
            _userRepo = userRepo;
            _emailSender = emailSender;
        }

        // ── Read ──────────────────────────────────────────────────

        public async Task<Result<EventResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _eventRepo.GetByIdWithItemsAsync(id, cancellationToken);
            if (entity == null)
                return Result<EventResponseDto>.NotFound(404,$"Event with id '{id}' was not found.");

            return Result<EventResponseDto>.Success(entity.ToResponseDto());
        }

        public async Task<Result<IEnumerable<EventSummaryDto>>> GetAllAsync(CancellationToken cancellationToken)
        {
            var entities = await _eventRepo.GetAllAsync(cancellationToken);
            return Result<IEnumerable<EventSummaryDto>>.Success(entities.Select(e => e.ToSummaryDto()));
        }

        public async Task<Result<IEnumerable<EventResponseDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var entities = await _eventRepo.GetByUserIdAsync(userId, cancellationToken);
            return Result<IEnumerable<EventResponseDto>>.Success(entities.Select(e => e.ToResponseDto()));
        }

        public async Task<Result<IEnumerable<EventResponseDto>>> GetByUserIdAndStatusAsync(Guid userId, string status, CancellationToken cancellationToken)
        {
            ValidateStatus(status);
            var entities = await _eventRepo.GetByUserIdAsync(userId, cancellationToken);
            return Result<IEnumerable<EventResponseDto>>.Success(entities.Where(e => e.EventStatus == status).Select(e => e.ToResponseDto()));
        }

        public async Task<Result<IEnumerable<EventResponseDto>>> GetByStatusAsync(string status, CancellationToken cancellationToken)
        {
            ValidateStatus(status);
            var entities = await _eventRepo.GetByStatusAsync(status, cancellationToken);
            return Result<IEnumerable<EventResponseDto>>.Success(entities.Select(e => e.ToResponseDto()));
        }

        // ── Write ─────────────────────────────────────────────────

        public async Task<Result<EventResponseDto>> CreateAsync(CreateEventDto dto, CancellationToken cancellationToken)
        {
            var types = await _eventTypeRepo.ExistsAsync(dto.EventTypeId, cancellationToken);
            if (!types)
                return Result<EventResponseDto>.NotFound(404,$"Event type with id '{dto.EventTypeId}' was not found."); 
            var created = await _eventRepo.CreateAsync(dto.ToEntity(), cancellationToken);
            return Result<EventResponseDto>.Success(created.ToResponseDto());
        }

        public async Task<Result<EventResponseDto>> UpdateAsync(Guid id, UpdateEventDto dto, CancellationToken cancellationToken)
        {
            var entity = await _eventRepo.GetByIdWithItemsAsync(id, cancellationToken);
            if (entity == null)
                return Result<EventResponseDto>.NotFound(404,$"Event with id '{id}' was not found.");

            ValidateStatus(dto.EventStatus);

            bool transitionedToCompleted = !entity.EventStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) && 
                                           dto.EventStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase);

            dto.ApplyTo(entity);

            var updated = await _eventRepo.UpdateAsync(entity, cancellationToken);
            if (dto.EventStatus == "Completed")
            {
                await _notificationService.SendAsync(updated.UserId, "EVENT_COMPLETED", "Event Completed", $"Your event '{updated.Title}' has been marked as completed.");
            }
            if (transitionedToCompleted)
            {
                var user = await _userRepo.GetByIdAsync(entity.UserId, cancellationToken);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailSender.SendCongratulatoryEmailAsync(user.Email, user.FirstName, entity.Title);
                }
            }

            return Result<EventResponseDto>.Success(updated.ToResponseDto());
        }

        public async Task<Result<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _eventRepo.ExistsAsync(id, cancellationToken))
                return Result<bool>.NotFound(404,$"Event with id '{id}' was not found.");

            var deleted = await _eventRepo.DeleteAsync(id, cancellationToken);
            return Result<bool>.Success(deleted);
        }

        public async Task<Result<bool>> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken)
        {
            ValidateStatus(status);

            var entity = await _eventRepo.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return Result<bool>.NotFound(404,$"Event with id '{id}' was not found.");

            bool transitionedToCompleted = !entity.EventStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase) && 
                                           status.Equals("Completed", StringComparison.OrdinalIgnoreCase);

            entity.EventStatus = status;
            await _eventRepo.UpdateAsync(entity, cancellationToken);
            await _notificationService.SendAsync(
                entity.UserId,
                nameof(NotificationType.EVENT_STATUS_UPDATED),  // type
                "Event Status Updated",                          // title
                $"Your event status has been updated to '{status}'.");

            if (transitionedToCompleted)
            {
                var user = await _userRepo.GetByIdAsync(entity.UserId, cancellationToken);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailSender.SendCongratulatoryEmailAsync(user.Email, user.FirstName, entity.Title);
                }
            }

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> CancelEventAsync(Guid id, CancelEventRequest request, CancellationToken cancellationToken)
        {
            var entity = await _eventRepo.GetByIdAsync(id, cancellationToken);
            if (entity == null)
                return Result<bool>.NotFound(404,$"Event with id '{id}' was not found.");

            if (entity.EventStatus == "Cancelled")
                return Result<bool>.InvalidOperation(400,"Event is already cancelled.");

            if (entity.EventStatus == "Completed")
                return Result<bool>.InvalidOperation(400,"A completed event cannot be cancelled.");

            request.ApplyTo(entity);
            await _eventRepo.UpdateAsync(entity, cancellationToken);
            return Result<bool>.Success(true);
        }

        public async Task<Result<EventItemResponseDto>> ApproveItemAsync(Guid eventId, Guid itemId, Guid vendorId, bool approve, string? reason, CancellationToken cancellationToken)
        {
            var item = await _eventRepo.GetItemByIdAsync(itemId, cancellationToken);

            if (item == null || item.EventId != eventId)
                return Result<EventItemResponseDto>.NotFound(404, "Item not found in this event.");

            if (item.Service.VendorId != vendorId)
                return Result<EventItemResponseDto>.Unauthorized(401, "You do not own this item.");

            if (item.ItemStatus != "Pending")
                return Result<EventItemResponseDto>.InvalidOperation(400, $"Item is already '{item.ItemStatus}'.");

            item.ItemStatus = approve ? "Approved" : "Rejected";
            item.RejectionReason = approve ? null : reason;

            var updated = await _eventRepo.UpdateItemAsync(item, cancellationToken);
            await SyncEventStatusAsync(eventId,cancellationToken);
            return Result<EventItemResponseDto>.Success(updated.ToResponseDto());
        }



        // EventService — add both methods
        public async Task<Result<EventItemResponseDto>> AddItemAsync(Guid eventId, CreateEventItemDto dto, CancellationToken cancellationToken)
        {
            var ev = await _eventRepo.GetByIdAsync(eventId, cancellationToken);
            if (ev == null)
                return Result<EventItemResponseDto>.NotFound(404, $"Event '{eventId}' not found.");

            if (ev.EventStatus is "Cancelled" or "Completed")
                return Result<EventItemResponseDto>.InvalidOperation(400, $"Cannot add items to a '{ev.EventStatus}' event.");
            if (dto.ServiceId == null && dto.PackageId == null)
                return Result<EventItemResponseDto>.InvalidOperation(400, "Either ServiceId or PackageId must be provided.");


            if(dto.ServiceId != null && dto.PackageId != null)
                return Result<EventItemResponseDto>.InvalidOperation(400, "Only one of ServiceId or PackageId can be provided.");

            var item = new EventItem
            {
                EventId = eventId,
                Quantity = dto.Quantity,
                ItemStatus = "Pending"
            };

            if (dto.ServiceId != null) item.ServiceId = dto.ServiceId;
            if (dto.PackageId != null) item.PackageId = dto.PackageId;
            

            var created = await _eventRepo.AddItemAsync(item, cancellationToken);
            return Result<EventItemResponseDto>.Success(created.ToResponseDto());
        }

        public async Task<Result<EventItemResponseDto>> UpdateItemAsync(Guid eventId, Guid itemId, UpdateEventItemDto dto, CancellationToken cancellationToken)
        {
            var item = await _eventRepo.GetItemByIdAsync(itemId, cancellationToken);

            if (item == null || item.EventId != eventId)
                return Result<EventItemResponseDto>.NotFound(404, "Item not found in this event.");

            if (item.ItemStatus is "Approved" or "Rejected")
                return Result<EventItemResponseDto>.InvalidOperation(400, $"Cannot edit an item that is already '{item.ItemStatus}'.");

           
            item.Quantity = dto.Quantity;
            item.ItemStatus = "Pending"; // reset to pending after edit

            var updated = await _eventRepo.UpdateItemAsync(item, cancellationToken);
            return Result<EventItemResponseDto>.Success(updated.ToResponseDto());
        }

        public async Task<Result<bool>> AddCollaboratorAsync(Guid eventId, string userEmailOrName, Domain.Enums.CollaboratorRole role, CancellationToken cancellationToken)
        {
            var ev = await _eventRepo.GetByIdAsync(eventId, cancellationToken);
            if (ev == null)
                return Result<bool>.NotFound(404, "Event not found.");

            var user = await _userRepo.GetByEmailAsync(userEmailOrName, cancellationToken) 
                       ?? await _userRepo.GetByNameAsync(userEmailOrName, cancellationToken);

            if (user == null)
            {
                if (IsEmail(userEmailOrName))
                {
                    await _emailSender.InviteCollaboratorAsync(userEmailOrName, ev.Title, role.ToString());
                    return Result<bool>.Success(true);
                }
                return Result<bool>.NotFound(404, "User not found.");
            }

            if (user.Id == ev.UserId)
                return Result<bool>.InvalidOperation(400, "User is already the owner of this event.");

            var existing = await _eventRepo.GetCollaboratorAsync(eventId, user.Id, cancellationToken);
            if (existing != null)
                return Result<bool>.InvalidOperation(400, "User is already a collaborator.");

            var collaborator = new EventCollaborator
            {
                Id = Guid.NewGuid(),
                EventId = eventId,
                UserId = user.Id,
                Role = role
            };

            await _eventRepo.AddCollaboratorAsync(collaborator, cancellationToken);
            
            await _notificationService.SendAsync(
                user.Id,
                "COLLABORATION_INVITE",
                "New Collaboration Invite",
                $"You have been invited to collaborate on event '{ev.Title}' as a {role}.");

            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> RemoveCollaboratorAsync(Guid eventId, Guid userId, CancellationToken cancellationToken)
        {
            var ev = await _eventRepo.GetByIdAsync(eventId, cancellationToken);
            if (ev == null)
                return Result<bool>.NotFound(404, "Event not found.");

            await _eventRepo.RemoveCollaboratorAsync(eventId, userId, cancellationToken);
            return Result<bool>.Success(true);
        }

        public async Task<Result<IEnumerable<EventCollaboratorDto>>> GetCollaboratorsAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var collabs = await _eventRepo.GetCollaboratorsAsync(eventId, cancellationToken);
            var dtos = collabs.Select(c => new EventCollaboratorDto
            {
                UserId = c.UserId,
                FullName = $"{c.User.FirstName} {c.User.LastName}",
                Email = c.User.Email,
                Role = c.Role,
                InvitedAt = c.InvitedAt
            });
            return Result<IEnumerable<EventCollaboratorDto>>.Success(dtos);
        }

        public async Task<bool> HasPermissionAsync(Guid eventId, Guid userId, bool requiresEdit, CancellationToken cancellationToken)
        {
            var ev = await _eventRepo.GetByIdAsync(eventId, cancellationToken);
            if (ev == null) return false;

            if (ev.UserId == userId) return true; // Owner

            var collaborator = await _eventRepo.GetCollaboratorAsync(eventId, userId, cancellationToken);
            if (collaborator == null) return false;

            if (requiresEdit)
            {
                return collaborator.Role == Domain.Enums.CollaboratorRole.Editor;
            }

            return true; // Viewer or Editor
        }
        // ── Helpers ───────────────────────────────────────────────

        private void ValidateStatus(string status)
        {
            if (!ValidStatuses.Contains(status))
                throw new ArgumentException($"Invalid status '{status}'. Valid values: Planned, Approved, Completed, Cancelled.");
        }

        private bool IsEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task SyncEventStatusAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var ev = await _eventRepo.GetByIdWithItemsAsync(eventId, cancellationToken);
            if (ev == null) return;

            if (ev.EventItems.All(i => i.ItemStatus == "Approved"))
                ev.EventStatus = "Approved";
            else if (ev.EventItems.Any(i => i.ItemStatus == "Rejected"))
                ev.EventStatus = "Cancelled";

            await _eventRepo.UpdateAsync(ev, cancellationToken);
        }

        
    }

    // ── Mappers ───────────────────────────────────────────────────

    internal static class EventMappers
    {
        // ── Event → DTOs ──────────────────────────────────────────

        internal static EventResponseDto ToResponseDto(this Event e) => new()
        {
            Id = e.Id,
            UserId = e.UserId,
            Title = e.Title,

            EventTypeName = e.EventType?.Name,

            EventDate = e.EventDate,
            TotalBudget = e.TotalBudget,
            GuestCount = e.GuestCount,
            Notes = e.Notes,
            EventStatus = e.EventStatus,
            CancellationReason = e.CancellationReason,
            AdditionalNotes = e.AdditionalNotes,
            CancelledAt = e.CancelledAt,

            Location = e.Location?.ToDto(),

            EventItems = e.EventItems?
                .Select(i => i.ToResponseDto())
                .ToList() ?? new(),

            Collaborators = e.Collaborators?
                .Select(c => c.ToCollaboratorDto())
                .ToList() ?? new()
        };


        internal static EventSummaryDto ToSummaryDto(this Event e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            EventDate = e.EventDate,
            EventStatus = e.EventStatus,
            TotalBudget = e.TotalBudget,
            ItemCount = e.EventItems?.Count ?? 0
        };


        internal static EventItemResponseDto ToResponseDto(this EventItem i)
        {
            var service = i.Service;
            var package = i.Package;
            var vendorId = service != null
            ? service.VendorId
            : package?.VendorId;

            return new()
            {
                Id = i.Id,
                EventId = i.EventId,

                ServiceImage = service != null
                    ? service.ServiceImages?.FirstOrDefault()?.ToString()
                    : null,

                ServiceName = service != null
                    ? service.Name
                    : package?.Name,

                Price = i.Price,

                VendorId =  vendorId ?? Guid.Empty,

                VendorName = service != null
            ? service.Vendor?.BusinessName
            : package?.Vendor?.BusinessName,

                Quantity = i.Quantity,

                ItemStatus = i.ItemStatus,

                RejectionReason = i.RejectionReason
            };
        }

        internal static AddressDto ToDto(this Address a) => new()
        {
            Street = a.Street,
            City = a.City,
            State = a.State
           
        };

        internal static EventCollaboratorDto ToCollaboratorDto(this EventCollaborator c) => new()
        {
            UserId = c.UserId,
            FullName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : "Unknown",
            Email = c.User?.Email ?? "Unknown",
            Role = c.Role,
            InvitedAt = c.InvitedAt
        };

        // ── DTOs → Entity ─────────────────────────────────────────

        internal static Event ToEntity(this CreateEventDto dto) => new()
        {
            UserId = (Guid)dto.UserId!,
            Title = dto.Title,
            EventTypeId = dto.EventTypeId,
            EventDate = dto.EventDate,
            TotalBudget = dto.TotalBudget,
            GuestCount = dto.GuestCount,
            Notes = dto.Notes,
            EventStatus = "Planned",
            Location = dto.Location?.ToEntity()
        };

        internal static void ApplyTo(this UpdateEventDto dto, Event entity)
        {
            entity.Title = dto.Title;
            entity.EventTypeId = dto.EventTypeId;
            entity.EventDate = dto.EventDate;
            entity.TotalBudget = dto.TotalBudget;
            entity.GuestCount = dto.GuestCount;
            entity.Notes = dto.Notes;
            entity.EventStatus = dto.EventStatus;

            if (dto.Location != null)
            {
                entity.Location ??= new Address();
                dto.Location.ApplyTo(entity.Location);
            }
        }

        internal static void ApplyTo(this CancelEventRequest request, Event entity)
        {
            entity.EventStatus = "Cancelled";
            entity.CancellationReason = request.Reason;
            entity.AdditionalNotes = request.AdditionalNotes;
            entity.CancelledAt = DateTime.UtcNow;
        }

        internal static Address ToEntity(this AddressDto dto) => new()
        {
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
        };

        internal static void ApplyTo(this AddressDto dto, Address address)
        {
            address.Street = dto.Street;
            address.City = dto.City;
            address.State = dto.State;
        }
    }
}
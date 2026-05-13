using Application;
using Application.DTOs.VendorDTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared;
using System.Linq.Expressions;
using Application.Services.Helpers;
using System.Text.Json;

namespace Application.Services
{
    public class VendorService(
        IUserRepository userRepository, 
        UserManager<ApplicationUser> userManager,
        IVendorRepository vendorRepository, 
        IEventItemRepository _eventItemRepository, 
        IMapper mapper, 
        IFileService _fileService, 
        LlamaService llamaService,
        ISearchService searchService) : IVendorService
    {
        public async Task<Result<PaginatedResponse<VendorListDTO>>> GetVendorsAsync(
            PaginatedRequest paginatedRequest,
            bool isAdmin,
            CancellationToken cancellationToken)
        {
            // If there's a search term or advanced filters, use Lucene
            if (!string.IsNullOrWhiteSpace(paginatedRequest.SearchTerm) || 
                !string.IsNullOrWhiteSpace(paginatedRequest.City))
            {
                var vendorIds = await searchService.SearchVendorsAsync(
                    paginatedRequest.SearchTerm ?? "", 
                    paginatedRequest.City, 
                    includeUnverified: isAdmin);
                var idList = vendorIds.ToList();
                
                // Fetch from DB to get full details and ensure status is correct
                var vendorsFromDb = await vendorRepository.GetByIdsAsync(idList, cancellationToken);
                
                // Maintain Lucene order or re-apply pagination if needed
                var items = mapper.Map<List<VendorListDTO>>(vendorsFromDb);
                
                return Result<PaginatedResponse<VendorListDTO>>.Success(new PaginatedResponse<VendorListDTO>(
                    items, items.Count, paginatedRequest.PageIndex, paginatedRequest.PageSize));
            }

            // Fallback to standard DB pagination
            Expression<Func<Vendor, bool>> visibilityFilter = isAdmin
                ? v => true
                : v => v.IsVerified;

            var vendors = await vendorRepository.GetVendorsAsync(
                paginatedRequest,
                visibilityFilter,
                cancellationToken);

            var mappedItems = mapper.Map<List<VendorListDTO>>(vendors.Items);
            var response = new PaginatedResponse<VendorListDTO>(
                mappedItems,
                vendors.TotalCount,
                vendors.PageNumber,
                vendors.PageSize);

            return Result<PaginatedResponse<VendorListDTO>>.Success(response);
        }

        public async Task<Result<VendorDetailsDTO>> GetVendorByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(id, cancellationToken);
            if (vendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound(404,"Vendor not found");
            }
            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public async Task<List<VendorBookingDto>> GetVendorBookingsAsync(
                   Guid vendorId,
                   CancellationToken cancellationToken = default)
        {
            var bookings = await _eventItemRepository.GetVendorBookingsAsync(vendorId, cancellationToken);

            return bookings.Select(ei => new VendorBookingDto
            {
                EventItemId = ei.Id,
                ServiceName = ei.ServiceName,
                Price = ei.Price,
                BookingStatus = ei.ItemStatus,
                Notes = ei.Event.Notes,

                EventId = ei.EventId,
                EventTitle = ei.Event.Title,
                EventType = ei.Event.EventType?.Name ?? string.Empty,
                EventDate = ei.Event.EventDate,
                EventStatus = ei.Event.EventStatus,
                GuestCount = ei.Event.GuestCount,
                Location = ei.Event.Location?.ToString() ?? string.Empty
            }).ToList();
        }

        public async Task<Result<VendorDetailsDTO>> AddVendorAsync(CreateVendorRequest request, CancellationToken cancellationToken)
        {
            var user = new ApplicationUser
            {
                Id = new Guid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.Name,
                Email = request.Email,
                PhoneNumber = request.Phone,
            };

            var identityResult = await userRepository.CreateAsync(user, request.Password, "Vendor", cancellationToken);
            if (!identityResult.Succeeded)
            {
                var errors = string.Join(", ", identityResult.Errors.Select(e => e.Description));
                return Result<VendorDetailsDTO>.Failure(new Error(ErrorType.AlreadyExists, 409, errors));
            }

            var profilePicture = await _fileService.Upload("Vendors", request.ProfilePicture, cancellationToken);
            var document = await _fileService.Upload("VendorDocuments", request.Document, cancellationToken);
            var vendor = new Vendor
            {
                UserId = user.Id,
                BusinessName = request.BusinessName,
                YearsInBusiness = request.YearsInBusiness,
                Description = request.Description,
                PortfolioLink = request.PortfolioLink,
                Address = request.Address,
                IsVerified = false,
                VendorTypeId = request.VendorTypeId,
                ProfilePicture = profilePicture,
                Document = document,
                ServiceAreas = request.ServiceAreas?.Select(sa => new ServiceArea
                {
                    City = sa.City,
                    Region = sa.Region,
                    Latitude = sa.Latitude,
                    Longitude = sa.Longitude
                }).ToList()
            };

            await vendorRepository.AddVendorAsync(vendor, cancellationToken);
            await userManager.AddToRoleAsync(user, "Vendor");

            // Index in Lucene
            await searchService.IndexVendorAsync(vendor);

            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public async Task<Result<VendorDetailsDTO>> UpdateVendorAsync(Guid id, UpdateVendorRequest request, CancellationToken cancellationToken)
        {
            var existingVendor = await vendorRepository.GetVendorByIdAsync(id, cancellationToken);
            if (existingVendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound(404, "Vendor not found");
            }
            var vendorMapped = mapper.Map(request, existingVendor);
            await vendorRepository.UpdateVendorAsync(vendorMapped, cancellationToken);

            // Update Lucene Index
            await searchService.IndexVendorAsync(vendorMapped);

            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendorMapped);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public async Task<Result<VendorDetailsDTO>> DeleteVendorAsync(Guid id, CancellationToken cancellationToken)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(id, cancellationToken);
            if (vendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound(404, "Vendor not found");
            }

            await vendorRepository.DeleteVendorAsync(vendor, cancellationToken);
            
            // Remove from Lucene
            await searchService.RemoveVendorAsync(id);

            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public async Task<Result<VendorDetailsDTO>> ApproveVendorAsync(Guid id, CancellationToken cancellationToken)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(id, cancellationToken);
            if (vendor == null)
            {
                return Result<VendorDetailsDTO>.NotFound(404, "Vendor not found");
            }
            vendor.IsVerified = true;
            await vendorRepository.UpdateVendorAsync(vendor, cancellationToken);

            // Update Lucene Index (verified status changed)
            await searchService.IndexVendorAsync(vendor);

            var vendorDTO = mapper.Map<VendorDetailsDTO>(vendor);
            return Result<VendorDetailsDTO>.Success(vendorDTO);
        }

        public Task<Result<VendorDetailsDTO>> RateVendorAsync(Guid id, RatingVendorRequest request, CancellationToken cancellationToken)
        { 
            return Task.FromResult(Result<VendorDetailsDTO>.Success(new VendorDetailsDTO()));
        }

        public async Task<Result<VendorVibeDTO>> GetVendorVibeAsync(Guid vendorId, CancellationToken cancellationToken)
        {
            var vendor = await vendorRepository.GetVendorByIdAsync(vendorId, cancellationToken);
            if (vendor == null)
                return Result<VendorVibeDTO>.NotFound(404, "Vendor not found");

            var reviews = vendor.Services
                .SelectMany(s => s.ServiceRatings)
                .Select(r => r.Review)
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            if (!reviews.Any())
            {
                return Result<VendorVibeDTO>.Success(new VendorVibeDTO
                {
                    VibeSummary = "This vendor doesn't have any reviews yet.",
                    KeyStrengths = new List<string> { "Fresh presence" },
                    OverallSentiment = "Neutral"
                });
            }

            var prompt = $@"
                Analyze the following customer reviews for vendor '{vendor.BusinessName}' and provide a 'Vendor Vibe' summary.
                Return ONLY a JSON object with this structure:
                {{
                    ""VibeSummary"": ""A concise 1-sentence summary of the vendor's vibe."",
                    ""KeyStrengths"": [""Strength 1"", ""Strength 2"", ""Strength 3""],
                    ""OverallSentiment"": ""Positive/Neutral/Negative""
                }}

                REVIEWS:
                {string.Join("\n- ", reviews)}
                ";

            var aiResult = await llamaService.SendMessageAsync(prompt, "You are a professional sentiment analyst. Return JSON only.");

            if (aiResult.IsFailure)
                return Result<VendorVibeDTO>.Failure(aiResult.Error);

            try
            {
                var vibe = JsonSerializer.Deserialize<VendorVibeDTO>(aiResult.Value, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Result<VendorVibeDTO>.Success(vibe!);
            }
            catch (Exception ex)
            {
                return Result<VendorVibeDTO>.Unexpected(5001, $"Failed to parse AI response: {ex.Message}");
            }
        }
        public async Task RebuildSearchIndexAsync()
        {
            await searchService.RebuildIndexAsync();
            var paginatedResult = await vendorRepository.GetVendorsAsync(
                new PaginatedRequest { PageSize = int.MaxValue }, 
                v => true, 
                CancellationToken.None);
            
            foreach (var vendor in paginatedResult.Items)
            {
                await searchService.IndexVendorAsync(vendor);
            }
        }
    }
}

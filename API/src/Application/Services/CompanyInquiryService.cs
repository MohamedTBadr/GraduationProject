using Application.DTOs.CompanyInquiryDTOs;
using Application.DTOs.EventTypesDTOs;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Entities;
using Shared;

namespace Application.Services
{
    public class CompanyInquiryService : ICompanyInquiryService
    {
        private readonly ICompanyInquiryRepository _repository;

        public CompanyInquiryService(ICompanyInquiryRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<string>> AddAsync(CreateCompanyInquiryDto dto, CancellationToken ct)
        {
            var entity = new CorporationInquiry
            {
                Id = Guid.NewGuid(),
                CompanyName = dto.CompanyName,
                ContactPerson = dto.ContactPerson,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                EventTypeId = dto.EventTypeId,
                ExpectedDate = dto.ExpectedDate,
                EstimatedAttendees = dto.EstimatedAttendees,
                ApproximateBudget = dto.ApproximateBudget,
                AdditionalRequirements = dto.AdditionalRequirements,
                Status = dto.Status
            };

            await _repository.AddCompanyInquiryAsync(entity, ct);
            return Result<string>.Success("Company inquiry submitted successfully");
        }

        public async Task<Result<string>> UpdateAsync(UpdateCompanyInquiryDto dto, CancellationToken ct)
        {
            var entity = new CorporationInquiry
            {
                Id = dto.Id,
                CompanyName = dto.CompanyName,
                ContactPerson = dto.ContactPerson,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                EventTypeId = dto.EventTypeId,
                ExpectedDate = dto.ExpectedDate,
                EstimatedAttendees = dto.EstimatedAttendees,
                ApproximateBudget = dto.ApproximateBudget,
                AdditionalRequirements = dto.AdditionalRequirements,
                Status = dto.Status
            };

            await _repository.UpdateCompanyInquiryAsync(entity, ct);
            return Result<string>.Success("Company inquiry updated successfully");
        }

        public async Task<Result<string>> DeleteAsync(Guid id, CancellationToken ct)
        {
            await _repository.DeleteCompanyInquiryAsync(id, ct);

            return Result<string>.Success("Company inquiry deleted successfully");
        }

        public async Task<CompanyInquiryDto> GetByIdAsync(Guid id, CancellationToken ct)
        {
            var entity = await _repository.GetCompanyInquiryByIdAsync(id, ct);
            var dto = MapToDto(entity);
            if(dto == null)
            {
                return Result<CompanyInquiryDto>.NotFound(404, "Company inquiry not found").Value;
            }
            return Result<CompanyInquiryDto>.Success(dto).Value;

        }

        public async Task<PaginatedResponse<CompanyInquiryDto>> GetAllAsync(PaginatedRequest request, CancellationToken ct)
        {
            var result = await _repository.GetAllCompanyInquiriesAsync(request, ct);

            var dtoItems = result.Items.Select(MapToDto);

            var response = new PaginatedResponse<CompanyInquiryDto>(
                dtoItems,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            );
            var resultPattern= Result<PaginatedResponse<CompanyInquiryDto>>.Success(response);

            return resultPattern.Value;
        }

        private static CompanyInquiryDto MapToDto(CorporationInquiry entity)
        {
            return new CompanyInquiryDto
            {
                Id = entity.Id,
                CompanyName = entity.CompanyName,
                ContactPerson = entity.ContactPerson,
                PhoneNumber = entity.PhoneNumber,
                Email = entity.Email,
                ExpectedDate = entity.ExpectedDate,
                EstimatedAttendees = entity.EstimatedAttendees,
                ApproximateBudget = entity.ApproximateBudget,
                AdditionalRequirements = entity.AdditionalRequirements,
                Status = entity.Status,
                EventType = entity.EventType == null ? null : new EventTypeResponseDto(
    entity.EventType.Id,
    entity.EventType.Name
)
            };
        }
    }
}
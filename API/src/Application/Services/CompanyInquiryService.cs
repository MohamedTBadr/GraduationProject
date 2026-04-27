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

        public async Task AddAsync(CreateCompanyInquiryDto dto)
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

            await _repository.AddCompanyInquiryAsync(entity);
        }

        public async Task UpdateAsync(UpdateCompanyInquiryDto dto)
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

            await _repository.UpdateCompanyInquiryAsync(entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteCompanyInquiryAsync(id);
        }

        public async Task<CompanyInquiryDto> GetByIdAsync(Guid id)
        {
            var entity = await _repository.GetCompanyInquiryByIdAsync(id);

            return MapToDto(entity);
        }

        public async Task<PaginatedResponse<CompanyInquiryDto>> GetAllAsync(PaginatedRequest request)
        {
            var result = await _repository.GetAllCompanyInquiriesAsync(request);

            var dtoItems = result.Items.Select(MapToDto);

            return new PaginatedResponse<CompanyInquiryDto>(
                dtoItems,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            );
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
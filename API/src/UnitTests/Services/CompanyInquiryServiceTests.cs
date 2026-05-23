using Application.DTOs.CompanyInquiryDTOs;
using Application.Services;
using Domain.Contracts;
using Domain.Entities;
using Moq;
using Shared;
using Xunit;

namespace Application.UnitTests.Services;

public class CompanyInquiryServiceTests
{
    private readonly Mock<ICompanyInquiryRepository> _repositoryMock = new();
    private readonly CompanyInquiryService _sut;

    public CompanyInquiryServiceTests()
    {
        _sut = new CompanyInquiryService(_repositoryMock.Object);
    }

    [Fact]
    public async Task AddAsync_MapsDtoAndPersists()
    {
        var dto = CreateDto();

        await _sut.AddAsync(dto, CancellationToken.None);

        _repositoryMock.Verify(
            x => x.AddCompanyInquiryAsync(
                It.Is<CorporationInquiry>(i =>
                    i.Id != Guid.Empty &&
                    i.CompanyName == dto.CompanyName &&
                    i.EventTypeId == dto.EventTypeId &&
                    i.Status == dto.Status),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_MapsDtoAndPersists()
    {
        var dto = new UpdateCompanyInquiryDto
        {
            Id = Guid.NewGuid(),
            CompanyName = "Updated Co",
            ContactPerson = "Mona",
            PhoneNumber = "0100",
            Email = "mona@test.com",
            EventTypeId = Guid.NewGuid(),
            ExpectedDate = DateTime.UtcNow.AddDays(10),
            EstimatedAttendees = 50,
            ApproximateBudget = 10000,
            AdditionalRequirements = "Stage",
            Status = "Approved"
        };

        await _sut.UpdateAsync(dto, CancellationToken.None);

        _repositoryMock.Verify(
            x => x.UpdateCompanyInquiryAsync(
                It.Is<CorporationInquiry>(i => i.Id == dto.Id && i.CompanyName == dto.CompanyName && i.Status == "Approved"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_MapsEntityWithEventType()
    {
        var id = Guid.NewGuid();
        _repositoryMock
            .Setup(x => x.GetCompanyInquiryByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CorporationInquiry
            {
                Id = id,
                CompanyName = "Acme",
                ContactPerson = "Ali",
                PhoneNumber = "0100",
                Email = "ali@test.com",
                EventType = new EventType { Id = Guid.NewGuid(), Name = "Conference" },
                ExpectedDate = DateTime.UtcNow.AddDays(5),
                EstimatedAttendees = 100,
                ApproximateBudget = 50000,
                AdditionalRequirements = "AV",
                Status = "Pending"
            });

        var result = await _sut.GetByIdAsync(id, CancellationToken.None);

        Assert.Equal("Acme", result.CompanyName);
        Assert.Equal("Conference", result.EventType.Name);
    }

    [Fact]
    public async Task GetAllAsync_MapsPaginatedResponse()
    {
        var request = new PaginatedRequest { PageIndex = 2, PageSize = 5 };
        _repositoryMock
            .Setup(x => x.GetAllCompanyInquiriesAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResponse<CorporationInquiry>(
                [new CorporationInquiry { Id = Guid.NewGuid(), CompanyName = "Acme", ContactPerson = "Ali", PhoneNumber = "0100", Email = "ali@test.com", AdditionalRequirements = "AV" }],
                12,
                2,
                5));

        var result = await _sut.GetAllAsync(request, CancellationToken.None);

        Assert.Equal(12, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToRepository()
    {
        var id = Guid.NewGuid();

        await _sut.DeleteAsync(id, CancellationToken.None);

        _repositoryMock.Verify(x => x.DeleteCompanyInquiryAsync(id, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateCompanyInquiryDto CreateDto() => new()
    {
        CompanyName = "Acme",
        ContactPerson = "Ali",
        PhoneNumber = "0100",
        Email = "ali@test.com",
        EventTypeId = Guid.NewGuid(),
        ExpectedDate = DateTime.UtcNow.AddDays(20),
        EstimatedAttendees = 100,
        ApproximateBudget = 50000,
        AdditionalRequirements = "AV",
        Status = "Pending"
    };
}

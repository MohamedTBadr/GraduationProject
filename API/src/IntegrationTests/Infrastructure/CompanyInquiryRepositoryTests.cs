using Domain.Entities;
using EpicHub.IntegrationTests.Infrastructure.Shared;
using Infrastructure.Repositories;
using Shared;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure.Tests;

public class CompanyInquiryRepositoryTests
{
    private static async Task<Guid> SeedEventTypeAsync(TestDatabase db, string name = "Conference")
    {
        var eventType = EntityBuilders.BuildEventType(name);
        db.Context.EventTypes.Add(eventType);
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
        return eventType.Id;
    }

    private static async Task SeedInquiriesAsync(TestDatabase db, Guid eventTypeId, int count)
    {
        for (var i = 1; i <= count; i++)
            db.Context.CorporationInquiries.Add(
                EntityBuilders.BuildInquiry(eventTypeId, companyName: $"Company {i}"));
        await db.Context.SaveChangesAsync();
        db.Context.ChangeTracker.Clear();
    }

    // ── Pagination ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllCompanyInquiriesAsync_ReturnsCorrectTotalCount()
    {
        await using var db    = await TestDatabase.CreateAsync();
        var eventTypeId       = await SeedEventTypeAsync(db);
        await SeedInquiriesAsync(db, eventTypeId, count: 3);
        var repo              = new CompanyInquiryRepository(db.Context);

        var page = await repo.GetAllCompanyInquiriesAsync(
            new PaginatedRequest { PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Equal(3, page.TotalCount);
    }

    [Fact]
    public async Task GetAllCompanyInquiriesAsync_ReturnsCorrectPageSlice()
    {
        await using var db    = await TestDatabase.CreateAsync();
        var eventTypeId       = await SeedEventTypeAsync(db);
        await SeedInquiriesAsync(db, eventTypeId, count: 3);
        var repo              = new CompanyInquiryRepository(db.Context);

        // Page 2 with page-size 2 should contain the 3rd item only.
        var page = await repo.GetAllCompanyInquiriesAsync(
            new PaginatedRequest { PageIndex = 2, PageSize = 2 }, CancellationToken.None);

        Assert.Single(page.Items);
    }

    [Fact]
    public async Task GetAllCompanyInquiriesAsync_IncludesEventTypeNavigation()
    {
        await using var db    = await TestDatabase.CreateAsync();
        var eventTypeId       = await SeedEventTypeAsync(db, "Conference");
        await SeedInquiriesAsync(db, eventTypeId, count: 1);
        var repo              = new CompanyInquiryRepository(db.Context);

        var page = await repo.GetAllCompanyInquiriesAsync(
            new PaginatedRequest { PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        Assert.Equal("Conference", page.Items.Single().EventType.Name);
    }

    // ── Read ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetCompanyInquiryByIdAsync_ReturnsInquiryWithEventType()
    {
        await using var db    = await TestDatabase.CreateAsync();
        var eventTypeId       = await SeedEventTypeAsync(db, "Conference");
        await SeedInquiriesAsync(db, eventTypeId, count: 1);
        var repo              = new CompanyInquiryRepository(db.Context);

        var page    = await repo.GetAllCompanyInquiriesAsync(
            new PaginatedRequest { PageIndex = 1, PageSize = 10 }, CancellationToken.None);
        var inquiry = await repo.GetCompanyInquiryByIdAsync(page.Items.Single().Id, CancellationToken.None);

        Assert.NotNull(inquiry);
        Assert.Equal("Conference", inquiry.EventType.Name);
    }

    [Fact]
    public async Task GetCompanyInquiryByIdAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new CompanyInquiryRepository(db.Context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => repo.GetCompanyInquiryByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    // ── Update ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateCompanyInquiryAsync_PersistsStatusChange()
    {
        await using var db    = await TestDatabase.CreateAsync();
        var eventTypeId       = await SeedEventTypeAsync(db);
        await SeedInquiriesAsync(db, eventTypeId, count: 1);
        var repo              = new CompanyInquiryRepository(db.Context);

        var page    = await repo.GetAllCompanyInquiriesAsync(
            new PaginatedRequest { PageIndex = 1, PageSize = 10 }, CancellationToken.None);
        var original = page.Items.Single();

        await repo.UpdateCompanyInquiryAsync(new CorporationInquiry
        {
            Id                     = original.Id,
            CompanyName            = original.CompanyName,
            ContactPerson          = original.ContactPerson,
            PhoneNumber            = original.PhoneNumber,
            Email                  = original.Email,
            EventTypeId            = eventTypeId,
            ExpectedDate           = original.ExpectedDate,
            EstimatedAttendees     = original.EstimatedAttendees,
            ApproximateBudget      = original.ApproximateBudget,
            AdditionalRequirements = original.AdditionalRequirements,
            Status                 = InquiryStatuses.Approved
        }, CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var updated = await repo.GetCompanyInquiryByIdAsync(original.Id, CancellationToken.None);
        Assert.Equal(InquiryStatuses.Approved, updated.Status);
    }

    // ── Delete ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteCompanyInquiryAsync_ThrowsKeyNotFoundException_WhenNotFound()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new CompanyInquiryRepository(db.Context);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => repo.DeleteCompanyInquiryAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCompanyInquiryAsync_RemovesInquiryFromDatabase()
    {
        await using var db    = await TestDatabase.CreateAsync();
        var eventTypeId       = await SeedEventTypeAsync(db);
        await SeedInquiriesAsync(db, eventTypeId, count: 1);
        var repo              = new CompanyInquiryRepository(db.Context);

        var page = await repo.GetAllCompanyInquiriesAsync(
            new PaginatedRequest { PageIndex = 1, PageSize = 10 }, CancellationToken.None);
        var id   = page.Items.Single().Id;

        await repo.DeleteCompanyInquiryAsync(id, CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => repo.GetCompanyInquiryByIdAsync(id, CancellationToken.None));
    }
}

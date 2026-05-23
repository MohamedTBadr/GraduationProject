using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure;

public class RepositoryIntegrationTests
{
    [Fact]
    public async Task EventTypeRepository_PerformsCrudAgainstRealDbContext()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new EventTypeRespository(database.Context);
        var eventType = new EventType { Id = Guid.NewGuid(), Name = "Wedding" };

        await repository.CreateAsync(eventType, CancellationToken.None);

        Assert.True(await repository.ExistsAsync(eventType.Id, CancellationToken.None));
        Assert.Equal("Wedding", (await repository.GetByIdAsync(eventType.Id, CancellationToken.None)).Name);

        eventType.Name = "Corporate Wedding";
        await repository.UpdateAsync(eventType, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        Assert.Equal("Corporate Wedding", (await repository.GetByIdAsync(eventType.Id, CancellationToken.None)).Name);
        database.Context.ChangeTracker.Clear();

        await repository.DeleteAsync(eventType, CancellationToken.None);

        Assert.False(await repository.ExistsAsync(eventType.Id, CancellationToken.None));
    }

    [Fact]
    public async Task VendorTypeRepository_PerformsCrudAgainstRealDbContext()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new VendorTypeRepository(database.Context);
        var vendorType = new VendorType { Id = Guid.NewGuid(), Name = "Photography" };

        await repository.AddVendorTypeAsync(vendorType, CancellationToken.None);
        var created = await repository.GetVendorTypeByIdAsync(vendorType.Id, CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal("Photography", created.Name);

        created.Name = "Event Photography";
        await repository.UpdateVendorTypeAsync(created, CancellationToken.None);

        var all = await repository.GetVendorTypesAsync(CancellationToken.None);
        Assert.Single(all);
        Assert.Equal("Event Photography", all[0].Name);

        await repository.DeleteVendorTypeAsync(vendorType.Id, CancellationToken.None);
        var deleted = await repository.GetVendorTypeByIdAsync(vendorType.Id, CancellationToken.None);

        Assert.Null(deleted);
    }

    [Fact]
    public async Task ServiceTypeRepository_PerformsCrudAgainstRealDbContext()
    {
        await using var database = await TestDatabase.CreateAsync();
        var vendorType = new VendorType { Id = Guid.NewGuid(), Name = "Creative" };
        database.Context.VendorTypes.Add(vendorType);
        await database.Context.SaveChangesAsync();

        var repository = new ServiceTypeRepository(database.Context);
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = "Decor", VendorTypeId = vendorType.Id };

        await repository.AddTypeAsync(serviceType, CancellationToken.None);
        var created = await repository.GetServiceTypeByIdAsync(serviceType.Id, CancellationToken.None);

        Assert.Equal("Decor", created.Name);

        created.Name = "Premium Decor";
        await repository.UpdateTypeAsync(created, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        var all = await repository.GetAllServiceTypesAsync(CancellationToken.None);
        Assert.Single(all);
        Assert.Equal("Premium Decor", all[0].Name);

        await repository.DeleteTypeAsync(serviceType.Id, CancellationToken.None);

        Assert.Empty(await repository.GetAllServiceTypesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task EventRepository_QueriesCollaboratorsUpdatesItemsAndDeletes()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new EventRepository(database.Context);
        var owner = User("owner@test.com");
        var collaborator = User("collab@test.com");
        var eventType = new EventType { Id = Guid.NewGuid(), Name = "Wedding" };
        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            EventTypeId = eventType.Id,
            Title = "Wedding Party",
            EventDate = DateTime.UtcNow.AddDays(30),
            Location = new Address { Street = "Street", City = "Cairo", State = "Cairo" },
            TotalBudget = 10000,
            GuestCount = 100,
            Notes = "Notes",
            EventStatus = "Planned"
        };
        database.Context.Users.AddRange(owner, collaborator);
        database.Context.EventTypes.Add(eventType);
        database.Context.Events.Add(eventEntity);
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();

        var byOwner = (await repository.GetByUserIdAsync(owner.Id, CancellationToken.None)).ToList();
        Assert.Single(byOwner);

        await repository.AddCollaboratorAsync(new EventCollaborator
        {
            Id = Guid.NewGuid(),
            EventId = eventEntity.Id,
            UserId = collaborator.Id,
            Role = Domain.Enums.CollaboratorRole.Viewer
        }, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        var byCollaborator = (await repository.GetByUserIdAsync(collaborator.Id, CancellationToken.None)).ToList();
        Assert.Single(byCollaborator);

        var collaborators = (await repository.GetCollaboratorsAsync(eventEntity.Id, CancellationToken.None)).ToList();
        Assert.Single(collaborators);
        Assert.Equal(collaborator.Id, collaborators[0].UserId);

        await repository.RemoveCollaboratorAsync(eventEntity.Id, collaborator.Id, CancellationToken.None);
        Assert.Empty(await repository.GetCollaboratorsAsync(eventEntity.Id, CancellationToken.None));

        var statusMatches = await repository.GetByStatusAsync("Planned", CancellationToken.None);
        Assert.Single(statusMatches);

        var loaded = await repository.GetByIdAsync(eventEntity.Id, CancellationToken.None);
        Assert.NotNull(loaded);
        loaded.Title = "Updated Wedding";
        await repository.UpdateAsync(loaded, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        Assert.Equal("Updated Wedding", (await repository.GetByIdAsync(eventEntity.Id, CancellationToken.None))!.Title);
        Assert.True(await repository.DeleteAsync(eventEntity.Id, CancellationToken.None));
        Assert.False(await repository.ExistsAsync(eventEntity.Id, CancellationToken.None));
    }

    [Fact]
    public async Task OrderRepository_PersistsQueriesAmountsAndDeletes()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new OrderRepository(database.Context);
        var owner = User("order-owner@test.com");
        var vendorUser = User("vendor@test.com");
        var eventType = new EventType { Id = Guid.NewGuid(), Name = "Conference" };
        var vendorType = new VendorType { Id = Guid.NewGuid(), Name = "Venue" };
        var serviceType = new ServiceType { Id = Guid.NewGuid(), Name = "Hall", VendorTypeId = vendorType.Id };
        var vendor = new Vendor
        {
            UserId = vendorUser.Id,
            VendorTypeId = vendorType.Id,
            BusinessName = "Venue Co",
            Description = "Venue",
            ProfilePicture = "",
            PortfolioLink = "",
            Address = new Address { Street = "Street", City = "Cairo", State = "Cairo" },
            Document = "",
            Services = [],
            Packages = [],
            VendorRatings = [],
            ServiceAreas = []
        };
        var service = new Service
        {
            Id = Guid.NewGuid(),
            Name = "Main Hall",
            Description = "Hall",
            Price = 200,
            VendorId = vendor.UserId,
            ServiceTypeId = serviceType.Id,
            ServiceImages = [],
            EventTypes = []
        };
        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            EventTypeId = eventType.Id,
            Title = "Conference",
            EventDate = DateTime.UtcNow.AddDays(10),
            Location = new Address { Street = "Street", City = "Cairo", State = "Cairo" },
            TotalBudget = 5000,
            GuestCount = 50,
            Notes = "Notes",
            EventStatus = "Planned"
        };
        var item = new EventItem { Id = Guid.NewGuid(), EventId = eventEntity.Id, ServiceId = service.Id, Quantity = 3, Price = 200 };
        database.Context.Users.AddRange(owner, vendorUser);
        database.Context.EventTypes.Add(eventType);
        database.Context.VendorTypes.Add(vendorType);
        database.Context.ServiceTypes.Add(serviceType);
        database.Context.Vendors.Add(vendor);
        database.Context.Services.Add(service);
        database.Context.Events.Add(eventEntity);
        database.Context.EventItems.Add(item);
        await database.Context.SaveChangesAsync();

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = owner.Id,
            EventId = eventEntity.Id,
            Amount = 600,
            Currency = "EGP",
            PaymentIntentId = "intent-1",
            ShippingAddress = new Address { Street = "Street", City = "Cairo", State = "Cairo" }
        };

        await repository.AddAsync(order, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        Assert.True(await repository.ExistsAsync(order.Id, CancellationToken.None));
        Assert.Equal(600, await repository.GetOrderAmountAsync(eventEntity.Id, CancellationToken.None));
        Assert.NotNull(await repository.GetEventWithItemsAsync(eventEntity.Id, CancellationToken.None));
        Assert.NotNull(await repository.GetByPaymentIntentIdAsync("intent-1", CancellationToken.None));
        Assert.Single(await repository.GetByUserIdAsync(owner.Id, CancellationToken.None));

        var loaded = await repository.GetByIdWithItemsAsync(order.Id, CancellationToken.None);
        Assert.NotNull(loaded);
        loaded.PaymentStatus = "Paid";
        await repository.UpdateAsync(loaded, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        Assert.Equal("Paid", (await repository.GetByIdAsync(order.Id, CancellationToken.None))!.PaymentStatus);

        await repository.DeleteAsync(order.Id, CancellationToken.None);
        Assert.False(await repository.ExistsAsync(order.Id, CancellationToken.None));
    }

    [Fact]
    public async Task VoucherRepository_LoadsOwnerChecksRewardsAndOrdersByCreatedAt()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new VoucherRepository(database.Context);
        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "owner@test.com",
            Email = "owner@test.com",
            FirstName = "Owner",
            LastName = "User",
            ReferralCode = "REF42"
        };

        database.Context.Users.Add(owner);
        var olderVoucher = new Voucher
        {
            Id = Guid.NewGuid(),
            OwnerId = owner.Id,
            Code = "WELCOME",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(10)
        };
        var rewardVoucher = new Voucher
        {
            Id = Guid.NewGuid(),
            OwnerId = owner.Id,
            Code = "REWARD-REF42",
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        };
        database.Context.Vouchers.AddRange(olderVoucher, rewardVoucher);
        await database.Context.SaveChangesAsync();

        var byCode = await repository.GetByCodeAsync("REWARD-REF42", CancellationToken.None);
        var exists = await repository.ExistsForReferrerAsync(owner.Id, "REF42", CancellationToken.None);
        var ownerVouchers = (await repository.GetByOwnerIdAsync(owner.Id, CancellationToken.None)).ToList();

        Assert.NotNull(byCode);
        Assert.NotNull(byCode.Owner);
        Assert.Equal(owner.Email, byCode.Owner.Email);
        Assert.True(exists);
        Assert.Equal(new[] { "REWARD-REF42", "WELCOME" }, ownerVouchers.Select(v => v.Code));
    }

    [Fact]
    public async Task VoucherRepository_AddAsyncAndSaveChanges_PersistsVoucher()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new VoucherRepository(database.Context);
        var owner = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "owner@test.com",
            Email = "owner@test.com",
            FirstName = "Owner",
            LastName = "User"
        };
        database.Context.Users.Add(owner);
        await database.Context.SaveChangesAsync();

        await repository.AddAsync(
            new Voucher
            {
                OwnerId = owner.Id,
                Code = "SAVE10",
                DiscountPercent = 10,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            },
            CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        Assert.True(await database.Context.Vouchers.AnyAsync(v => v.Code == "SAVE10"));
    }

    [Fact]
    public async Task CompanyInquiryRepository_PaginatesIncludesEventTypeAndThrowsForMissingRows()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new CompanyInquiryRepository(database.Context);
        var eventType = new EventType { Id = Guid.NewGuid(), Name = "Conference" };
        database.Context.EventTypes.Add(eventType);
        for (var i = 1; i <= 3; i++)
        {
            database.Context.CorporationInquiries.Add(new CorporationInquiry
            {
                Id = Guid.NewGuid(),
                CompanyName = $"Company {i}",
                ContactPerson = "Ali",
                PhoneNumber = "0100",
                Email = $"company{i}@test.com",
                EventTypeId = eventType.Id,
                ExpectedDate = DateTime.UtcNow.AddDays(i),
                EstimatedAttendees = 100,
                ApproximateBudget = 50000,
                AdditionalRequirements = "AV",
                Status = "Pending"
            });
        }
        await database.Context.SaveChangesAsync();
        database.Context.ChangeTracker.Clear();

        var page = await repository.GetAllCompanyInquiriesAsync(new Shared.PaginatedRequest { PageIndex = 2, PageSize = 2 }, CancellationToken.None);

        Assert.Equal(3, page.TotalCount);
        Assert.Single(page.Items);
        Assert.Equal("Conference", page.Items.Single().EventType.Name);

        var inquiry = page.Items.Single();
        await repository.UpdateCompanyInquiryAsync(new CorporationInquiry
        {
            Id = inquiry.Id,
            CompanyName = inquiry.CompanyName,
            ContactPerson = inquiry.ContactPerson,
            PhoneNumber = inquiry.PhoneNumber,
            Email = inquiry.Email,
            EventTypeId = eventType.Id,
            ExpectedDate = inquiry.ExpectedDate,
            EstimatedAttendees = inquiry.EstimatedAttendees,
            ApproximateBudget = inquiry.ApproximateBudget,
            AdditionalRequirements = inquiry.AdditionalRequirements,
            Status = "Approved"
        }, CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        var loaded = await repository.GetCompanyInquiryByIdAsync(inquiry.Id, CancellationToken.None);
        Assert.Equal("Approved", loaded.Status);
        Assert.Equal("Conference", loaded.EventType.Name);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.GetCompanyInquiryByIdAsync(Guid.NewGuid(), CancellationToken.None));
        await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.DeleteCompanyInquiryAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task SupportTicketRepository_FiltersCountsRepliesAndAgents()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new SupportTicketRepository(database.Context);
        var agent = new SupportAgent { Id = Guid.NewGuid(), AgentCode = "AGT-1", Name = "Support One", Email = "support@test.com" };
        var openTicket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            TicketNumber = "TK-1",
            Title = "Open",
            Description = "Open ticket",
            From = "user@test.com",
            Type = TicketType.Client,
            Priority = TicketPriority.Critical,
            Status = TicketStatus.Open,
            OpenedAt = DateTime.UtcNow.AddHours(-2),
            AssignedAgentId = agent.Id
        };
        var resolvedTicket = new SupportTicket
        {
            Id = Guid.NewGuid(),
            TicketNumber = "TK-2",
            Title = "Resolved",
            Description = "Resolved ticket",
            From = "vendor@test.com",
            Type = TicketType.Vendor,
            Priority = TicketPriority.Low,
            Status = TicketStatus.Resolved,
            OpenedAt = DateTime.UtcNow,
            ResolvedAt = DateTime.UtcNow
        };
        database.Context.SupportAgents.Add(agent);
        database.Context.SupportTickets.AddRange(openTicket, resolvedTicket);
        await database.Context.SaveChangesAsync();

        await repository.AddReplyAsync(new TicketReply
        {
            Id = Guid.NewGuid(),
            TicketId = openTicket.Id,
            ReplyNumber = "RPL-001",
            Message = "Reply",
            RepliedBy = "Admin",
            RepliedAt = DateTime.UtcNow
        }, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);
        database.Context.ChangeTracker.Clear();

        var loaded = await repository.GetByTicketNumberAsync("TK-1", CancellationToken.None);
        var filtered = await repository.GetAllAsync(TicketStatus.Open, TicketPriority.Critical, TicketType.Client, 1, 10, CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Equal("Support One", loaded.AssignedAgent?.Name);
        Assert.Single(loaded.Replies);
        Assert.Single(filtered.Items);
        Assert.Equal(1, await repository.CountByStatusAsync(TicketStatus.Open, CancellationToken.None));
        Assert.Equal(1, await repository.CountByPriorityAsync(TicketPriority.Critical, CancellationToken.None));
        Assert.Equal(50, await repository.GetResolutionRateAsync(CancellationToken.None));
        Assert.NotNull(await repository.GetAgentByCodeAsync("AGT-1", CancellationToken.None));
    }

    [Fact]
    public async Task NotificationRepository_AddRangeQueryAndMarkAsRead_UpdatesPersistedRows()
    {
        await using var database = await TestDatabase.CreateAsync();
        var repository = new NotificationRepository(database.Context);
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        database.Context.Users.AddRange(
            new ApplicationUser
            {
                Id = userId,
                UserName = "notification-user@test.com",
                Email = "notification-user@test.com",
                FirstName = "Notification",
                LastName = "User"
            },
            new ApplicationUser
            {
                Id = otherUserId,
                UserName = "other-notification-user@test.com",
                Email = "other-notification-user@test.com",
                FirstName = "Other",
                LastName = "User"
            });
        await database.Context.SaveChangesAsync();

        var oldNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = NotificationType.ORDER_PLACED,
            Title = "Old",
            Message = "Old message",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        var newNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = NotificationType.ORDER_COMPLETED,
            Title = "New",
            Message = "New message",
            CreatedAt = DateTime.UtcNow
        };
        var otherNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            Type = NotificationType.ORDER_CANCELLED,
            Title = "Other",
            Message = "Other message",
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddRangeAsync(new[] { oldNotification, newNotification, otherNotification });

        var userNotifications = await repository.GetByUserIdAsync(userId);
        Assert.Equal(new[] { "New", "Old" }, userNotifications.Select(n => n.Title));

        database.Context.ChangeTracker.Clear();
        await repository.MarkAsReadAsync(oldNotification.Id, userId);
        database.Context.ChangeTracker.Clear();
        var markedNotification = await database.Context.Notifications.FindAsync(oldNotification.Id);

        Assert.NotNull(markedNotification);
        Assert.True(markedNotification.IsRead);
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection, ApplicationDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public ApplicationDbContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;

            var context = new ApplicationDbContext(options);
            await context.Database.EnsureCreatedAsync();

            return new TestDatabase(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private static ApplicationUser User(string email) => new()
    {
        Id = Guid.NewGuid(),
        UserName = email,
        Email = email,
        FirstName = "Test",
        LastName = "User"
    };
}

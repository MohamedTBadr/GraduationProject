using Domain.Enums;
using EpicHub.IntegrationTests.Infrastructure.Shared;
using Infrastructure.Repositories;
using Xunit;

namespace EpicHub.IntegrationTests.Infrastructure.Tests;

public class SupportTicketRepositoryTests
{
    // ── GetByTicketNumber ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetByTicketNumberAsync_LoadsAssignedAgentNavigation()
    {
        await using var db = await TestDatabase.CreateAsync();
        var agent          = EntityBuilders.BuildSupportAgent("AGT-001");
        var ticket         = EntityBuilders.BuildTicket("TK-1", agentId: agent.Id);
        db.Context.SupportAgents.Add(agent);
        db.Context.SupportTickets.Add(ticket);
        await db.Context.SaveChangesAsync();
        var repo           = new SupportTicketRepository(db.Context);

        var loaded = await repo.GetByTicketNumberAsync("TK-1", CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.NotNull(loaded.AssignedAgent);
        Assert.Equal("Support Agent", loaded.AssignedAgent.Name);
    }

    [Fact]
    public async Task GetByTicketNumberAsync_LoadsReplies()
    {
        await using var db = await TestDatabase.CreateAsync();
        var ticket         = EntityBuilders.BuildTicket("TK-2");
        db.Context.SupportTickets.Add(ticket);
        await db.Context.SaveChangesAsync();
        var repo           = new SupportTicketRepository(db.Context);

        await repo.AddReplyAsync(EntityBuilders.BuildReply(ticket.Id, "RPL-001"), CancellationToken.None);
        await repo.SaveChangesAsync(CancellationToken.None);
        db.Context.ChangeTracker.Clear();

        var loaded = await repo.GetByTicketNumberAsync("TK-2", CancellationToken.None);

        Assert.NotNull(loaded);
        Assert.Single(loaded.Replies);
        Assert.Equal("RPL-001", loaded.Replies.First().ReplyNumber);
    }

    [Fact]
    public async Task GetByTicketNumberAsync_ReturnsNull_WhenTicketNotFound()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new SupportTicketRepository(db.Context);

        var result = await repo.GetByTicketNumberAsync("TK-MISSING", CancellationToken.None);

        Assert.Null(result);
    }

    // ── GetAll with filters ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_FiltersByStatus()
    {
        await using var db = await TestDatabase.CreateAsync();
        db.Context.SupportTickets.AddRange(
            EntityBuilders.BuildTicket("TK-1", status: TicketStatus.Open),
            EntityBuilders.BuildTicket("TK-2", status: TicketStatus.Open),
            EntityBuilders.BuildTicket("TK-3", status: TicketStatus.Resolved));
        await db.Context.SaveChangesAsync();
        var repo = new SupportTicketRepository(db.Context);

        var result = await repo.GetAllAsync(
            TicketStatus.Open, null, null, page: 1, limit: 10, CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, t => Assert.Equal(TicketStatus.Open, t.Status));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByPriority()
    {
        await using var db = await TestDatabase.CreateAsync();
        db.Context.SupportTickets.AddRange(
            EntityBuilders.BuildTicket("TK-1", priority: TicketPriority.Critical),
            EntityBuilders.BuildTicket("TK-2", priority: TicketPriority.Low));
        await db.Context.SaveChangesAsync();
        var repo = new SupportTicketRepository(db.Context);

        var result = await repo.GetAllAsync(
            null, TicketPriority.Critical, null, page: 1, limit: 10, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(TicketPriority.Critical, result.Items[0].Priority);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByType()
    {
        await using var db = await TestDatabase.CreateAsync();
        db.Context.SupportTickets.AddRange(
            EntityBuilders.BuildTicket("TK-1", type: TicketType.Client),
            EntityBuilders.BuildTicket("TK-2", type: TicketType.Vendor));
        await db.Context.SaveChangesAsync();
        var repo = new SupportTicketRepository(db.Context);

        var result = await repo.GetAllAsync(
            null, null, TicketType.Vendor, page: 1, limit: 10, CancellationToken.None);

        Assert.Single(result.Items);
        Assert.Equal(TicketType.Vendor, result.Items[0].Type);
    }

    // ── Count helpers ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CountByStatusAsync_ReturnsCorrectCount()
    {
        await using var db = await TestDatabase.CreateAsync();
        db.Context.SupportTickets.AddRange(
            EntityBuilders.BuildTicket("TK-1", status: TicketStatus.Open),
            EntityBuilders.BuildTicket("TK-2", status: TicketStatus.Open),
            EntityBuilders.BuildTicket("TK-3", status: TicketStatus.Resolved));
        await db.Context.SaveChangesAsync();
        var repo = new SupportTicketRepository(db.Context);

        Assert.Equal(2, await repo.CountByStatusAsync(TicketStatus.Open,     CancellationToken.None));
        Assert.Equal(1, await repo.CountByStatusAsync(TicketStatus.Resolved, CancellationToken.None));
    }

    [Fact]
    public async Task CountByPriorityAsync_ReturnsCorrectCount()
    {
        await using var db = await TestDatabase.CreateAsync();
        db.Context.SupportTickets.AddRange(
            EntityBuilders.BuildTicket("TK-1", priority: TicketPriority.Critical),
            EntityBuilders.BuildTicket("TK-2", priority: TicketPriority.Critical),
            EntityBuilders.BuildTicket("TK-3", priority: TicketPriority.Low));
        await db.Context.SaveChangesAsync();
        var repo = new SupportTicketRepository(db.Context);

        Assert.Equal(2, await repo.CountByPriorityAsync(TicketPriority.Critical, CancellationToken.None));
        Assert.Equal(1, await repo.CountByPriorityAsync(TicketPriority.Low,      CancellationToken.None));
    }

    // ── Resolution rate ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetResolutionRateAsync_Returns50_WhenHalfOfTicketsAreResolved()
    {
        await using var db = await TestDatabase.CreateAsync();
        // 1 resolved out of 2 total = 50 %
        db.Context.SupportTickets.AddRange(
            EntityBuilders.BuildTicket("TK-1", status: TicketStatus.Open),
            EntityBuilders.BuildTicket("TK-2", status: TicketStatus.Resolved));
        await db.Context.SaveChangesAsync();
        var repo = new SupportTicketRepository(db.Context);

        var rate = await repo.GetResolutionRateAsync(CancellationToken.None);

        Assert.Equal(50, rate);
    }

    [Fact]
    public async Task GetResolutionRateAsync_Returns100_WhenAllTicketsAreResolved()
    {
        await using var db = await TestDatabase.CreateAsync();
        db.Context.SupportTickets.AddRange(
            EntityBuilders.BuildTicket("TK-1", status: TicketStatus.Resolved),
            EntityBuilders.BuildTicket("TK-2", status: TicketStatus.Resolved));
        await db.Context.SaveChangesAsync();
        var repo = new SupportTicketRepository(db.Context);

        var rate = await repo.GetResolutionRateAsync(CancellationToken.None);

        Assert.Equal(100, rate);
    }

    // ── Agent lookup ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAgentByCodeAsync_ReturnsMatchingAgent()
    {
        await using var db = await TestDatabase.CreateAsync();
        var agent          = EntityBuilders.BuildSupportAgent("AGT-007");
        db.Context.SupportAgents.Add(agent);
        await db.Context.SaveChangesAsync();
        var repo           = new SupportTicketRepository(db.Context);

        var result = await repo.GetAgentByCodeAsync("AGT-007", CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(agent.Id, result.Id);
    }

    [Fact]
    public async Task GetAgentByCodeAsync_ReturnsNull_WhenAgentNotFound()
    {
        await using var db = await TestDatabase.CreateAsync();
        var repo           = new SupportTicketRepository(db.Context);

        var result = await repo.GetAgentByCodeAsync("AGT-MISSING", CancellationToken.None);

        Assert.Null(result);
    }
}

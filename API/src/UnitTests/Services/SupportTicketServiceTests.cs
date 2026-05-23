using Application.DTOs.Support;
using Application.Services;
using Domain.Contracts;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class SupportTicketServiceTests
{
    private readonly Mock<ISupportTicketRepository> _repositoryMock = new();
    private readonly SupportTicketService _sut;

    public SupportTicketServiceTests()
    {
        _sut = new SupportTicketService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_CreatesOpenTicketAndSaves()
    {
        var request = new CreateTicketRequestDTO { Title = "Help", Description = "Need help", Type = "Vendor", Priority = "Critical", BookingRef = "B-1" };

        var result = await _sut.CreateAsync(request, "user@test.com", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Help", result.Value.Title);
        Assert.Equal("Vendor", result.Value.Type);
        Assert.Equal("critical", result.Value.Priority);
        Assert.Equal("open", result.Value.Status);
        _repositoryMock.Verify(x => x.AddAsync(It.Is<SupportTicket>(t => t.From == "user@test.com" && t.Status == TicketStatus.Open), It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsRepositoryCounts()
    {
        _repositoryMock.Setup(x => x.CountByPriorityAsync(TicketPriority.Critical, It.IsAny<CancellationToken>())).ReturnsAsync(2);
        _repositoryMock.Setup(x => x.CountByStatusAsync(TicketStatus.Open, It.IsAny<CancellationToken>())).ReturnsAsync(3);
        _repositoryMock.Setup(x => x.CountByStatusAsync(TicketStatus.InProgress, It.IsAny<CancellationToken>())).ReturnsAsync(4);
        _repositoryMock.Setup(x => x.GetResolutionRateAsync(It.IsAny<CancellationToken>())).ReturnsAsync(80);

        var result = await _sut.GetStatsAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Critical);
        Assert.Equal(3, result.Value.Open);
        Assert.Equal(4, result.Value.InProgress);
        Assert.Equal(80, result.Value.ResolutionRate);
    }

    [Fact]
    public async Task GetAllAsync_ParsesFiltersAndReturnsPagedResult()
    {
        var ticket = Ticket("TK-1", TicketStatus.Open, TicketPriority.High, TicketType.Client);
        _repositoryMock
            .Setup(x => x.GetAllAsync(TicketStatus.Open, TicketPriority.High, TicketType.Client, 2, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(([ticket], 15));

        var result = await _sut.GetAllAsync(new TicketQueryDTO { Status = "open", Priority = "high", Type = "client", Page = 2, Limit = 10 }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(15, result.Value.Total);
        Assert.Single(result.Value.Data);
        Assert.Equal("high", result.Value.Data[0].Priority);
    }

    [Fact]
    public async Task GetByIdAsync_WhenMissing_ReturnsNotFound()
    {
        _repositoryMock
            .Setup(x => x.GetByTicketNumberAsync("TK-404", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportTicket?)null);

        var result = await _sut.GetByIdAsync("TK-404", CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error?.Code);
    }

    [Fact]
    public async Task ReplyAsync_WhenTicketExists_AddsReplyWithNextNumberAndNotificationChannels()
    {
        var ticket = Ticket("TK-1");
        ticket.Replies.Add(new TicketReply { ReplyNumber = "RPL-001", Message = "First", RepliedBy = "Admin", RepliedAt = DateTime.UtcNow });
        _repositoryMock.Setup(x => x.GetByTicketNumberAsync("TK-1", It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

        var result = await _sut.ReplyAsync("TK-1", new TicketReplyRequestDTO { Message = "Second", SendEmail = true, SendSms = true }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("RPL-002", result.Value.ReplyId);
        Assert.Equal(["email", "sms"], result.Value.NotifiedVia);
        _repositoryMock.Verify(x => x.AddReplyAsync(It.Is<TicketReply>(r => r.TicketId == ticket.Id && r.Message == "Second"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignAsync_WhenAgentMissing_ReturnsNotFound()
    {
        _repositoryMock.Setup(x => x.GetByTicketNumberAsync("TK-1", It.IsAny<CancellationToken>())).ReturnsAsync(Ticket("TK-1"));
        _repositoryMock.Setup(x => x.GetAgentByCodeAsync("AGT-404", It.IsAny<CancellationToken>())).ReturnsAsync((SupportAgent?)null);

        var result = await _sut.AssignAsync("TK-1", new TicketAssignRequestDTO { AgentId = "AGT-404" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.Error?.Code);
    }

    [Fact]
    public async Task AssignAsync_WhenTicketAndAgentExist_AssignsAgentAndMovesInProgress()
    {
        var ticket = Ticket("TK-1");
        var agent = new SupportAgent { Id = Guid.NewGuid(), AgentCode = "AGT-1", Name = "Support One", Email = "support@test.com" };
        _repositoryMock.Setup(x => x.GetByTicketNumberAsync("TK-1", It.IsAny<CancellationToken>())).ReturnsAsync(ticket);
        _repositoryMock.Setup(x => x.GetAgentByCodeAsync("AGT-1", It.IsAny<CancellationToken>())).ReturnsAsync(agent);

        var result = await _sut.AssignAsync("TK-1", new TicketAssignRequestDTO { AgentId = "AGT-1", Note = "Handle quickly" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TicketStatus.InProgress, ticket.Status);
        Assert.Equal(agent.Id, ticket.AssignedAgentId);
        Assert.Equal("Support One", result.Value.AssignedTo.Name);
        _repositoryMock.Verify(x => x.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveAsync_WhenAlreadyResolved_ReturnsBusinessRuleFailure()
    {
        _repositoryMock.Setup(x => x.GetByTicketNumberAsync("TK-1", It.IsAny<CancellationToken>())).ReturnsAsync(Ticket("TK-1", TicketStatus.Resolved));

        var result = await _sut.ResolveAsync("TK-1", new TicketResolveRequestDTO { ResolutionNote = "Done" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ticket is already resolved", result.Error?.Description);
    }

    [Fact]
    public async Task ResolveAsync_WhenOpen_ResolvesAndSaves()
    {
        var ticket = Ticket("TK-1");
        _repositoryMock.Setup(x => x.GetByTicketNumberAsync("TK-1", It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

        var result = await _sut.ResolveAsync("TK-1", new TicketResolveRequestDTO { ResolutionNote = "Fixed" }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TicketStatus.Resolved, ticket.Status);
        Assert.Equal("Fixed", ticket.ResolutionNote);
        _repositoryMock.Verify(x => x.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EscalateAsync_WhenAlreadyEscalated_ReturnsBusinessRuleFailure()
    {
        var ticket = Ticket("TK-1");
        ticket.IsEscalated = true;
        _repositoryMock.Setup(x => x.GetByTicketNumberAsync("TK-1", It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

        var result = await _sut.EscalateAsync("TK-1", new TicketEscalateRequestDTO { EscalateTo = "cto", Reason = "Urgent" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Ticket has already been escalated", result.Error?.Description);
    }

    [Fact]
    public async Task EscalateAsync_WhenNotEscalated_UpdatesEscalationFields()
    {
        var ticket = Ticket("TK-1");
        _repositoryMock.Setup(x => x.GetByTicketNumberAsync("TK-1", It.IsAny<CancellationToken>())).ReturnsAsync(ticket);

        var result = await _sut.EscalateAsync("TK-1", new TicketEscalateRequestDTO { EscalateTo = "cto", Reason = "Urgent", NotifyFinance = true }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(ticket.IsEscalated);
        Assert.Equal("cto", ticket.EscalatedTo);
        Assert.True(ticket.FinanceNotified);
        _repositoryMock.Verify(x => x.UpdateAsync(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static SupportTicket Ticket(
        string ticketNumber,
        TicketStatus status = TicketStatus.Open,
        TicketPriority priority = TicketPriority.Low,
        TicketType type = TicketType.Client) => new()
        {
            Id = Guid.NewGuid(),
            TicketNumber = ticketNumber,
            Title = "Help",
            Description = "Need help",
            From = "user@test.com",
            Type = type,
            Priority = priority,
            Status = status,
            OpenedAt = DateTime.UtcNow
        };
}

using Application.Services;
using Domain.Contracts;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Application.UnitTests.Services;

public class VoucherServiceTests
{
    private readonly Mock<IVoucherRepository> _voucherRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly VoucherService _sut;

    public VoucherServiceTests()
    {
        _configurationMock
            .Setup(x => x["App:BaseUrl"])
            .Returns("https://epichub.test");

        _sut = new VoucherService(_voucherRepoMock.Object, _userRepoMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task ApplyReferralAsync_WhenReferralCodeIsInvalid_DoesNothing()
    {
        _userRepoMock
            .Setup(x => x.GetByReferralCodeAsync("BAD", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        await _sut.ApplyReferralAsync("BAD", Guid.NewGuid(), CancellationToken.None);

        _voucherRepoMock.Verify(x => x.AddAsync(It.IsAny<Voucher>(), It.IsAny<CancellationToken>()), Times.Never);
        _voucherRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApplyReferralAsync_WhenSelfReferral_DoesNothing()
    {
        var userId = Guid.NewGuid();
        _userRepoMock
            .Setup(x => x.GetByReferralCodeAsync("SELF", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser { Id = userId });

        await _sut.ApplyReferralAsync("SELF", userId, CancellationToken.None);

        _voucherRepoMock.Verify(x => x.AddAsync(It.IsAny<Voucher>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApplyReferralAsync_WhenAlreadyRewarded_DoesNothing()
    {
        var referrer = new ApplicationUser { Id = Guid.NewGuid() };
        _userRepoMock
            .Setup(x => x.GetByReferralCodeAsync("ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(referrer);
        _voucherRepoMock
            .Setup(x => x.ExistsForReferrerAsync(referrer.Id, "ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.ApplyReferralAsync("ABC123", Guid.NewGuid(), CancellationToken.None);

        _voucherRepoMock.Verify(x => x.AddAsync(It.IsAny<Voucher>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApplyReferralAsync_WhenValid_CreatesRewardVoucher()
    {
        var referrer = new ApplicationUser { Id = Guid.NewGuid() };
        _userRepoMock
            .Setup(x => x.GetByReferralCodeAsync("ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(referrer);
        _voucherRepoMock
            .Setup(x => x.ExistsForReferrerAsync(referrer.Id, "ABC123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.ApplyReferralAsync("ABC123", Guid.NewGuid(), CancellationToken.None);

        _voucherRepoMock.Verify(
            x => x.AddAsync(
                It.Is<Voucher>(v =>
                    v.OwnerId == referrer.Id &&
                    v.Code == "REWARD-ABC123" &&
                    v.DiscountPercent == 5 &&
                    v.ExpiresAt > DateTime.UtcNow),
                It.IsAny<CancellationToken>()),
            Times.Once);
        _voucherRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetReferralLinkAsync_WhenUserExists_ReturnsConfiguredLink()
    {
        var userId = Guid.NewGuid();
        _userRepoMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser { Id = userId, ReferralCode = "REF42" });

        var result = await _sut.GetReferralLinkAsync(userId, CancellationToken.None);

        Assert.Equal("https://epichub.test/register?ref=REF42", result);
    }

    [Fact]
    public async Task GetReferralLinkAsync_WhenUserMissing_ThrowsKeyNotFoundException()
    {
        _userRepoMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetReferralLinkAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task GetMyVouchersAsync_ReturnsMappedVoucherDtos()
    {
        var userId = Guid.NewGuid();
        var vouchers = new[]
        {
            new Voucher { Id = Guid.NewGuid(), OwnerId = userId, Code = "A", DiscountPercent = 5, IsUsed = false },
            new Voucher { Id = Guid.NewGuid(), OwnerId = userId, Code = "B", DiscountPercent = 10, IsUsed = true }
        };
        _voucherRepoMock
            .Setup(x => x.GetByOwnerIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vouchers);

        var result = (await _sut.GetMyVouchersAsync(userId, CancellationToken.None)).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(vouchers.Select(v => v.Code), result.Select(v => v.Code));
        Assert.Equal(vouchers.Select(v => v.IsUsed), result.Select(v => v.IsUsed));
    }

    [Theory]
    [InlineData(null, "Voucher not found.")]
    [InlineData("other-owner", "Voucher does not belong to you.")]
    [InlineData("used", "Voucher has already been used.")]
    [InlineData("expired", "Voucher has expired.")]
    public async Task ValidateVoucherAsync_WhenInvalid_ReturnsFailure(string? scenario, string expectedError)
    {
        var userId = Guid.NewGuid();
        Voucher? voucher = scenario switch
        {
            null => null,
            "other-owner" => new Voucher { OwnerId = Guid.NewGuid(), Code = "SAVE" },
            "used" => new Voucher { OwnerId = userId, Code = "SAVE", IsUsed = true },
            "expired" => new Voucher { OwnerId = userId, Code = "SAVE", ExpiresAt = DateTime.UtcNow.AddMinutes(-1) },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario))
        };
        _voucherRepoMock
            .Setup(x => x.GetByCodeAsync("SAVE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(voucher);

        var result = await _sut.ValidateVoucherAsync("SAVE", userId, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.Equal(0, result.DiscountPercent);
        Assert.Equal(expectedError, result.ErrorMessage);
    }

    [Fact]
    public async Task ValidateVoucherAsync_WhenValid_ReturnsDiscount()
    {
        var userId = Guid.NewGuid();
        _voucherRepoMock
            .Setup(x => x.GetByCodeAsync("SAVE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Voucher
            {
                OwnerId = userId,
                Code = "SAVE",
                DiscountPercent = 15,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });

        var result = await _sut.ValidateVoucherAsync("SAVE", userId, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.Equal(15, result.DiscountPercent);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task MarkVoucherUsedAsync_WhenVoucherMissing_ThrowsKeyNotFoundException()
    {
        _voucherRepoMock
            .Setup(x => x.GetByCodeAsync("MISSING", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Voucher?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.MarkVoucherUsedAsync("MISSING", CancellationToken.None));
    }

    [Fact]
    public async Task MarkVoucherUsedAsync_WhenVoucherExists_MarksUsedAndSaves()
    {
        var voucher = new Voucher { Code = "SAVE", IsUsed = false };
        _voucherRepoMock
            .Setup(x => x.GetByCodeAsync("SAVE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(voucher);

        await _sut.MarkVoucherUsedAsync("SAVE", CancellationToken.None);

        Assert.True(voucher.IsUsed);
        _voucherRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkVoucherUnusedAsync_WhenVoucherExists_MarksUnusedAndSaves()
    {
        var voucher = new Voucher { Code = "SAVE", IsUsed = true };
        _voucherRepoMock
            .Setup(x => x.GetByCodeAsync("SAVE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(voucher);

        await _sut.MarkVoucherUnusedAsync("SAVE", CancellationToken.None);

        Assert.False(voucher.IsUsed);
        _voucherRepoMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

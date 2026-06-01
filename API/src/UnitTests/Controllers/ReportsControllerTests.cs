using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.Contracts;
using Domain.Enums;
using Application.DTOs.Reports;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Api.Controllers;
using Xunit;

namespace Application.UnitTests.Controllers;

public class ReportsControllerTests
{
    private readonly Mock<IReportingService> _reportingMock;
    private readonly Mock<IPdfReportService> _pdfMock;
    private readonly Mock<IBackgroundJobClient> _jobsMock;
    private readonly ReportsController _sut;

    public ReportsControllerTests()
    {
        _reportingMock = new Mock<IReportingService>();
        _pdfMock = new Mock<IPdfReportService>();
        _jobsMock = new Mock<IBackgroundJobClient>();
        
        _sut = new ReportsController(
            _reportingMock.Object,
            _pdfMock.Object,
            _jobsMock.Object);
    }
    
    private void SetUserContext(string? role, Guid? nameIdentifier = null, string? email = null, string? name = null)
    {
        var claims = new System.Collections.Generic.List<Claim>();

        if (role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        
        if (nameIdentifier.HasValue)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, nameIdentifier.Value.ToString()));
        }

        if (email != null)
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        if (name != null)
        {
            claims.Add(new Claim(ClaimTypes.GivenName, name));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal
            }
        };
    }

    [Fact]
    public async Task GetExecutiveReport_AsAdmin_ReturnsOkWithReport()
    {
        // Arrange
        SetUserContext("Admin");
        var expectedReport = new ExecutiveReportDto();
        
        _reportingMock
            .Setup(x => x.GenerateFullReportAsync(null, ReportScope.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReport);

        // Act
        var result = await _sut.GetExecutiveReport(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedReport, okResult.Value);
    }

    [Fact]
    public void Constructor_WithValidArguments_InitializesController()
    {
        // Assert
        Assert.NotNull(_sut);
    }
    
    [Fact]
    public async Task GetExecutiveReport_AsVendor_ReturnsOkWithReport()
    {
        // Arrange
        var vendorId = Guid.NewGuid();
        SetUserContext("Vendor", vendorId);
        var expectedReport = new ExecutiveReportDto();
        
        _reportingMock
            .Setup(x => x.GenerateFullReportAsync(vendorId, ReportScope.Vendor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedReport);

        // Act
        var result = await _sut.GetExecutiveReport(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedReport, okResult.Value);
    }

    [Fact]
    public async Task GetExecutiveReport_WithInvalidScope_ReturnsForbid()
    {
        // Arrange
        SetUserContext("Customer"); // not admin, not vendor

        // Act
        var result = await _sut.GetExecutiveReport(CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DownloadExecutiveReportPdf_AsAdmin_ReturnsFileResult()
    {
        // Arrange
        SetUserContext("Admin");
        var report = new ExecutiveReportDto(); // GeneratedAt will be default
        var pdfBytes = new byte[] { 1, 2, 3 };
        
        _reportingMock
            .Setup(x => x.GenerateFullReportAsync(null, ReportScope.Admin, It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);
            
        _pdfMock
            .Setup(x => x.RenderAsync(report, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pdfBytes);

        // Act
        var result = await _sut.DownloadExecutiveReportPdf(CancellationToken.None);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal(pdfBytes, fileResult.FileContents);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.Contains("executive-report", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task DownloadExecutiveReportPdf_WithInvalidScope_ReturnsForbid()
    {
        // Arrange
        SetUserContext("Customer");

        // Act
        var result = await _sut.DownloadExecutiveReportPdf(CancellationToken.None);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void EnqueueReportEmail_WithInvalidScope_ReturnsForbid()
    {
        // Arrange
        SetUserContext("Customer");

        // Act
        var result = _sut.EnqueueReportEmail();

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
    
    [Fact]
    public void EnqueueReportEmail_AsVendor_EnqueuesJobAndReturnsAccepted()
    {
        // Arrange
        var vendorId = Guid.NewGuid();
        var email = "vendor@test.com";
        var name = "VendorUser";
        SetUserContext("Vendor", vendorId, email, name);

        // _jobs.Enqueue is an extension method. 
        // Hangfire IBackgroundJobClient has Create method for enqueueing
        _jobsMock.Setup(x => x.Create(It.IsAny<Hangfire.Common.Job>(), It.IsAny<Hangfire.States.EnqueuedState>()))
                 .Returns("job-123");

        // Act
        var result = _sut.EnqueueReportEmail();

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        Assert.NotNull(acceptedResult.Value);
    }
}

using Application.Services;
using Application.Interfaces;
using Domain.Contracts;
using Domain.Entities;
using Moq;
using Xunit;
using Application.Services.Helpers;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Shared;
using Application.DTOs.VendorDTOs;
using OpenAI.Chat;
using System.Linq.Expressions;
using System.ClientModel;


namespace Application.UnitTests.Services
{
    public class VendorServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IVendorRepository> _vendorRepositoryMock;
        private readonly Mock<IEventItemRepository> _eventItemRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly Mock<ISearchService> _searchServiceMock;
        private readonly VendorService _sut;
        
        public VendorServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            
            _vendorRepositoryMock = new Mock<IVendorRepository>();
            _eventItemRepositoryMock = new Mock<IEventItemRepository>();
            _mapperMock = new Mock<IMapper>();
            _fileServiceMock = new Mock<IFileService>();
            
            LlamaService llamaService = null!; 
            
            _searchServiceMock = new Mock<ISearchService>();
            
            _sut = new VendorService(
                _userRepositoryMock.Object,
                _userManagerMock.Object,
                _vendorRepositoryMock.Object,
                _eventItemRepositoryMock.Object,
                _mapperMock.Object,
                _fileServiceMock.Object,
                llamaService,
                _searchServiceMock.Object
            );
        }

        [Fact]
        public async Task GetVendorByIdAsync_VendorNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var vendorId = Guid.NewGuid();
            _vendorRepositoryMock.Setup(x => x.GetVendorByIdAsync(vendorId, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((Vendor?)null);

            // Act
            var result = await _sut.GetVendorByIdAsync(vendorId, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.Error!.Code);
        }
    }
}
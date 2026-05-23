using System;
using Application.Interfaces;
using Application.Interfaces.Services;
using Application.Services;
using Moq;
using Xunit;

namespace Application.UnitTests.Services
{
    public class ServiceManagerTests
    {
        [Fact]
        public void Constructor_ShouldInitializeLazily_DoesNotCallServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var serviceManager = new ServiceManager(serviceProviderMock.Object);

            // Assert
            serviceProviderMock.Verify(x => x.GetService(It.IsAny<Type>()), Times.Never);
        }

        [Fact]
        public void EmailSender_WhenAccessed_ResolvesFromServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var emailSenderMock = new Mock<IEmailSender>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IEmailSender)))
                .Returns(emailSenderMock.Object);

            var serviceManager = new ServiceManager(serviceProviderMock.Object);

            // Act
            var result = serviceManager.EmailSender;

            // Assert
            Assert.NotNull(result);
            Assert.Same(emailSenderMock.Object, result);
            serviceProviderMock.Verify(x => x.GetService(typeof(IEmailSender)), Times.Once);
        }

        [Fact]
        public void AuthenticationService_WhenAccessed_ResolvesFromServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IAuthenticationService)))
                .Returns(authenticationServiceMock.Object);

            var serviceManager = new ServiceManager(serviceProviderMock.Object);

            // Act
            var result = serviceManager.AuthenticationService;

            // Assert
            Assert.NotNull(result);
            Assert.Same(authenticationServiceMock.Object, result);
            serviceProviderMock.Verify(x => x.GetService(typeof(IAuthenticationService)), Times.Once);
        }

        [Fact]
        public void AttachmentService_WhenAccessed_ResolvesFromServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var attachmentServiceMock = new Mock<IAttachmentService>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IAttachmentService)))
                .Returns(attachmentServiceMock.Object);

            var serviceManager = new ServiceManager(serviceProviderMock.Object);

            // Act
            var result = serviceManager.AttachmentService;

            // Assert
            Assert.NotNull(result);
            Assert.Same(attachmentServiceMock.Object, result);
            serviceProviderMock.Verify(x => x.GetService(typeof(IAttachmentService)), Times.Once);
        }

        [Fact]
        public void VendorService_WhenAccessed_ResolvesFromServiceProvider()
        {
            // Arrange
            var serviceProviderMock = new Mock<IServiceProvider>();
            var vendorServiceMock = new Mock<IVendorService>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IVendorService)))
                .Returns(vendorServiceMock.Object);

            var serviceManager = new ServiceManager(serviceProviderMock.Object);

            // Act
            var result = serviceManager.VendorService;

            // Assert
            Assert.NotNull(result);
            Assert.Same(vendorServiceMock.Object, result);
            serviceProviderMock.Verify(x => x.GetService(typeof(IVendorService)), Times.Once);
        }
    }
}
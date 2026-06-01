using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.AuthenticationDTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Exceptions;
using Web.Api.Controllers;
using Xunit;

namespace Application.UnitTests.Controllers
{
    using AuthService = Application.Interfaces.IAuthenticationService;

    public class AuthenticationControllerTests
    {
        private readonly Mock<IServiceManager> _serviceManagerMock;
        private readonly Mock<AuthService> _authServiceMock;
        private readonly AuthenticationController _sut;

        public AuthenticationControllerTests()
        {
            _serviceManagerMock = new Mock<IServiceManager>();
            _authServiceMock = new Mock<AuthService>();

            _serviceManagerMock.SetupGet(x => x.AuthenticationService)
                .Returns(_authServiceMock.Object);

            _sut = new AuthenticationController(_serviceManagerMock.Object);
        }

        // ---------------------------
        // Helper: Setup Controller User
        // ---------------------------
        private void SetUser(Guid userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        // =========================================================
        // LOGIN TESTS
        // =========================================================

        [Fact]
        public async Task Login_ValidRequest_ReturnsOk()
        {
            var request = new LoginRequest("test@test.com", "123456");

            var expected = Result<UserResponse>.Success(new UserResponse("test", "test@test.com", "testttt","test","Admin"));

            _authServiceMock
                .Setup(x => x.LogIn(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Login(request, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected.Value, ok.Value);
        }

        [Fact]
        public async Task Login_InvalidModelState_ThrowsUnprocessableContentException()
        {
            _sut.ModelState.AddModelError("Email", "Required");

            var request = new LoginRequest("", "");

            await Assert.ThrowsAsync<UnprocessableContentException>(() =>
                _sut.Login(request, CancellationToken.None));
        }

        [Fact]
        public async Task Login_FailedResult_ReturnsMappedActionResult()
        {
            var request = new LoginRequest("test@test.com", "wrong");

            var failure = Result<UserResponse>.Validation(400, "Invalid credentials");

            _authServiceMock
                .Setup(x => x.LogIn(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(failure);

            var result = await _sut.Login(request, CancellationToken.None);

            Assert.IsAssignableFrom<IActionResult>(result);
        }

        // =========================================================
        // REGISTER TESTS
        // =========================================================

        [Fact]
        public async Task Register_ValidRequest_ReturnsOk()
        {
            var request = new SignUpRequest("test", "test", "test@test.com", "123456", "01121212", string.Empty);

            var expected = Result<UserResponse>.Success(new UserResponse("test", "test@test.com", "testttt","test","Admin"));

            _authServiceMock
                .Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _sut.Register(request, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expected.Value, ok.Value);
        }

        [Fact]
        public async Task Register_InvalidModelState_ThrowsBadRequestException()
        {
            _sut.ModelState.AddModelError("Email", "Invalid");

            var request = new SignUpRequest("test", "test", "invalid-email", "123456", "01121212", string.Empty);

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _sut.Register(request, CancellationToken.None));
        }

        // =========================================================
        // EMAIL CHECK TEST
        // =========================================================

        [Fact]
        public async Task CheckEmailExists_ReturnsAccepted()
        {
            var result = await _sut.CheckEmailExists("test@test.com", CancellationToken.None);

            var accepted = Assert.IsType<AcceptedResult>(result);

            Assert.NotNull(accepted.Value);
        }

        // =========================================================
        // REFRESH TOKEN TESTS
        // =========================================================

        [Fact]
        public async Task RefreshToken_ValidRequest_ReturnsOk()
        {
            var request = new RefreshTokenRequest("valid-token");

            var expectedUser = new UserResponse(
                "test",
                "test@test.com",
                "testttt",
                "test",
                "Admin"
            );

            var expectedResult = Result<UserResponse>.Success(expectedUser);

            _authServiceMock
                .Setup(x => x.RefreshTokenAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            var result = await _sut.RefreshToken(request, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);

            var actual = Assert.IsType<Result<UserResponse>>(ok.Value);

            Assert.True(actual.IsSuccess);
            Assert.Equal(expectedUser.name, actual.Value.name);
            Assert.Equal(expectedUser.email, actual.Value.email);
            Assert.Equal(expectedUser.AccessToken, actual.Value.AccessToken);
            Assert.Equal(expectedUser.RefreshToken, actual.Value.RefreshToken);
            Assert.Equal(expectedUser.role, actual.Value.role);
        }

        [Fact]
        public async Task RefreshToken_EmptyToken_ReturnsBadRequest()
        {
            var request = new RefreshTokenRequest("");

            var result = await _sut.RefreshToken(request, CancellationToken.None);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Refresh token is required.", bad.Value);
        }

        [Fact]
        public async Task RefreshToken_UnauthorizedException_ReturnsUnauthorized()
        {
            var request = new RefreshTokenRequest("token");

            _authServiceMock
                .Setup(x => x.RefreshTokenAsync(request, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedException("Invalid refresh token"));

            var result = await _sut.RefreshToken(request, CancellationToken.None);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorized.Value);
        }

        // =========================================================
        // FORGET PASSWORD TESTS
        // =========================================================

        [Fact]
        public async Task ForgetPassword_ValidRequest_ReturnsAccepted()
        {
            var email = "test@test.com";

            _authServiceMock
                .Setup(x => x.ForgetPassword(email, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.ForgetPassword(email, CancellationToken.None);

            var accepted = Assert.IsType<AcceptedResult>(result);
            Assert.NotNull(accepted.Value);
        }

        [Fact]
        public async Task ResetPassword_ValidRequest_ReturnsOk()
        {
            var request = new ResetPasswordRequest("test@test.com", "old-password", "new-password");

            _authServiceMock
                .Setup(x => x.ResetPassword(request, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.ResetPassword(request, CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task ResetPassword_InvalidModelState_ThrowsBadRequestException()
        {
            _sut.ModelState.AddModelError("Password", "Required");

            var request = new ResetPasswordRequest("test@test.com", "old-password", "new-password");

            await Assert.ThrowsAsync<BadRequestException>(() =>
                _sut.ResetPassword(request, CancellationToken.None));
        }

        // =========================================================
        // LOGOUT TESTS
        // =========================================================

        [Fact]
        public async Task Logout_ValidUser_ReturnsOk()
        {
            var userId = Guid.NewGuid();
            SetUser(userId);

            _authServiceMock
                .Setup(x => x.LogoutAsync(userId, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _sut.Logout(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(ok.Value);
        }

        [Fact]
        public async Task Logout_MissingUserId_ReturnsUnauthorized()
        {
            // Arrange
            var emptyClaims = new ClaimsPrincipal(new ClaimsIdentity());

            _sut.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = emptyClaims
                }
            };

            // Act
            var result = await _sut.Logout(CancellationToken.None);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}
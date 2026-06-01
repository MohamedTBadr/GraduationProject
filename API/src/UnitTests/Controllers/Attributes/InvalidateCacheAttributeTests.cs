using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Application.UnitTests.Controllers.Attributes
{
    public class InvalidateCacheAttributeTests
    {
        [Fact]
        public void Constructor_SetsTags()
        {
            // Arrange & Act
            var attribute = new InvalidateCacheAttribute("tag1", "tag2");

            // Assert
            Assert.Equal(new[] { "tag1", "tag2" }, attribute.Tags);
        }

        private static (ActionExecutingContext, ActionExecutedContext, Mock<HybridCache>) SetupContexts(IActionResult? result = null, Exception? exception = null)
        {
            var actionContext = new ActionContext(
                new DefaultHttpContext(),
                new RouteData(),
                new ActionDescriptor()
            );

            var executingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                new Mock<Controller>().Object
            );

            var executedContext = new ActionExecutedContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Mock<Controller>().Object
            )
            {
                Result = result,
                Exception = exception
            };

            var mockCache = new Mock<HybridCache>();
            var services = new ServiceCollection();
            services.AddSingleton(mockCache.Object);
            actionContext.HttpContext.RequestServices = services.BuildServiceProvider();

            return (executingContext, executedContext, mockCache);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenExceptionOccurs_ShouldNotInvalidateCache()
        {
            // Arrange
            var attribute = new InvalidateCacheAttribute("tag1");
            var (executingContext, executedContext, mockCache) = SetupContexts(exception: new Exception("Test error"));

            // Act
            await attribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(executedContext));

            // Assert
            mockCache.Verify(c => c.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Theory]
        [InlineData(199)]
        [InlineData(300)]
        public async Task OnActionExecutionAsync_WhenStatusCodeNotSuccess_ShouldNotInvalidateCache(int statusCode)
        {
            // Arrange
            var attribute = new InvalidateCacheAttribute("tag1");
            var (executingContext, executedContext, mockCache) = SetupContexts(result: new StatusCodeResult(statusCode));

            // Act
            await attribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(executedContext));

            // Assert
            mockCache.Verify(c => c.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenNoResultTypeMatched_ShouldNotInvalidateCache()
        {
            // Arrange
            var attribute = new InvalidateCacheAttribute("tag1");
            // Results like EmptyResult do not have a StatusCode property in this attribute's switch statement
            var (executingContext, executedContext, mockCache) = SetupContexts(result: new EmptyResult());

            // Act
            await attribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(executedContext));

            // Assert
            mockCache.Verify(c => c.RemoveByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenStatusCodeSuccessObjectResult_ShouldInvalidateCacheTagsAndResolvePlaceholders()
        {
            // Arrange
            var attribute = new InvalidateCacheAttribute("product-{id}", "user-{UserId}", "role-{UserRole}");
            var (executingContext, executedContext, mockCache) = SetupContexts(result: new ObjectResult("OK") { StatusCode = 200 });

            // Setup RouteData
            executingContext.RouteData.Values.Add("id", "123");

            // Setup User Claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "user-456"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            executingContext.HttpContext.User = claimsPrincipal;

            // Act
            await attribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(executedContext));

            // Assert
            mockCache.Verify(c => c.RemoveByTagAsync("product-123", It.IsAny<CancellationToken>()), Times.Once);
            mockCache.Verify(c => c.RemoveByTagAsync("user-user-456", It.IsAny<CancellationToken>()), Times.Once);
            mockCache.Verify(c => c.RemoveByTagAsync("role-Admin", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task OnActionExecutionAsync_WhenStatusCodeSuccessStatusCodeResult_ShouldInvalidateCacheTags()
        {
            // Arrange
            var attribute = new InvalidateCacheAttribute("simple-tag");
            var (executingContext, executedContext, mockCache) = SetupContexts(result: new StatusCodeResult(201));

            // Act
            await attribute.OnActionExecutionAsync(executingContext, () => Task.FromResult(executedContext));

            // Assert
            mockCache.Verify(c => c.RemoveByTagAsync("simple-tag", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

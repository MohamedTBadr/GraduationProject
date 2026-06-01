using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Web.Api.Attributes;
using Xunit;

namespace Application.UnitTests.Controllers.Attributes
{
    public class HybridCacheAttributeTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            // Arrange
            int duration = 60;
            string[] tags = new[] { "tag1", "tag2" };

            // Act
            var attribute = new HybridCacheAttribute(duration, tags);

            // Assert
            Assert.Equal(duration, attribute.DurationInSeconds);
            Assert.Equal(tags, attribute.Tags);
            Assert.False(attribute.CachePostRequest);
            Assert.False(attribute.PerUser);
            Assert.False(attribute.PerRole);
            Assert.Equal(CacheVariance.Shared, attribute.Variance);
            Assert.Equal(ClaimTypes.Role, attribute.RoleClaim);
        }
    }
}

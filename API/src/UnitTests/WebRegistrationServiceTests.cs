using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Web.Api;
using Xunit;

namespace Application.UnitTests
{
    public class WebRegistrationServiceTests
    {
        [Fact]
        public async Task AddWebsRegistrationServices_RegistersExpectedServices()
        {
            // Arrange
            var services = new ServiceCollection();
            // To satisfy Hangfire SqlServer storage, identity EF stores, etc. we need minimum services to avoid exceptions, 
            // but the method just registers definitions. Wait, EF store requires 'ApplicationDbContext'.
            // The method doesn't add it, so maybe it expects it already added. But it uses AddEntityFrameworkStores, 
            // which registers internal generic types, but doesn't instantly resolve DbContext.
            
            // We need to provide PaymobOptions for the PaymobClient
            services.Configure<Application.DTOs.PaymobDTOs.PaymobOptions>(options => 
            {
                options.BaseUrl = "https://accept.paymob.com/api";
            });

            var inMemorySettings = new Dictionary<string, string?> {
                {"Redis", "localhost:6379"},
                {"Gemini:ApiKey", "fake-api-key"},
                {"DefaultConnection", "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;"}
            };
            
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Act
            var result = await WebRegistrationService.AddWebsRegistrationServices(services, configuration);

            // Assert
            Assert.Same(services, result);
            Assert.NotEmpty(services);

            var serviceProvider = services.BuildServiceProvider();

            // Resolve delegates to increase coverage
            var geminiClient = serviceProvider.GetRequiredService<Google.GenAI.Client>();
            Assert.NotNull(geminiClient);

            // Resolve options that have delegates
            var redisOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.Extensions.Caching.StackExchangeRedis.RedisCacheOptions>>().Value;
            Assert.NotNull(redisOptions);

            var identityOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Identity.IdentityOptions>>().Value;
            Assert.True(identityOptions.User.RequireUniqueEmail);

            var authOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.Authorization.AuthorizationOptions>>().Value;
            Assert.NotNull(authOptions.GetPolicy("DashboardAccess"));
            
            var gzipOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>>().Value;
            Assert.NotNull(gzipOptions);

            var brotliOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>>().Value;
            Assert.NotNull(brotliOptions);

            var compressionOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.AspNetCore.ResponseCompression.ResponseCompressionOptions>>().Value;
            Assert.True(compressionOptions.EnableForHttps);

            var hybridOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<Microsoft.Extensions.Caching.Hybrid.HybridCacheOptions>>().Value;
            Assert.Equal(512, hybridOptions.MaximumKeyLength);

            // Resolve Resilience pipeline
            var resilienceProvider = serviceProvider.GetService<Polly.Registry.ResiliencePipelineProvider<string>>();
            if (resilienceProvider != null)
            {
                var storagePipeline = resilienceProvider.GetPipeline("storage-pipeline");
                Assert.NotNull(storagePipeline);
            }
            
            // Trigger PaymobClient creation
            var httpClientFactory = serviceProvider.GetRequiredService<System.Net.Http.IHttpClientFactory>();
            var paymobClient = httpClientFactory.CreateClient("PaymobClient");
            Assert.NotNull(paymobClient);
        }

        [Fact]
        public async Task AddWebsRegistrationServices_ThrowsIfGeminiKeyMissingOnResolve()
        {
            // Arrange
            var services = new ServiceCollection();
            var inMemorySettings = new Dictionary<string, string?> {
                // Gemini:ApiKey not set or empty
                {"Gemini:ApiKey", ""}
            };
            
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            await WebRegistrationService.AddWebsRegistrationServices(services, configuration);
            var serviceProvider = services.BuildServiceProvider();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => 
            {
                serviceProvider.GetRequiredService<Google.GenAI.Client>();
            });
            Assert.Contains("Gemini API Key is not configured", ex.Message);
        }
    }
}

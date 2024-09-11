using System.Net.Http;
using System.Threading.Tasks;
using ChatSupport.Queue;
using ChatSupport.Services.Interfaces;
using FluentAssertions;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using RabbitMQ.Client;
using Hangfire.MemoryStorage;
using Xunit;

namespace ChatSupport.Tests
{
    public class HealthCheckTests : IClassFixture<WebApplicationFactory<ChatSupport.Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<ChatSupport.Program> _factory;

        public HealthCheckTests(WebApplicationFactory<ChatSupport.Program> factory)
        {
            Mock<IMongoDatabase> mockMongoDatabase = new Mock<IMongoDatabase>();
            Mock<ISeedService> mockSeedService = new Mock<ISeedService>();
            var mockBackgroundJobClient = new Mock<IBackgroundJobClient>();

            mockSeedService.Setup(x => x.SeedData()).Returns(Task.CompletedTask);
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton<IMongoDatabase>(sp =>
                    {
                        return mockMongoDatabase.Object;
                    });
                    services.AddScoped<ISeedService>(sp =>
                    {
                        return mockSeedService.Object;
                    });
                    services.AddSingleton(mockBackgroundJobClient.Object);
                    services.AddHangfireMock();

                });
            });

            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task HealthCheck_ShouldReturnRunningMessage()
        {
            // Arrange
            var requestUri = "/";

            // Act
            var response = await _client.GetAsync(requestUri);
            response.EnsureSuccessStatusCode(); // Status Code 200-299

            var content = await response.Content.ReadAsStringAsync();

            // Assert
            content.Should().Contain("Healthy");
        }

        [Fact]
        public async Task IConnectionFactory_ShouldBeConfiguredFromAppSettings()
        {
            // Arrange
            // TODO: Mock the app settings

            // Act
            var scopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var provider = scope.ServiceProvider;
            var factory = provider.GetRequiredService<IConnectionFactory>();

            // Assert
            factory.Should().NotBeNull();
            ((ConnectionFactory)factory).HostName.Should().Be("localhost");
            factory.UserName.Should().Be("user");
            factory.Password.Should().Be("password");
        }
    }


    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireMock(this IServiceCollection services)
        {
            services.AddHangfire(config =>
            {
                config.UseMemoryStorage();
            });

            return services;
        }
    }
}
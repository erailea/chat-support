using System.Net.Http;
using System.Threading.Tasks;
using ChatSupport.Queue;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Xunit;

namespace ChatSupport.Tests
{
    public class HealthCheckTests : IClassFixture<WebApplicationFactory<ChatSupport.Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<ChatSupport.Program> _factory;

        public HealthCheckTests(WebApplicationFactory<ChatSupport.Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
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


}
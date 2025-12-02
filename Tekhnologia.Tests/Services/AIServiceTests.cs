using System;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Tekhnologia.Services;
using Xunit;

namespace Tekhnologia.Tests.Services
{
    public class AIServiceTests
    {
        [Fact]
        public void Constructor_ThrowsException_WhenApiKeyIsMissing()
        {
            // Arrange
            // Clear any environment variable API key
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);
            
            // Create mock configuration that returns null for OpenAI:ApiKey
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["OpenAI:ApiKey"]).Returns((string?)null);

            // Act
            Action act = () => new AIService(mockConfig.Object);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*API key missing*");
        }

        [Fact]
        public void Constructor_CreatesInstance_WhenApiKeyIsProvided()
        {
            // Arrange
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["OpenAI:ApiKey"]).Returns("test-api-key");

            // Act
            Action act = () => new AIService(mockConfig.Object);

            // Assert
            act.Should().NotThrow();
        }
    }
}

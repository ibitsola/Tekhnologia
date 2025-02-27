using System;
using FluentAssertions;
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
            // Clear any API key.
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", null);

            // Act
            Action act = () => new AIService();

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*API Key is missing*");
        }
    }
}

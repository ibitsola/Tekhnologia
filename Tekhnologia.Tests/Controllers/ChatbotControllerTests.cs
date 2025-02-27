using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tekhnologia.Controllers;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Tests.Controllers
{
    public class ChatbotControllerTests
    {
        private readonly Mock<IAIService> _aiServiceMock;
        private readonly ChatbotController _controller;

        public ChatbotControllerTests()
        {
            _aiServiceMock = new Mock<IAIService>();

            // Set up a fake user identity.
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUser")
            }, "TestAuth"));

            _controller = new ChatbotController(_aiServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
        }

        [Fact]
        public async Task GetBusinessCoaching_ReturnsOk_WithResponse()
        {
            // Arrange
            var requestDto = new ChatRequestDTO { Message = "How can I grow my business?" };
            string dummyResponse = "This is a dummy coaching response.";
            _aiServiceMock.Setup(s => s.GetBusinessCoachingResponse(requestDto.Message))
                        .ReturnsAsync(dummyResponse);

            // Act
            var result = await _controller.GetBusinessCoaching(requestDto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("An OK result should be returned when the call is successful");

            var responseDto = okResult!.Value as ChatbotResponseDTO;
            responseDto.Should().NotBeNull("The response should be of type ChatbotResponseDTO");
            responseDto!.Response.Should().Be(dummyResponse);
        }

    }
}

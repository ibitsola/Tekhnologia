using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tekhnologia.Controllers;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces;
using Xunit;

namespace Tekhnologia.Tests.Controllers
{
    public class VisionBoardControllerTests
    {
        private readonly Mock<IVisionBoardService> _visionBoardServiceMock;
        private readonly VisionBoardController _controller;

        public VisionBoardControllerTests()
        {
            _visionBoardServiceMock = new Mock<IVisionBoardService>();

            // Set up a fake user identity with a test user ID.
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUser")
            }, "TestAuth"));

            _controller = new VisionBoardController(_visionBoardServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
        }

        [Fact]
        public async Task CreateVisionBoardItem_ReturnsOk_WhenCreationSucceeds()
        {
            // Arrange
            var dto = new CreateVisionBoardItemDTO
            {
                ImageUrl = "http://example.com/image.jpg",
                Caption = "Test Caption"
            };

            var createdItem = new VisionBoardItem
            {
                VisionId = Guid.NewGuid(),
                UserId = "testUser",
                ImageUrl = dto.ImageUrl,
                Caption = dto.Caption,
                CreatedAt = DateTime.UtcNow
            };

            _visionBoardServiceMock.Setup(s => s.CreateVisionBoardItemAsync("testUser", dto))
                .ReturnsAsync(createdItem);

            // Act
            var result = await _controller.CreateVisionBoardItem(dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("the controller should return Ok when creation succeeds");

            // Extract properties from the anonymous object returned by the controller.
            var resultType = okResult.Value!.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull("the result should contain a 'message' property");
            var message = messageProp!.GetValue(okResult.Value) as string;
            message.Should().Be("Vision board item added successfully");

            var itemProp = resultType.GetProperty("item");
            itemProp.Should().NotBeNull("the result should contain an 'item' property");
            var returnedItem = itemProp!.GetValue(okResult.Value) as VisionBoardItem;
            returnedItem.Should().BeEquivalentTo(createdItem);
        }

        [Fact]
        public async Task GetUserVisionBoard_ReturnsOk_WithItems()
        {
            // Arrange
            var items = new List<VisionBoardItemDTO>
            {
                new VisionBoardItemDTO { VisionId = Guid.NewGuid(), ImageUrl = "http://example.com/1.jpg", Caption = "Caption 1", CreatedAt = DateTime.UtcNow },
                new VisionBoardItemDTO { VisionId = Guid.NewGuid(), ImageUrl = "http://example.com/2.jpg", Caption = "Caption 2", CreatedAt = DateTime.UtcNow }
            };

            _visionBoardServiceMock.Setup(s => s.GetUserVisionBoardAsync("testUser"))
                .ReturnsAsync(items);

            // Act
            var result = await _controller.GetUserVisionBoard();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(items);
        }

        [Fact]
        public async Task UpdateVisionBoardItem_ReturnsOk_WhenUpdateSucceeds()
        {
            // Arrange
            Guid visionId = Guid.NewGuid();
            var dto = new CreateVisionBoardItemDTO
            {
                ImageUrl = "http://example.com/new.jpg",
                Caption = "New Caption"
            };

            var updatedItem = new VisionBoardItem
            {
                VisionId = visionId,
                UserId = "testUser",
                ImageUrl = dto.ImageUrl,
                Caption = dto.Caption,
                CreatedAt = DateTime.UtcNow
            };

            _visionBoardServiceMock.Setup(s => s.UpdateVisionBoardItemAsync(visionId, dto, "testUser"))
                .ReturnsAsync((true, string.Empty, updatedItem));

            // Act
            var result = await _controller.UpdateVisionBoardItem(visionId, dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var resultType = okResult.Value!.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull();
            var message = messageProp!.GetValue(okResult.Value) as string;
            message.Should().Be("Vision board item updated successfully");

            var itemProp = resultType.GetProperty("item");
            itemProp.Should().NotBeNull();
            var returnedItem = itemProp!.GetValue(okResult.Value) as VisionBoardItem;
            returnedItem.Should().BeEquivalentTo(updatedItem);
        }

        [Fact]
        public async Task DeleteVisionBoardItem_ReturnsOk_WhenDeletionSucceeds()
        {
            // Arrange
            Guid visionId = Guid.NewGuid();
            _visionBoardServiceMock.Setup(s => s.DeleteVisionBoardItemAsync(visionId, "testUser"))
                .ReturnsAsync((true, string.Empty));

            // Act
            var result = await _controller.DeleteVisionBoardItem(visionId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var resultType = okResult.Value!.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull();
            var message = messageProp!.GetValue(okResult.Value) as string;
            message.Should().Be("Vision board item deleted successfully");
        }
    }
}

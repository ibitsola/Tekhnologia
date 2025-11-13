using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Tekhnologia.Controllers;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces;
using Xunit;

namespace Tekhnologia.Tests.Controllers
{
    public class AdminControllerTests
    {
        private readonly Mock<IAdminService> _adminServiceMock;
        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _adminServiceMock = new Mock<IAdminService>();

            // Set up a fake admin identity.
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "admin1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuth"));

            _controller = new AdminController(_adminServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
        }

        [Fact]
        public async Task GetAllUsers_ReturnsOk_WithSortedUsers()
        {
            // Arrange
            var users = new List<object>
            {
                new { Id = "1", Name = "Alice", Email = "alice@example.com", Role = "User", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
                new { Id = "2", Name = "Bob", Email = "bob@example.com", Role = "Admin", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
            };

            _adminServiceMock.Setup(s => s.GetAllUsersAsync())
                .ReturnsAsync(users);

            // Act
            var result = await _controller.GetAllUsers();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(users);
        }

        [Fact]
        public async Task GetUserById_ReturnsOk_WhenUserFound()
        {
            // Arrange
            var user = new User
            {
                Id = "1",
                Name = "Alice",
                Email = "alice@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _adminServiceMock.Setup(s => s.GetUserByIdAsync("1"))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.GetUserById("1");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult!.Value;
            response.Should().NotBeNull("The response object should not be null");

            // Extract properties via reflection and use null-coalescing operators.
            var type = response.GetType();

            var idProp = type.GetProperty("Id") ?? throw new Exception("Property 'Id' not found");
            var nameProp = type.GetProperty("Name") ?? throw new Exception("Property 'Name' not found");
            var emailProp = type.GetProperty("Email") ?? throw new Exception("Property 'Email' not found");

            string id = idProp.GetValue(response)?.ToString() ?? string.Empty;
            string name = nameProp.GetValue(response)?.ToString() ?? string.Empty;
            string email = emailProp.GetValue(response)?.ToString() ?? string.Empty;

            id.Should().Be("1");
            name.Should().Be("Alice");
            email.Should().Be("alice@example.com");
        }

        [Fact]
        public async Task GetUserById_ReturnsNotFound_WhenUserNotFound()
        {
            // Arrange
            _adminServiceMock.Setup(s => s.GetUserByIdAsync("nonexistent"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _controller.GetUserById("nonexistent");

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.Value.Should().Be("User not found");
        }

        [Fact]
        public async Task UpdateUser_ReturnsOk_WhenUpdateSucceeds()
        {
            // Arrange
            var updateDto = new UpdateUserDTO { Name = "Alice Updated", Email = "alice.updated@example.com" };
            var updatedUser = new User
            {
                Id = "1",
                Name = "Alice Updated",
                Email = "alice.updated@example.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _adminServiceMock.Setup(s => s.UpdateUserAsync("1", updateDto))
                .ReturnsAsync((true, Array.Empty<string>(), updatedUser));

            // Act
            var result = await _controller.UpdateUser("1", updateDto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult!.Value;
            response.Should().NotBeNull("The response object should not be null");

            var type = response.GetType();

            var messageProp = type.GetProperty("message") ?? throw new Exception("Property 'message' not found");
            string message = messageProp.GetValue(response)?.ToString() ?? string.Empty;
            message.Should().Contain("updated successfully");

            var userProp = type.GetProperty("user") ?? throw new Exception("Property 'user' not found");
            var returnedUser = userProp.GetValue(response) as User;
            returnedUser.Should().BeEquivalentTo(updatedUser);
        }

        [Fact]
        public async Task UpdateUser_ReturnsBadRequest_WhenUpdateFails()
        {
            // Arrange
            var updateDto = new UpdateUserDTO { Name = "Alice Updated", Email = "alice.updated@example.com" };
            var errors = new List<string> { "User not found" };

            _adminServiceMock.Setup(s => s.UpdateUserAsync("nonexistent", updateDto))
                .ReturnsAsync((false, errors, (User?)null));

            // Act
            var result = await _controller.UpdateUser("nonexistent", updateDto);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task PromoteToAdmin_ReturnsOk_WhenPromotionSucceeds()
        {
            // Arrange
            _adminServiceMock.Setup(s => s.PromoteToAdminAsync("1"))
                .ReturnsAsync((true, Array.Empty<string>()));

            // Act
            var result = await _controller.PromoteToAdmin("1");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult!.Value;
            response.Should().NotBeNull("The response object should not be null");

            var type = response.GetType();
            var messageProp = type.GetProperty("message") ?? throw new Exception("Property 'message' not found");
            string message = messageProp.GetValue(response)?.ToString() ?? string.Empty;
            message.Should().Be("User has been promoted to Admin!");
        }

        [Fact]
        public async Task PromoteToAdmin_ReturnsBadRequest_WhenPromotionFails()
        {
            // Arrange
            var errors = new List<string> { "User is already an Admin" };
            _adminServiceMock.Setup(s => s.PromoteToAdminAsync("1"))
                .ReturnsAsync((false, errors));

            // Act
            var result = await _controller.PromoteToAdmin("1");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().BeEquivalentTo(errors);
        }

        [Fact]
        public async Task DeleteUser_ReturnsOk_WhenDeletionSucceeds()
        {
            // Arrange
            _adminServiceMock.Setup(s => s.DeleteUserAsync("1"))
                .ReturnsAsync((true, Array.Empty<string>()));

            // Act
            var result = await _controller.DeleteUser("1");

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var response = okResult!.Value;
            response.Should().NotBeNull("The response object should not be null");

            var type = response.GetType();
            var messageProp = type.GetProperty("message") ?? throw new Exception("Property 'message' not found");
            string message = messageProp.GetValue(response)?.ToString() ?? string.Empty;
            message.Should().Be("User deleted successfully");
        }

        [Fact]
        public async Task DeleteUser_ReturnsForbidden_WhenUserCannotBeDeleted()
        {
            // Arrange
            var errors = new List<string> { "This user cannot be deleted." };
            _adminServiceMock.Setup(s => s.DeleteUserAsync("protectedId"))
                .ReturnsAsync((false, errors));

            // Act
            var result = await _controller.DeleteUser("protectedId");

            // Assert
            var objectResult = result as ObjectResult;
            objectResult.Should().NotBeNull();
            objectResult!.StatusCode.Should().Be(403);

            var response = objectResult!.Value;
            response.Should().NotBeNull("The response object should not be null");

            var type = response.GetType();
            var messageProp = type.GetProperty("message") ?? throw new Exception("Property 'message' not found");
            string message = messageProp.GetValue(response)?.ToString() ?? string.Empty;
            message.Should().Be("This user cannot be deleted.");
        }

        [Fact]
        public async Task DeleteUser_ReturnsBadRequest_WhenDeletionFails()
        {
            // Arrange
            var errors = new List<string> { "User not found" };
            _adminServiceMock.Setup(s => s.DeleteUserAsync("nonexistent"))
                .ReturnsAsync((false, errors));

            // Act
            var result = await _controller.DeleteUser("nonexistent");

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.Value.Should().BeEquivalentTo(errors);
        }
    }
}

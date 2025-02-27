using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tekhnologia.Controllers;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces;
using Xunit;

namespace Tekhnologia.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();

            // Set up a fake user identity with both NameIdentifier and Name
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUser"),
                new Claim(ClaimTypes.Name, "testUser")
            }, "TestAuth"));

            _controller = new UserController(_userServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
        }

        [Fact]
        public async Task UpdatePassword_ReturnsOk_WhenPasswordUpdated()
        {
            // Arrange
            var updatePasswordDto = new UpdatePasswordDTO
            {
                OldPassword = "OldPass123",
                NewPassword = "NewPass456"
            };

            // Setup the mock to return a success tuple.
            _userServiceMock.Setup(s => s.UpdateUserPasswordAsync("testUser", updatePasswordDto))
                .ReturnsAsync((true, new string[0]));

            // Act
            var result = await _controller.UpdatePassword(updatePasswordDto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("an OK result should be returned when the password is updated");
            
            // Since our controller returns an anonymous object, use reflection to extract the "message" property.
            var resultType = okResult.Value!.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull("the returned object should contain a 'message' property");
            var message = messageProp!.GetValue(okResult.Value) as string;
            message.Should().Be("Password updated successfully. Please log in again.");
        }

        [Fact]
        public async Task UpdateMyProfile_ReturnsOk_WhenUpdateSucceeds()
        {
            // Arrange
            var updateUserDto = new UpdateUserDTO
            {
                Name = "Updated Name",
                Email = "updated@example.com"
            };

            var updatedUser = new Models.User
            {
                Id = "testUser",
                Name = "Updated Name",
                Email = "updated@example.com"
            };

            _userServiceMock.Setup(s => s.UpdateUserProfileAsync("testUser", updateUserDto))
                .ReturnsAsync((true, new string[0], updatedUser));

            // Act
            var result = await _controller.UpdateMyProfile(updateUserDto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("an OK result should be returned when the profile is updated");
            
            var resultType = okResult.Value!.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull("the returned object should contain a 'message' property");
            var message = messageProp!.GetValue(okResult.Value) as string;
            message.Should().Be("Profile updated successfully");
        }
    }
}

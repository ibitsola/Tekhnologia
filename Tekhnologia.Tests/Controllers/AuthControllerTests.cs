using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Tekhnologia.Controllers;
using Tekhnologia.Models;
using Tekhnologia.Services.Interfaces;
using Tekhnologia.Tests.Fakes;
using Xunit;

namespace Tekhnologia.Tests.Controllers
{
    public class AuthControllerTests
    {
        [Fact]
        public async Task Register_ReturnsOk_WhenRegistrationSucceeds()
        {
            // Arrange
            var registerModel = new RegisterModel
            {
                Email = "newuser@example.com",
                Password = "Password123!",
                Name = "New User"
            };

            // Use the fake auth service.
            IAuthService fakeAuthService = new FakeAuthService();
            var controller = new AuthController(fakeAuthService);

            // Act
            var result = await controller.Register(registerModel);

            // Assert
            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new { message = "User registered successfully" });
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenLoginSucceeds()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Email = "test@example.com",
                Password = "Test@123"
            };

            IAuthService fakeAuthService = new FakeAuthService();
            var controller = new AuthController(fakeAuthService);

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("an OK result should be returned when login succeeds");

            // The controller returns an anonymous object with a property "Token".
            okResult.Value.Should().BeEquivalentTo(new { Token = "dummy_jwt_token" });

        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenLoginFails()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword"
            };

            IAuthService fakeAuthService = new FakeAuthService();
            var controller = new AuthController(fakeAuthService);

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            result.Should().BeOfType<UnauthorizedObjectResult>()
                  .Which.Value.Should().Be("Invalid credentials");
        }
    }
}

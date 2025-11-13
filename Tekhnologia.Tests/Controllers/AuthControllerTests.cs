using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tekhnologia.Controllers;
using Tekhnologia.Models;

namespace Tekhnologia.Tests.Controllers
{
    public class AuthControllerTests
    {
        private static Mock<UserManager<User>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        private static Mock<SignInManager<User>> GetMockSignInManager(UserManager<User> userManager)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            return new Mock<SignInManager<User>>(
                userManager, contextAccessor.Object, claimsFactory.Object,
                null!, null!, null!, null!);
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

            var testUser = new User { Email = loginModel.Email, UserName = loginModel.Email };

            var userManagerMock = GetMockUserManager();
            var signInManagerMock = GetMockSignInManager(userManagerMock.Object);

            userManagerMock.Setup(m => m.FindByEmailAsync(loginModel.Email))
                        .ReturnsAsync(testUser);
            userManagerMock.Setup(m => m.CheckPasswordAsync(testUser, loginModel.Password))
                        .ReturnsAsync(true);
            signInManagerMock.Setup(m => m.SignInAsync(testUser, false, null))
                            .Returns(Task.CompletedTask);

            var controller = new AuthController(signInManagerMock.Object, userManagerMock.Object);

            // Act
            var result = await controller.Login(loginModel);

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenLoginFails()
        {
            var loginModel = new LoginModel
            {
                Email = "wrong@example.com",
                Password = "WrongPassword"
            };

            var userManagerMock = GetMockUserManager();
            var signInManagerMock = GetMockSignInManager(userManagerMock.Object);

            signInManagerMock.Setup(m => m.PasswordSignInAsync(
                loginModel.Email, loginModel.Password,
                false, false)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var controller = new AuthController(signInManagerMock.Object, userManagerMock.Object);

            var result = await controller.Login(loginModel);

            result.Should().BeOfType<UnauthorizedObjectResult>()
                  .Which.Value.Should().Be("Invalid email or password.");
        }

        [Fact]
        public async Task Register_ReturnsOk_WhenUserCreated()
        {
            var registerModel = new RegisterModel
            {
                Email = "test@example.com",
                Password = "Test@123",
                Name = "Test User"
            };

            var userManagerMock = GetMockUserManager();
            var signInManagerMock = GetMockSignInManager(userManagerMock.Object);

            userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), registerModel.Password))
                           .ReturnsAsync(IdentityResult.Success);

            var controller = new AuthController(signInManagerMock.Object, userManagerMock.Object);

            var result = await controller.Register(registerModel);

            result.Should().BeOfType<OkObjectResult>()
                  .Which.Value.Should().BeEquivalentTo(new { message = "User registered successfully" });
        }
    }
}

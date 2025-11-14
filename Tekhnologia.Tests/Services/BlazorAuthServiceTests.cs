using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Tekhnologia.Models;
using Tekhnologia.Services;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;

namespace Tekhnologia.Tests.Services
{
    public class BlazorAuthServiceTests
    {
        private Mock<UserManager<User>> CreateMockUserManager()
        {
            var store = new Mock<Microsoft.AspNetCore.Identity.IUserStore<User>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var passwordHasher = new Mock<IPasswordHasher<User>>();
            var userValidators = new List<IUserValidator<User>>();
            var pwdValidators = new List<IPasswordValidator<User>>();
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<User>>>();

            var mgr = new Mock<UserManager<User>>(store.Object, options.Object, passwordHasher.Object, userValidators, pwdValidators, keyNormalizer.Object, errors.Object, services.Object, logger.Object);
            return mgr;
        }

        private Mock<SignInManager<User>> CreateMockSignInManager(Mock<UserManager<User>> userManagerMock)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<User>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var logger = new Mock<ILogger<SignInManager<User>>>();
            var schemes = new Mock<IAuthenticationSchemeProvider>();
            var confirmation = new Mock<IUserConfirmation<User>>();

            var signInMgr = new Mock<SignInManager<User>>(userManagerMock.Object, contextAccessor.Object, claimsFactory.Object, options.Object, logger.Object, schemes.Object, confirmation.Object);
            return signInMgr;
        }

        [Fact]
        public async Task LoginAsync_ReturnsEmail_WhenLoginSucceeds()
        {
            // Arrange
            // Arrange
            var user = new User { Email = "user@example.com" };
            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            userManagerMock.Setup(m => m.CheckPasswordAsync(user, "ValidPassword")).ReturnsAsync(true);

            var signInMock = CreateMockSignInManager(userManagerMock);
            signInMock.Setup(s => s.SignInAsync(It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<string?>())).Returns(Task.CompletedTask);

            var service = new BlazorAuthService(userManagerMock.Object, signInMock.Object);

            var loginModel = new LoginModel { Email = "user@example.com", Password = "ValidPassword" };

            // Act
            var result = await service.LoginAsync(loginModel);

            // Assert
            result.Should().Be(loginModel.Email);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenLoginFails()
        {
            // Arrange
            // Arrange
            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(m => m.FindByEmailAsync("wrong@example.com")).ReturnsAsync((User?)null);

            var signInMock = CreateMockSignInManager(userManagerMock);
            var service = new BlazorAuthService(userManagerMock.Object, signInMock.Object);

            var loginModel = new LoginModel { Email = "wrong@example.com", Password = "WrongPassword" };

            // Act
            var result = await service.LoginAsync(loginModel);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_ReturnsSuccess_WhenServerResponds200()
        {
            // Arrange
            // Arrange
            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), "StrongPass123!")).ReturnsAsync(IdentityResult.Success);

            var signInMock = CreateMockSignInManager(userManagerMock);
            var service = new BlazorAuthService(userManagerMock.Object, signInMock.Object);

            var registerModel = new RegisterModel { Email = "new@example.com", Password = "StrongPass123!", Name = "New User" };

            // Act
            var result = await service.RegisterAsync(registerModel);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WhenServerResponds400WithErrors()
        {
            // Arrange
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var identityError = new IdentityError { Description = "Email already taken" };
            userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), "Pass123!")).ReturnsAsync(IdentityResult.Failed(identityError));

            var signInMock = CreateMockSignInManager(userManagerMock);
            var service = new BlazorAuthService(userManagerMock.Object, signInMock.Object);

            var registerModel = new RegisterModel { Email = "existing@example.com", Password = "Pass123!", Name = "Existing User" };

            // Act
            var result = await service.RegisterAsync(registerModel);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Description == "Email already taken");
        }
    }
}

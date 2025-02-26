using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Tekhnologia.Services;
using Tekhnologia.Models;


namespace Tekhnologia.Tests.Services
{
    public class AuthServiceTests
    {
        // Helper method to create the AuthService with provided mocks and configuration
        private AuthService CreateAuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration)
        {
            return new AuthService(userManager, signInManager, configuration);
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnSuccess_WhenUserIsCreated()
        {
            // Arrange
            var registerModel = new RegisterModel
            {
                Email = "test@example.com",
                Password = "Test@123",
                Name = "Test User"
            };

            // Create a mock UserStore and UserManager.
            // Using null! (null-forgiving operator) for unused parameters.
            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock
                .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Setup a mock SignInManager (not used in registration)
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var signInManagerMock = new Mock<SignInManager<User>>(
                userManagerMock.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                null!, null!, null!, null!);

            // Setup in-memory configuration for JWT settings using a dictionary with nullable values.
            var inMemorySettings = new Dictionary<string, string?> 
            {
                {"Jwt:Secret", "super_secret_key_for_testing_which_is_long_enough"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var authService = CreateAuthService(userManagerMock.Object, signInManagerMock.Object, configuration);

            // Act
            var result = await authService.RegisterAsync(registerModel);

            // Assert
            result.Succeeded.Should().BeTrue();
            userManagerMock.Verify(x =>
                x.CreateAsync(
                    It.Is<User>(u => u.Email == registerModel.Email && u.UserName == registerModel.Email),
                    registerModel.Password),
                Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Email = "test@example.com",
                Password = "Test@123"
            };

            // Create a dummy user that the mocks will return.
            var dummyUser = new User
            {
                Id = "dummyId",
                Email = "test@example.com",
                UserName = "test@example.com",
                Name = "Test User"
            };

            // Setup mocks for UserManager.
            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(x => x.FindByEmailAsync(loginModel.Email))
                           .ReturnsAsync(dummyUser);
            userManagerMock.Setup(x => x.GetRolesAsync(dummyUser))
                           .ReturnsAsync(new List<string>()); // No roles for simplicity

            // Setup mock for SignInManager to simulate successful login.
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var signInManagerMock = new Mock<SignInManager<User>>(
                userManagerMock.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                null!, null!, null!, null!);
            signInManagerMock.Setup(x => x.PasswordSignInAsync(dummyUser.UserName, loginModel.Password, false, false))
                             .ReturnsAsync(SignInResult.Success);

            // Setup in-memory configuration for JWT.
            var inMemorySettings = new Dictionary<string, string?> 
            {
                {"Jwt:Secret", "super_secret_key_for_testing_which_is_long_enough"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var authService = CreateAuthService(userManagerMock.Object, signInManagerMock.Object, configuration);

            // Act
            var token = await authService.LoginAsync(loginModel);

            // Assert
            token.Should().NotBeNullOrEmpty();
            // Verify that the token is a valid JWT token.
            var handler = new JwtSecurityTokenHandler();
            handler.CanReadToken(token).Should().BeTrue();
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnNull_WhenUserNotFound()
        {
            // Arrange
            var loginModel = new LoginModel
            {
                Email = "nonexistent@example.com",
                Password = "WrongPassword"
            };

            // Setup mocks so that FindByEmailAsync returns null.
            var userStoreMock = new Mock<IUserStore<User>>();
            var userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
            userManagerMock.Setup(x => x.FindByEmailAsync(loginModel.Email))
                           .ReturnsAsync((User?)null);

            // Setup a mock for SignInManager.
            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            var signInManagerMock = new Mock<SignInManager<User>>(
                userManagerMock.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                null!, null!, null!, null!);

            // Setup in-memory configuration for JWT.
            var inMemorySettings = new Dictionary<string, string?> 
            {
                {"Jwt:Secret", "super_secret_key_for_testing_which_is_long_enough"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var authService = CreateAuthService(userManagerMock.Object, signInManagerMock.Object, configuration);

            // Act
            var token = await authService.LoginAsync(loginModel);

            // Assert
            token.Should().BeNull();
        }
    }
}

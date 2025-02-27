using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services;
using Tekhnologia.Services.Interfaces;
using Microsoft.AspNetCore.Http;


namespace Tekhnologia.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            // Create mocks for UserManager and SignInManager.
           var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, 
                null!, 
                null!, 
                null!, 
                null!, 
                null!, 
                null!, 
                null!, 
                null!);

            var contextAccessorMock = new Mock<IHttpContextAccessor>();
            var userPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object, 
                contextAccessorMock.Object, 
                userPrincipalFactoryMock.Object, 
                null!, 
                null!, 
                null!, 
                null!);
            _userService = new UserService(_userManagerMock.Object, _signInManagerMock.Object);
        }

        [Fact]
        public async Task GetUserProfileAsync_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            var testUser = new User { Id = "user123", Email = "test@example.com", Name = "Test User" };
            _userManagerMock.Setup(um => um.FindByIdAsync("user123"))
                .ReturnsAsync(testUser);

            // Act
            var result = await _userService.GetUserProfileAsync("user123");

            // Assert
            result.Should().BeEquivalentTo(testUser);
        }

        [Fact]
        public async Task UpdateUserProfileAsync_ShouldReturnSuccess_WhenUpdateSucceeds()
        {
            // Arrange
            var testUser = new User { Id = "user123", Email = "old@example.com", Name = "Old Name" };
            var updateDto = new UpdateUserDTO { Name = "New Name", Email = "new@example.com" };

            _userManagerMock.Setup(um => um.FindByIdAsync("user123"))
                .ReturnsAsync(testUser);
            _userManagerMock.Setup(um => um.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var (success, errors, updatedUser) = await _userService.UpdateUserProfileAsync("user123", updateDto);

            // Assert
            success.Should().BeTrue();
            errors.Should().BeEmpty();
            updatedUser.Should().NotBeNull();
            updatedUser!.Name.Should().Be("New Name");
            updatedUser.Email.Should().Be("new@example.com");
        }

        [Fact]
        public async Task UpdateUserPasswordAsync_ShouldReturnSuccess_WhenPasswordChanged()
        {
            // Arrange
            var testUser = new User { Id = "user123", Email = "test@example.com", Name = "Test User" };
            var updatePasswordDto = new UpdatePasswordDTO
            {
                OldPassword = "oldPass",
                NewPassword = "newPass"
            };

            _userManagerMock.Setup(um => um.FindByIdAsync("user123"))
                .ReturnsAsync(testUser);
            _userManagerMock.Setup(um => um.CheckPasswordAsync(testUser, "oldPass"))
                .ReturnsAsync(true);
            _userManagerMock.Setup(um => um.ChangePasswordAsync(testUser, "oldPass", "newPass"))
                .ReturnsAsync(IdentityResult.Success);
            _signInManagerMock.Setup(sm => sm.SignOutAsync())
                .Returns(Task.CompletedTask);

            // Act
            var (success, errors) = await _userService.UpdateUserPasswordAsync("user123", updatePasswordDto);

            // Assert
            success.Should().BeTrue();
            errors.Should().BeEmpty();
        }
    }
}

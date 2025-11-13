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

namespace Tekhnologia.Tests.Services
{
    public class BlazorAuthServiceTests
    {
        private HttpClient CreateMockHttpClient(HttpResponseMessage response)
        {
            var handlerMock = new Mock<HttpMessageHandler>();

            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://localhost")
            };
        }

        [Fact]
        public async Task LoginAsync_ReturnsEmail_WhenLoginSucceeds()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var httpClient = CreateMockHttpClient(response);
            var service = new BlazorAuthService(httpClient);

            var loginModel = new LoginModel
            {
                Email = "user@example.com",
                Password = "ValidPassword"
            };

            // Act
            var result = await service.LoginAsync(loginModel);

            // Assert
            result.Should().Be(loginModel.Email);
        }

        [Fact]
        public async Task LoginAsync_ReturnsNull_WhenLoginFails()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            var httpClient = CreateMockHttpClient(response);
            var service = new BlazorAuthService(httpClient);

            var loginModel = new LoginModel
            {
                Email = "wrong@example.com",
                Password = "WrongPassword"
            };

            // Act
            var result = await service.LoginAsync(loginModel);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task RegisterAsync_ReturnsSuccess_WhenServerResponds200()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            var httpClient = CreateMockHttpClient(response);
            var service = new BlazorAuthService(httpClient);

            var registerModel = new RegisterModel
            {
                Email = "new@example.com",
                Password = "StrongPass123!",
                Name = "New User"
            };

            // Act
            var result = await service.RegisterAsync(registerModel);

            // Assert
            result.Succeeded.Should().BeTrue();
        }

        [Fact]
        public async Task RegisterAsync_ReturnsFailure_WhenServerResponds400WithErrors()
        {
            // Arrange
            var errorJson = JsonSerializer.Serialize(new List<IdentityError>
            {
                new IdentityError { Description = "Email already taken" }
            });

            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorJson)
            };

            var httpClient = CreateMockHttpClient(response);
            var service = new BlazorAuthService(httpClient);

            var registerModel = new RegisterModel
            {
                Email = "existing@example.com",
                Password = "Pass123!",
                Name = "Existing User"
            };

            // Act
            var result = await service.RegisterAsync(registerModel);

            // Assert
            result.Succeeded.Should().BeFalse();
            result.Errors.Should().Contain(e => e.Description == "Email already taken");
        }
    }
}

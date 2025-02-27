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

namespace Tekhnologia.Tests.Controllers
{
    public class DigitalResourceControllerTest
    {
        private readonly Mock<IDigitalResourceService> _digitalResourceServiceMock;
        private readonly DigitalResourceController _controller;

        public DigitalResourceControllerTest()
        {
            _digitalResourceServiceMock = new Mock<IDigitalResourceService>();

            // Set up a fake user identity with both NameIdentifier and Name claims.
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUser"),
                new Claim(ClaimTypes.Name, "testUser")
            }, "TestAuth"));

            _controller = new DigitalResourceController(_digitalResourceServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
        }


        [Fact]
        public void GetAllResources_ReturnsOk_WithResources()
        {
            // Arrange
            var resources = new List<DigitalResourceDTO>
            {
                new DigitalResourceDTO { Id = 1, Title = "Resource 1", FileType = "pdf", Category = "Category1", IsFree = true, Price = null, FilePath = "/digital-resources/file1.pdf", UploadDate = DateTime.UtcNow },
                new DigitalResourceDTO { Id = 2, Title = "Resource 2", FileType = "pdf", Category = "Category2", IsFree = false, Price = 9.99m, FilePath = "/digital-resources/file2.pdf", UploadDate = DateTime.UtcNow }
            };

            _digitalResourceServiceMock.Setup(s => s.GetAllResources(null, null))
                .Returns(resources);

            // Act
            var result = _controller.GetAllResources(null, null);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(resources);
        }

        [Fact]
        public async Task UploadResource_ReturnsOk_WhenUploadSucceeds()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.Length).Returns(1024);
            fileMock.Setup(f => f.FileName).Returns("test.pdf");

            var dto = new CreateDigitalResourceDTO
            {
                Title = "Test Resource",
                File = fileMock.Object,
                Category = "TestCategory",
                IsFree = true,
                Price = null
            };

            var newResource = new DigitalResource
            {
                Id = 1,
                Title = dto.Title,
                FileName = "unique_test.pdf",
                FilePath = "/digital-resources/unique_test.pdf",
                FileType = "pdf",
                Category = dto.Category,
                IsFree = dto.IsFree,
                Price = dto.Price,
                UploadDate = DateTime.UtcNow,
                UploadedBy = "testUser"
            };

            _digitalResourceServiceMock.Setup(s => s.UploadResourceAsync(dto, "testUser"))
                .ReturnsAsync(newResource);

            // Act
            var result = await _controller.UploadResource(dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("the controller should return an OkObjectResult when upload succeeds");
            okResult!.Value.Should().NotBeNull("the returned object should not be null");

            // Use reflection to extract properties from the anonymous object.
            var resultType = okResult.Value.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull("the returned object should contain a 'message' property");
            var message = messageProp!.GetValue(okResult.Value) as string;
            message.Should().Be("Resource uploaded successfully!");

            var newResourceProp = resultType.GetProperty("newResource");
            newResourceProp.Should().NotBeNull("the returned object should contain a 'newResource' property");
            var returnedResource = newResourceProp!.GetValue(okResult.Value) as DigitalResource;
            returnedResource.Should().BeEquivalentTo(newResource);
        }

        [Fact]
        public void DownloadResource_ReturnsFile_WhenSuccessful()
        {
            // Arrange
            int resourceId = 1;
            var userId = "testUser";
            var resource = new DigitalResource
            {
                Id = resourceId,
                FileName = "file.pdf"
            };
            byte[] fileBytes = new byte[] { 0, 1, 2, 3 };

            _digitalResourceServiceMock.Setup(s => s.DownloadResource(resourceId, userId))
                .Returns((resource, fileBytes));

            // Act
            var result = _controller.DownloadResource(resourceId);

            // Assert
            var fileResult = result as FileContentResult;
            fileResult.Should().NotBeNull();
            fileResult!.FileContents.Should().BeEquivalentTo(fileBytes);
            fileResult.FileDownloadName.Should().Be(resource.FileName);
        }

        [Fact]
        public async Task DeleteResource_ReturnsOk_WhenDeletionSucceeds()
        {
            // Arrange
            int resourceId = 1;
            _digitalResourceServiceMock.Setup(s => s.DeleteResourceAsync(resourceId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteResource(resourceId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().Be("Resource deleted successfully.");
        }

        [Fact]
        public async Task EditResource_ReturnsOk_WhenEditSucceeds()
        {
            // Arrange
            int resourceId = 1;
            var updatedDTO = new DigitalResourceDTO
            {
                Id = resourceId,
                Title = "Updated Title",
                FileType = "pdf",
                Category = "UpdatedCategory",
                IsFree = false,
                Price = 5.99m,
                FilePath = "/digital-resources/updated.pdf",
                UploadDate = DateTime.UtcNow
            };

            _digitalResourceServiceMock.Setup(s => s.EditResourceAsync(resourceId, updatedDTO))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.EditResource(resourceId, updatedDTO);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().Be("Resource updated successfully.");
        }
    }
}

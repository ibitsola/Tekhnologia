using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services;
using Microsoft.AspNetCore.Hosting;

namespace Tekhnologia.Tests.Services
{
    public class DigitalResourceServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DigitalResourceService _service;
        private readonly string _tempWebRoot;
        private readonly Mock<IWebHostEnvironment> _envMock;

        public DigitalResourceServiceTests()
        {
            // Create a temporary folder to simulate the web root.
            _tempWebRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempWebRoot);

            // Create a unique in‑memory database for isolation.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Set up the web environment mock to return the temporary folder as the WebRootPath.
            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(env => env.WebRootPath).Returns(_tempWebRoot);

            _service = new DigitalResourceService(_context, _envMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            if (Directory.Exists(_tempWebRoot))
            {
                Directory.Delete(_tempWebRoot, true);
            }
        }

        [Fact]
        public void GetAllResources_ShouldReturnFilteredResults()
        {
            // Arrange
            _context.DigitalResources.AddRange(
                new DigitalResource
                {
                    Title = "Resource 1",
                    FileType = "pdf",
                    Category = "Category1",
                    IsFree = true,
                    Price = null,
                    FilePath = "/digital-resources/resource1.pdf",
                    UploadDate = DateTime.UtcNow,
                    UploadedBy = "Uploader1"
                },
                new DigitalResource
                {
                    Title = "Resource 2",
                    FileType = "pdf",
                    Category = "Category2",
                    IsFree = false,
                    Price = 9.99m,
                    FilePath = "/digital-resources/resource2.pdf",
                    UploadDate = DateTime.UtcNow,
                    UploadedBy = "Uploader2"
                }
            );
            _context.SaveChanges();

            // Act
            var freeResources = _service.GetAllResources(null, true);
            var category2Resources = _service.GetAllResources("Category2", null);

            // Assert
            freeResources.Should().HaveCount(1);
            freeResources.First().Title.Should().Be("Resource 1");
            category2Resources.Should().HaveCount(1);
            category2Resources.First().Title.Should().Be("Resource 2");
        }

        [Fact]
        public async Task UploadResourceAsync_ShouldSaveFileAndRecord()
        {
            // Arrange: Create a fake IFormFile.
            var fileContent = "This is a test file.";
            var fileName = "testfile.txt";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(fileContent));
            ms.Position = 0;
            var formFile = new FormFile(ms, 0, ms.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var dto = new CreateDigitalResourceDTO
            {
                Title = "Test Resource",
                File = formFile,
                Category = "TestCategory",
                IsFree = false,
                Price = 5.99m
            };

            string uploaderName = "Tester";

            // Act
            var resource = await _service.UploadResourceAsync(dto, uploaderName);

            // Assert: Check DB record and that file exists in _tempWebRoot/digital-resources.
            resource.Should().NotBeNull();
            resource.Title.Should().Be("Test Resource");
            resource.Category.Should().Be("TestCategory");
            resource.IsFree.Should().BeFalse();
            resource.Price.Should().Be(5.99m);
            resource.UploadedBy.Should().Be(uploaderName);
            // The file path is set as "/digital-resources/{uniqueFileName}"
            string expectedFolder = Path.Combine(_tempWebRoot, "digital-resources");
            string fullPath = Path.Combine(expectedFolder, resource.FileName);
            File.Exists(fullPath).Should().BeTrue();

            // Verify file contents.
            var savedContent = File.ReadAllText(fullPath);
            savedContent.Should().Be(fileContent);
        }

        [Fact]
        public void DownloadResource_ShouldReturnFileBytes_WhenResourceIsFree()
        {
            // Arrange: Insert a free resource record and create its file.
            var resource = new DigitalResource
            {
                Title = "Free Resource",
                FileType = "pdf",
                Category = "Docs",
                IsFree = true,
                Price = null,
                FileName = "free_resource.pdf",
                FilePath = "/digital-resources/free_resource.pdf",
                UploadDate = DateTime.UtcNow,
                UploadedBy = "Uploader"
            };
            _context.DigitalResources.Add(resource);
            _context.SaveChanges();

            // Create the file in the digital-resources folder.
            string folder = Path.Combine(_tempWebRoot, "digital-resources");
            Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, resource.FileName);
            string fileContent = "Dummy PDF content";
            File.WriteAllText(fullPath, fileContent);

            // Act
            var (returnedResource, fileBytes) = _service.DownloadResource(resource.Id, "anyUser");

            // Assert
            returnedResource.Should().NotBeNull();
            returnedResource.Id.Should().Be(resource.Id);
            fileBytes.Should().NotBeNull();
            Encoding.UTF8.GetString(fileBytes).Should().Be(fileContent);
        }

        [Fact]
        public void DownloadResource_ShouldThrowException_WhenResourceNotFound()
        {
            // Act
            Action act = () => _service.DownloadResource(999, "user");

            // Assert
            act.Should().Throw<Exception>().WithMessage("Resource not found.");
        }

        [Fact]
        public void DownloadResource_ShouldThrowException_WhenPaymentRequired()
        {
            // Arrange: Insert a paid resource without a corresponding purchase.
            var resource = new DigitalResource
            {
                Title = "Paid Resource",
                FileType = "pdf",
                Category = "Docs",
                IsFree = false,
                Price = 10.0m,
                FileName = "paid_resource.pdf",
                FilePath = "/digital-resources/paid_resource.pdf",
                UploadDate = DateTime.UtcNow,
                UploadedBy = "Uploader"
            };
            _context.DigitalResources.Add(resource);
            _context.SaveChanges();

            // Create the file.
            string folder = Path.Combine(_tempWebRoot, "digital-resources");
            Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, resource.FileName);
            File.WriteAllText(fullPath, "Paid resource content");

            // Act
            Action act = () => _service.DownloadResource(resource.Id, "userWithoutPurchase");

            // Assert
            act.Should().Throw<Exception>().WithMessage("Payment required to download this resource.");
        }

        [Fact]
        public async Task DeleteResourceAsync_ShouldDeleteRecordAndFile()
        {
            // Arrange: Insert a resource record and create its file.
            var resource = new DigitalResource
            {
                Title = "Resource to Delete",
                FileType = "pdf",
                Category = "Docs",
                IsFree = true,
                Price = null,
                FileName = "delete_resource.pdf",
                FilePath = "/digital-resources/delete_resource.pdf",
                UploadDate = DateTime.UtcNow,
                UploadedBy = "Uploader"
            };
            _context.DigitalResources.Add(resource);
            await _context.SaveChangesAsync();

            string folder = Path.Combine(_tempWebRoot, "digital-resources");
            Directory.CreateDirectory(folder);
            string fullPath = Path.Combine(folder, resource.FileName);
            File.WriteAllText(fullPath, "Delete me");

            // Act
            await _service.DeleteResourceAsync(resource.Id);

            // Assert: File should be deleted and record removed.
            File.Exists(fullPath).Should().BeFalse();
            var record = _context.DigitalResources.Find(resource.Id);
            record.Should().BeNull();
        }

        [Fact]
        public async Task EditResourceAsync_ShouldUpdateRecord()
        {
            // Arrange: Insert a resource record.
            var resource = new DigitalResource
            {
                Title = "Old Title",
                FileType = "pdf",
                Category = "OldCategory",
                IsFree = true,
                Price = null,
                FileName = "edit_resource.pdf",
                FilePath = "/digital-resources/edit_resource.pdf",
                UploadDate = DateTime.UtcNow,
                UploadedBy = "Uploader"
            };
            _context.DigitalResources.Add(resource);
            await _context.SaveChangesAsync();

            // Prepare an updated DTO.
            var updatedDto = new DigitalResourceDTO
            {
                Id = resource.Id,
                Title = "New Title",
                FileType = resource.FileType, // unchanged
                Category = "NewCategory",
                IsFree = false,
                Price = 15.99m,
                FilePath = resource.FilePath, // unchanged
                UploadDate = resource.UploadDate // unchanged
            };

            // Act
            await _service.EditResourceAsync(resource.Id, updatedDto);

            // Assert
            var updatedRecord = _context.DigitalResources.Find(resource.Id);
            updatedRecord.Should().NotBeNull();
            updatedRecord!.Title.Should().Be("New Title");
            updatedRecord.Category.Should().Be("NewCategory");
            updatedRecord.IsFree.Should().BeFalse();
            updatedRecord.Price.Should().Be(15.99m);
        }
    }
}

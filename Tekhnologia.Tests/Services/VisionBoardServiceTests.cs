using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Tests.Services
{
    public class VisionBoardServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IVisionBoardService _visionBoardService;

        public VisionBoardServiceTests()
        {
            // Create a new unique in‑memory database for isolation.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _visionBoardService = new VisionBoardService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateVisionBoardItemAsync_ShouldAddItem()
        {
            // Arrange
            string userId = "user1";
            var dto = new CreateVisionBoardItemDTO
            {
                ImageUrl = "http://example.com/image.jpg",
                Caption = "Test Caption"
            };

            // Act
            var item = await _visionBoardService.CreateVisionBoardItemAsync(userId, dto);

            // Assert
            item.Should().NotBeNull();
            item.ImageUrl.Should().Be(dto.ImageUrl);
            item.Caption.Should().Be(dto.Caption);
            item.UserId.Should().Be(userId);
            _context.VisionBoardItems.Any(i => i.VisionId == item.VisionId).Should().BeTrue();
        }

        [Fact]
        public async Task GetUserVisionBoardAsync_ShouldReturnItemsForUser()
        {
            // Arrange
            string userId = "user2";
            // Add two items for the target user and one for a different user.
            _context.VisionBoardItems.Add(new VisionBoardItem
            {
                UserId = userId,
                ImageUrl = "http://example.com/1.jpg",
                Caption = "Caption 1",
                CreatedAt = DateTime.UtcNow
            });
            _context.VisionBoardItems.Add(new VisionBoardItem
            {
                UserId = userId,
                ImageUrl = "http://example.com/2.jpg",
                Caption = "Caption 2",
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            });
            _context.VisionBoardItems.Add(new VisionBoardItem
            {
                UserId = "otherUser",
                ImageUrl = "http://example.com/3.jpg",
                Caption = "Other Caption",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act
            var items = await _visionBoardService.GetUserVisionBoardAsync(userId);

            // Assert
            items.Should().HaveCount(2);
            items.All(i => i.ImageUrl.StartsWith("http://example.com/")).Should().BeTrue();
        }

        [Fact]
        public async Task GetVisionBoardItemByIdAsync_ShouldReturnItem_WhenUserMatches()
        {
            // Arrange
            string userId = "user3";
            var item = new VisionBoardItem
            {
                UserId = userId,
                ImageUrl = "http://example.com/item.jpg",
                Caption = "Find me",
                CreatedAt = DateTime.UtcNow
            };
            _context.VisionBoardItems.Add(item);
            await _context.SaveChangesAsync();

            // Act
            var result = await _visionBoardService.GetVisionBoardItemByIdAsync(item.VisionId, userId);

            // Assert
            result.Should().NotBeNull();
            result!.VisionId.Should().Be(item.VisionId);
        }

        [Fact]
        public async Task GetVisionBoardItemByIdAsync_ShouldReturnNull_WhenUserMismatch()
        {
            // Arrange
            var item = new VisionBoardItem
            {
                UserId = "userX",
                ImageUrl = "http://example.com/item2.jpg",
                Caption = "Mismatch",
                CreatedAt = DateTime.UtcNow
            };
            _context.VisionBoardItems.Add(item);
            await _context.SaveChangesAsync();

            // Act
            var result = await _visionBoardService.GetVisionBoardItemByIdAsync(item.VisionId, "differentUser");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task UpdateVisionBoardItemAsync_ShouldUpdateItem_WhenUserMatches()
        {
            // Arrange
            string userId = "user4";
            var item = new VisionBoardItem
            {
                UserId = userId,
                ImageUrl = "http://example.com/old.jpg",
                Caption = "Old Caption",
                CreatedAt = DateTime.UtcNow
            };
            _context.VisionBoardItems.Add(item);
            await _context.SaveChangesAsync();

            var dto = new CreateVisionBoardItemDTO
            {
                ImageUrl = "http://example.com/new.jpg",
                Caption = "New Caption"
            };

            // Act
            var (success, error, updatedItem) = await _visionBoardService.UpdateVisionBoardItemAsync(item.VisionId, dto, userId);

            // Assert
            success.Should().BeTrue();
            error.Should().BeEmpty();
            updatedItem.Should().NotBeNull();
            updatedItem!.ImageUrl.Should().Be(dto.ImageUrl);
            updatedItem.Caption.Should().Be(dto.Caption);
        }

        [Fact]
        public async Task UpdateVisionBoardItemAsync_ShouldReturnFalse_WhenItemNotFoundOrUserMismatch()
        {
            // Arrange
            string userId = "user4";
            var dto = new CreateVisionBoardItemDTO
            {
                ImageUrl = "http://example.com/new.jpg",
                Caption = "New Caption"
            };

            // Act
            var (success, error, updatedItem) = await _visionBoardService.UpdateVisionBoardItemAsync(Guid.NewGuid(), dto, userId);

            // Assert
            success.Should().BeFalse();
            error.Should().NotBeEmpty();
            updatedItem.Should().BeNull();
        }

        [Fact]
        public async Task DeleteVisionBoardItemAsync_ShouldDeleteItem_WhenUserMatches()
        {
            // Arrange
            string userId = "user5";
            var item = new VisionBoardItem
            {
                UserId = userId,
                ImageUrl = "http://example.com/delete.jpg",
                Caption = "Delete me",
                CreatedAt = DateTime.UtcNow
            };
            _context.VisionBoardItems.Add(item);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _visionBoardService.DeleteVisionBoardItemAsync(item.VisionId, userId);

            // Assert
            success.Should().BeTrue();
            error.Should().BeEmpty();
            var itemInDb = await _context.VisionBoardItems.FindAsync(item.VisionId);
            itemInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeleteVisionBoardItemAsync_ShouldReturnFalse_WhenUserMismatch()
        {
            // Arrange
            var item = new VisionBoardItem
            {
                UserId = "user6",
                ImageUrl = "http://example.com/nodelete.jpg",
                Caption = "Cannot delete by different user",
                CreatedAt = DateTime.UtcNow
            };
            _context.VisionBoardItems.Add(item);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _visionBoardService.DeleteVisionBoardItemAsync(item.VisionId, "differentUser");

            // Assert
            success.Should().BeFalse();
            error.Should().NotBeEmpty();
            var itemInDb = await _context.VisionBoardItems.FindAsync(item.VisionId);
            itemInDb.Should().NotBeNull();
        }
    }
}

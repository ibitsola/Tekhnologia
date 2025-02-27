using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services;
using Tekhnologia.Services.Interfaces; 

namespace Tekhnologia.Tests.Services
{
    public class JournalServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IJournalService _journalService;

        public JournalServiceTests()
        {
            // Create a new in-memory database with a unique name for isolation
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _journalService = new JournalService(_context);
        }

        public void Dispose()
        {
            // Cleanup the in-memory database after each test
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateJournalEntryAsync_ShouldAddEntry()
        {
            // Arrange
            string userId = "user123";
            var dto = new CreateJournalEntryDTO 
            { 
                EntryText = "This is a test journal entry",
                SentimentScore = 5,
                Visibility = true
            };

            // Act
            var entry = await _journalService.CreateJournalEntryAsync(userId, dto);

            // Assert
            entry.Should().NotBeNull();
            entry.EntryText.Should().Be(dto.EntryText);
            entry.UserId.Should().Be(userId);
            _context.JournalEntries.Any(e => e.EntryId == entry.EntryId).Should().BeTrue();
        }

        [Fact]
        public async Task GetUserJournalEntriesAsync_ShouldReturnEntriesForUser()
        {
            // Arrange
            string userId = "user456";
            // Add two entries for the target user and one for a different user.
            _context.JournalEntries.Add(new JournalEntry
            {
                UserId = userId,
                EntryText = "User entry 1",
                SentimentScore = 3,
                Visibility = true,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            _context.JournalEntries.Add(new JournalEntry
            {
                UserId = userId,
                EntryText = "User entry 2",
                SentimentScore = 4,
                Visibility = false,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            _context.JournalEntries.Add(new JournalEntry
            {
                UserId = "otherUser",
                EntryText = "Other user entry",
                SentimentScore = 2,
                Visibility = true,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Act
            var entries = await _journalService.GetUserJournalEntriesAsync(userId);

            // Assert
            entries.Should().HaveCount(2);
            entries.All(e => e.EntryText.Contains("User entry")).Should().BeTrue();
        }

        [Fact]
        public async Task GetJournalEntryByIdAsync_ShouldReturnEntry_WhenUserMatches()
        {
            // Arrange
            string userId = "user789";
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                EntryText = "Find me",
                SentimentScore = 7,
                Visibility = true,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            // Act
            var entry = await _journalService.GetJournalEntryByIdAsync(journalEntry.EntryId, userId);

            // Assert
            entry.Should().NotBeNull();
            entry!.EntryId.Should().Be(journalEntry.EntryId);
        }

        [Fact]
        public async Task GetJournalEntryByIdAsync_ShouldReturnNull_WhenUserDoesNotMatch()
        {
            // Arrange
            var journalEntry = new JournalEntry
            {
                UserId = "differentUser",
                EntryText = "Should not be retrieved",
                SentimentScore = 5,
                Visibility = true,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            // Act
            var entry = await _journalService.GetJournalEntryByIdAsync(journalEntry.EntryId, "userABC");

            // Assert
            entry.Should().BeNull();
        }

        [Fact]
        public async Task UpdateJournalEntryAsync_ShouldUpdateEntry_WhenUserMatches()
        {
            // Arrange
            string userId = "userXYZ";
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                EntryText = "Original text",
                SentimentScore = 3,
                Visibility = true,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            var updateDto = new CreateJournalEntryDTO
            {
                EntryText = "Updated text",
                SentimentScore = 8,
                Visibility = false
            };

            // Act
            var (success, error, updatedEntry) = await _journalService.UpdateJournalEntryAsync(journalEntry.EntryId, updateDto, userId);

            // Assert
            success.Should().BeTrue();
            error.Should().BeEmpty();
            updatedEntry.Should().NotBeNull();
            updatedEntry!.EntryText.Should().Be("Updated text");
            updatedEntry.SentimentScore.Should().Be(8);
            updatedEntry.Visibility.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateJournalEntryAsync_ShouldReturnFalse_WhenEntryNotFoundOrUserMismatch()
        {
            // Arrange
            string userId = "userXYZ";
            var updateDto = new CreateJournalEntryDTO
            {
                EntryText = "Updated text",
                SentimentScore = 8,
                Visibility = false
            };

            // Act
            var (success, error, updatedEntry) = await _journalService.UpdateJournalEntryAsync(Guid.NewGuid(), updateDto, userId);

            // Assert
            success.Should().BeFalse();
            error.Should().NotBeEmpty();
            updatedEntry.Should().BeNull();
        }

        [Fact]
        public async Task DeleteJournalEntryAsync_ShouldDeleteEntry_WhenUserMatches()
        {
            // Arrange
            string userId = "userDelete";
            var journalEntry = new JournalEntry
            {
                UserId = userId,
                EntryText = "To be deleted",
                SentimentScore = 5,
                Visibility = true,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _journalService.DeleteJournalEntryAsync(journalEntry.EntryId, userId);

            // Assert
            success.Should().BeTrue();
            error.Should().BeEmpty();
            var entryInDb = await _context.JournalEntries.FindAsync(journalEntry.EntryId);
            entryInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeleteJournalEntryAsync_ShouldReturnFalse_WhenUserDoesNotMatch()
        {
            // Arrange
            string userId = "userDelete";
            var journalEntry = new JournalEntry
            {
                UserId = "anotherUser",
                EntryText = "Not deletable by userDelete",
                SentimentScore = 4,
                Visibility = true,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.JournalEntries.Add(journalEntry);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _journalService.DeleteJournalEntryAsync(journalEntry.EntryId, userId);

            // Assert
            success.Should().BeFalse();
            error.Should().NotBeEmpty();
            var entryInDb = await _context.JournalEntries.FindAsync(journalEntry.EntryId);
            entryInDb.Should().NotBeNull();
        }
    }
}

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Tekhnologia.Controllers;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces;
using Xunit;

namespace Tekhnologia.Tests.Controllers
{
    public class JournalControllerTests
    {
        private readonly Mock<IJournalService> _journalServiceMock;
        private readonly JournalController _controller;

        public JournalControllerTests()
        {
            _journalServiceMock = new Mock<IJournalService>();

            // Set up a fake user identity in the controller context.
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUser")
            }, "TestAuth"));

            _controller = new JournalController(_journalServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
        }

        [Fact]
        public async Task CreateJournalEntry_ReturnsOk_WhenEntryCreated()
        {
            // Arrange
            var dto = new CreateJournalEntryDTO
            {
                EntryText = "Test journal entry",
                SentimentScore = 5,
                Visibility = true
            };

            var createdEntry = new JournalEntry
            {
                EntryId = Guid.NewGuid(),
                UserId = "testUser",
                EntryText = dto.EntryText,
                SentimentScore = dto.SentimentScore,
                Visibility = dto.Visibility,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _journalServiceMock.Setup(s => s.CreateJournalEntryAsync("testUser", dto))
                .ReturnsAsync(createdEntry);

            // Act
            var result = await _controller.CreateJournalEntry(dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("CreateJournalEntry should return an OkObjectResult when successful");

            okResult.Value.Should().NotBeNull("the returned object should not be null");
            var resultType = okResult!.Value.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull("the returned object should contain a 'message' property");
            var message = messageProp!.GetValue(okResult.Value) as string;
            message.Should().Be("Journal entry created successfully");

            var entryProp = resultType.GetProperty("entry");
            entryProp.Should().NotBeNull("the returned object should contain an 'entry' property");
            var entry = entryProp!.GetValue(okResult.Value) as JournalEntry;
            entry.Should().BeEquivalentTo(createdEntry);
        }

        [Fact]
        public async Task GetUserJournalEntries_ReturnsOk_WithEntries()
        {
            // Arrange
            var entries = new List<JournalEntryDTO>
            {
                new JournalEntryDTO
                {
                    EntryId = Guid.NewGuid(),
                    EntryText = "Entry 1",
                    SentimentScore = 4,
                    Visibility = true,
                    Date = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new JournalEntryDTO
                {
                    EntryId = Guid.NewGuid(),
                    EntryText = "Entry 2",
                    SentimentScore = 3,
                    Visibility = false,
                    Date = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _journalServiceMock.Setup(s => s.GetUserJournalEntriesAsync("testUser"))
                .ReturnsAsync(entries);

            // Act
            var result = await _controller.GetUserJournalEntries();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(entries);
        }

        [Fact]
        public async Task GetJournalEntryById_ReturnsOk_WhenEntryExists()
        {
            // Arrange
            var entryId = Guid.NewGuid();
            var entry = new JournalEntry
            {
                EntryId = entryId,
                UserId = "testUser",
                EntryText = "Specific entry",
                SentimentScore = 6,
                Visibility = true,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _journalServiceMock.Setup(s => s.GetJournalEntryByIdAsync(entryId, "testUser"))
                .ReturnsAsync(entry);

            // Act
            var result = await _controller.GetJournalEntryById(entryId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.Value.Should().BeEquivalentTo(entry);
        }

        [Fact]
        public async Task GetJournalEntryById_ReturnsNotFound_WhenEntryDoesNotExist()
        {
            // Arrange
            var entryId = Guid.NewGuid();
            _journalServiceMock.Setup(s => s.GetJournalEntryByIdAsync(entryId, "testUser"))
                .ReturnsAsync((JournalEntry?)null);

            // Act
            var result = await _controller.GetJournalEntryById(entryId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.Value.Should().Be("Entry not found or access denied");
        }

        [Fact]
        public async Task UpdateJournalEntry_ReturnsOk_WhenUpdateSucceeds()
        {
            // Arrange
            var entryId = Guid.NewGuid();
            var dto = new CreateJournalEntryDTO
            {
                EntryText = "Updated entry",
                SentimentScore = 7,
                Visibility = false
            };

            var updatedEntry = new JournalEntry
            {
                EntryId = entryId,
                UserId = "testUser",
                EntryText = dto.EntryText,
                SentimentScore = dto.SentimentScore,
                Visibility = dto.Visibility,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _journalServiceMock.Setup(s => s.UpdateJournalEntryAsync(entryId, dto, "testUser"))
                .ReturnsAsync((true, string.Empty, updatedEntry));

            // Act
            var result = await _controller.UpdateJournalEntry(entryId, dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();

            var value = okResult!.Value;
            var resultType = value!.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull();
            var message = messageProp!.GetValue(value) as string;
            message.Should().Be("Journal entry updated successfully");

            var entryProp = resultType.GetProperty("entry");
            entryProp.Should().NotBeNull();
            var entry = entryProp!.GetValue(value) as JournalEntry;
            entry.Should().BeEquivalentTo(updatedEntry);
        }

        [Fact]
        public async Task DeleteJournalEntry_ReturnsOk_WhenDeletionSucceeds()
        {
            // Arrange
            var entryId = Guid.NewGuid();
            _journalServiceMock.Setup(s => s.DeleteJournalEntryAsync(entryId, "testUser"))
                .ReturnsAsync((true, string.Empty));

            // Act
            var result = await _controller.DeleteJournalEntry(entryId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            // Note: In the controller, the Delete action returns an anonymous object with a message.
            okResult.Value.Should().NotBeNull("the returned object should not be null");
            var resultType = okResult!.Value.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull();
            var message = messageProp!.GetValue(okResult.Value) as string;
            message.Should().Be("Journal entry deleted successfully");
        }

        [Fact]
        public async Task DeleteJournalEntry_ReturnsNotFound_WhenDeletionFails()
        {
            // Arrange
            var entryId = Guid.NewGuid();
            _journalServiceMock.Setup(s => s.DeleteJournalEntryAsync(entryId, "testUser"))
                .ReturnsAsync((false, "Entry not found or access denied"));

            // Act
            var result = await _controller.DeleteJournalEntry(entryId);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            notFoundResult.Should().NotBeNull();
            notFoundResult!.Value.Should().Be("Entry not found or access denied");
        }
    }
}

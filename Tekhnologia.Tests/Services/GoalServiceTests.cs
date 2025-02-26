using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Tests.Services
{
    public class GoalServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IGoalService _goalService;

        public GoalServiceTests()
        {
            // Use a unique in‑memory database for each test run to ensure isolation.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _goalService = new GoalService(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task CreateGoalAsync_ShouldAddGoal()
        {
            // Arrange
            string userId = "user1";
            var dto = new CreateGoalDTO
            {
                Title = "Test Goal",
                Description = "Goal Description",
                Deadline = DateTime.UtcNow.AddDays(7),
                Urgency = "Urgent",
                Importance = "Important"
            };

            // Act
            var goal = await _goalService.CreateGoalAsync(userId, dto);

            // Assert
            goal.Should().NotBeNull();
            goal.Title.Should().Be(dto.Title);
            goal.Description.Should().Be(dto.Description);
            goal.Deadline.Should().Be(dto.Deadline);
            goal.IsCompleted.Should().BeFalse();
            goal.UserId.Should().Be(userId);
            _context.Goals.Any(g => g.GoalId == goal.GoalId).Should().BeTrue();
        }

        [Fact]
        public async Task GetUserGoalsAsync_ShouldReturnGoalsForUser()
        {
            // Arrange
            string userId = "user2";
            // Add two goals for the target user and one for a different user.
            _context.Goals.Add(new Goal 
            { 
                UserId = userId, 
                Title = "Goal 1", 
                Description = "Desc 1", 
                Deadline = DateTime.UtcNow.AddDays(3), 
                Urgency = "Urgent", 
                Importance = "Important", 
                IsCompleted = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            });
            _context.Goals.Add(new Goal 
            { 
                UserId = userId, 
                Title = "Goal 2", 
                Description = "Desc 2", 
                Deadline = DateTime.UtcNow.AddDays(4), 
                Urgency = "Not Urgent", 
                Importance = "Not Important", 
                IsCompleted = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            });
            _context.Goals.Add(new Goal 
            { 
                UserId = "otherUser", 
                Title = "Other Goal", 
                Description = "Other", 
                Deadline = DateTime.UtcNow.AddDays(5), 
                Urgency = "Urgent", 
                Importance = "Important", 
                IsCompleted = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            });
            await _context.SaveChangesAsync();

            // Act
            var goals = await _goalService.GetUserGoalsAsync(userId);

            // Assert
            goals.Should().HaveCount(2);
            goals.All(g => g.Title.Contains("Goal")).Should().BeTrue();
        }

        [Fact]
        public async Task UpdateGoalAsync_ShouldUpdateGoal_WhenUserMatches()
        {
            // Arrange
            string userId = "user3";
            var goal = new Goal 
            { 
                UserId = userId, 
                Title = "Old Title", 
                Description = "Old Desc", 
                Deadline = DateTime.UtcNow.AddDays(2), 
                Urgency = "Urgent", 
                Importance = "Important", 
                IsCompleted = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            };
            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            var dto = new CreateGoalDTO 
            { 
                Title = "New Title", 
                Description = "New Desc", 
                Deadline = DateTime.UtcNow.AddDays(10), 
                Urgency = "Not Urgent", 
                Importance = "Not Important", 
                IsCompleted = false 
            };

            // Act
            var (success, error, updatedGoal) = await _goalService.UpdateGoalAsync(goal.GoalId, dto, userId);

            // Assert
            success.Should().BeTrue();
            error.Should().BeEmpty();
            updatedGoal.Should().NotBeNull();
            updatedGoal!.Title.Should().Be(dto.Title);
            updatedGoal.Description.Should().Be(dto.Description);
            updatedGoal.Deadline.Should().Be(dto.Deadline);
            // The completion status should remain unchanged (false)
            updatedGoal.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateGoalAsync_ShouldReturnFalse_WhenGoalNotFoundOrUserMismatch()
        {
            // Arrange
            string userId = "user3";
            var dto = new CreateGoalDTO 
            { 
                Title = "New Title", 
                Description = "New Desc", 
                Deadline = DateTime.UtcNow.AddDays(10), 
                Urgency = "Not Urgent", 
                Importance = "Not Important", 
                IsCompleted = false 
            };

            // Act
            var (success, error, updatedGoal) = await _goalService.UpdateGoalAsync(Guid.NewGuid(), dto, userId);

            // Assert
            success.Should().BeFalse();
            error.Should().NotBeEmpty();
            updatedGoal.Should().BeNull();
        }

        [Fact]
        public async Task MarkGoalAsCompletedAsync_ShouldMarkCompleted_WhenUserMatches()
        {
            // Arrange
            string userId = "user4";
            var goal = new Goal 
            { 
                UserId = userId, 
                Title = "Goal to complete", 
                Description = "Complete me", 
                Deadline = DateTime.UtcNow.AddDays(2), 
                Urgency = "Urgent", 
                Importance = "Important", 
                IsCompleted = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            };
            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            // Act
            var (success, error, updatedGoal) = await _goalService.MarkGoalAsCompletedAsync(goal.GoalId, userId);

            // Assert
            success.Should().BeTrue();
            error.Should().BeEmpty();
            updatedGoal.Should().NotBeNull();
            updatedGoal!.IsCompleted.Should().BeTrue();
        }

        [Fact]
        public async Task MarkGoalAsCompletedAsync_ShouldReturnFalse_WhenUserMismatch()
        {
            // Arrange
            var goal = new Goal 
            { 
                UserId = "userX", 
                Title = "Goal", 
                Description = "Mismatch", 
                Deadline = DateTime.UtcNow.AddDays(2), 
                Urgency = "Urgent", 
                Importance = "Important", 
                IsCompleted = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            };
            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            // Act
            var (success, error, updatedGoal) = await _goalService.MarkGoalAsCompletedAsync(goal.GoalId, "differentUser");

            // Assert
            success.Should().BeFalse();
            error.Should().NotBeEmpty();
            updatedGoal.Should().BeNull();
        }

        [Fact]
        public async Task DeleteGoalAsync_ShouldDeleteGoal_WhenUserMatches()
        {
            // Arrange
            string userId = "user5";
            var goal = new Goal 
            { 
                UserId = userId, 
                Title = "Goal to delete", 
                Description = "Delete me", 
                Deadline = DateTime.UtcNow.AddDays(2), 
                Urgency = "Urgent", 
                Importance = "Important", 
                IsCompleted = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            };
            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _goalService.DeleteGoalAsync(goal.GoalId, userId);

            // Assert
            success.Should().BeTrue();
            error.Should().BeEmpty();
            var goalInDb = await _context.Goals.FindAsync(goal.GoalId);
            goalInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeleteGoalAsync_ShouldReturnFalse_WhenUserMismatch()
        {
            // Arrange
            var goal = new Goal 
            { 
                UserId = "user6", 
                Title = "Goal not deletable", 
                Description = "Mismatch", 
                Deadline = DateTime.UtcNow.AddDays(2), 
                Urgency = "Urgent", 
                Importance = "Important", 
                IsCompleted = false, 
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow 
            };
            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _goalService.DeleteGoalAsync(goal.GoalId, "differentUser");

            // Assert
            success.Should().BeFalse();
            error.Should().NotBeEmpty();
            var goalInDb = await _context.Goals.FindAsync(goal.GoalId);
            goalInDb.Should().NotBeNull();
        }
    }
}

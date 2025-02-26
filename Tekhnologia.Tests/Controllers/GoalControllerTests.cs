using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Tekhnologia.Controllers;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Tests.Controllers
{
    public class GoalControllerTests
    {
        private readonly Mock<IGoalService> _goalServiceMock;
        private readonly GoalController _controller;

        public GoalControllerTests()
        {
            _goalServiceMock = new Mock<IGoalService>();

            // Set up a fake user identity in the controller context.
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "testUser")
            }, "TestAuth"));

            _controller = new GoalController(_goalServiceMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
        }

        [Fact]
        public async Task CreateGoal_ReturnsOk_WhenGoalCreated()
        {
            // Arrange
            var dto = new CreateGoalDTO
            {
                Title = "New Goal",
                Description = "Goal Description",
                Deadline = DateTime.UtcNow.AddDays(7),
                Urgency = "Urgent",
                Importance = "Important"
            };

            var createdGoal = new Goal
            {
                GoalId = Guid.NewGuid(),
                UserId = "testUser",
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                Urgency = dto.Urgency,
                Importance = dto.Importance,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _goalServiceMock.Setup(s => s.CreateGoalAsync("testUser", dto))
                .ReturnsAsync(createdGoal);

            // Act
            var result = await _controller.CreateGoal(dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("CreateGoal should return an OkObjectResult when successful");

            // Instead of using dynamic (which may cause runtime binder issues), use reflection to retrieve the properties.
            var value = okResult!.Value;
            value.Should().NotBeNull("The returned value should not be null");

            var resultType = value.GetType();

            // Check 'message' property
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull("the returned object should contain a 'message' property");
            var message = messageProp!.GetValue(value) as string;
            message.Should().Be("Goal created successfully");

            // Check 'goal' property
            var goalProp = resultType.GetProperty("goal");
            goalProp.Should().NotBeNull("the returned object should contain a 'goal' property");
            var goal = goalProp!.GetValue(value) as Goal;
            goal.Should().BeEquivalentTo(createdGoal);
        }

        [Fact]
        public async Task UpdateGoal_ReturnsOk_WhenUpdateSucceeds()
        {
            // Arrange
            Guid goalId = Guid.NewGuid();
            var dto = new CreateGoalDTO
            {
                Title = "Updated Goal",
                Description = "Updated Description",
                Deadline = DateTime.UtcNow.AddDays(15),
                Urgency = "Not Urgent",
                Importance = "Not Important"
            };

            var updatedGoal = new Goal
            {
                GoalId = goalId,
                UserId = "testUser",
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                Urgency = dto.Urgency,
                Importance = dto.Importance,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _goalServiceMock.Setup(s => s.UpdateGoalAsync(goalId, dto, "testUser"))
                .ReturnsAsync((true, string.Empty, updatedGoal));

            // Act
            var result = await _controller.UpdateGoal(goalId, dto);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("UpdateGoal should return an OkObjectResult when successful");

            var value = okResult!.Value;
            value.Should().NotBeNull("The returned value should not be null");

            var resultType = value.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull("the returned object should contain a 'message' property");
            var message = messageProp!.GetValue(value) as string;
            message.Should().Be("Goal updated successfully");

            var goalProp = resultType.GetProperty("goal");
            goalProp.Should().NotBeNull("the returned object should contain a 'goal' property");
            var goal = goalProp!.GetValue(value) as Goal;
            goal.Should().BeEquivalentTo(updatedGoal);
        }




        [Fact]
        public async Task DeleteGoal_ReturnsOk_WhenDeletionSucceeds()
        {
            // Arrange
            Guid goalId = Guid.NewGuid();
            _goalServiceMock.Setup(s => s.DeleteGoalAsync(goalId, "testUser"))
                .ReturnsAsync((true, string.Empty));

            // Act
            var result = await _controller.DeleteGoal(goalId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("DeleteGoal should return an OkObjectResult when deletion succeeds");

            // Instead of comparing to a plain string, compare to an anonymous object with a message property.
            okResult!.Value.Should().BeEquivalentTo(new { message = "Goal deleted successfully" });
        }


        [Fact]
        public async Task MarkGoalAsCompleted_ReturnsOk_WhenSuccessful()
        {
            // Arrange
            Guid goalId = Guid.NewGuid();
            var updatedGoal = new Goal
            {
                GoalId = goalId,
                UserId = "testUser",
                Title = "Some Goal",
                Description = "Some Description",
                Deadline = DateTime.UtcNow.AddDays(5),
                Urgency = "Urgent",
                Importance = "Important",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _goalServiceMock.Setup(s => s.MarkGoalAsCompletedAsync(goalId, "testUser"))
                .ReturnsAsync((true, string.Empty, updatedGoal));

            // Act
            var result = await _controller.MarkGoalAsCompleted(goalId);

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull("MarkGoalAsCompleted should return an OkObjectResult when successful");

            var value = okResult!.Value;
            value.Should().NotBeNull("The returned value should not be null");

            var resultType = value.GetType();
            var messageProp = resultType.GetProperty("message");
            messageProp.Should().NotBeNull("the returned object should contain a 'message' property");
            var message = messageProp!.GetValue(value) as string;
            message.Should().Be("Goal marked as completed");

            var goalProp = resultType.GetProperty("goal");
            goalProp.Should().NotBeNull("the returned object should contain a 'goal' property");
            var goal = goalProp!.GetValue(value) as Goal;
            goal.Should().NotBeNull();
            goal!.IsCompleted.Should().BeTrue();
        }
    }
}

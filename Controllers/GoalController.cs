using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tekhnologia.Services;
using System.Security.Claims;

namespace Tekhnologia.Controllers
{
    [ApiController]
    [Route("api/goals")]
    public class GoalController : ControllerBase
    {
        private readonly GoalService _goalService;

        public GoalController(GoalService goalService)
        {
            _goalService = goalService;
        }

        // Create a new goal
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var goal = await _goalService.CreateGoalAsync(userId, dto);
            return Ok(new { message = "Goal created successfully", goal });
        }

        // Get all goals for the logged-in user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserGoals()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var goals = await _goalService.GetUserGoalsAsync(userId);
            return Ok(goals);
        }

        // Update goal (only the owner)
        [HttpPut("{goalId}")]
        [Authorize]
        public async Task<IActionResult> UpdateGoal(Guid goalId, [FromBody] CreateGoalDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, error, updatedGoal) = await _goalService.UpdateGoalAsync(goalId, dto, userId!);
            if (!success)
                return NotFound(error);

            return Ok(new { message = "Goal updated successfully", goal = updatedGoal });
        }

        // Mark goal as completed
        [HttpPut("{goalId}/complete")]
        [Authorize]
        public async Task<IActionResult> MarkGoalAsCompleted(Guid goalId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, error, updatedGoal) = await _goalService.MarkGoalAsCompletedAsync(goalId, userId!);
            if (!success)
                return NotFound(error);

            return Ok(new { message = "Goal marked as completed", goal = updatedGoal });
        }

        // Delete a goal
        [HttpDelete("{goalId}")]
        [Authorize]
        public async Task<IActionResult> DeleteGoal(Guid goalId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, error) = await _goalService.DeleteGoalAsync(goalId, userId!);
            if (!success)
                return NotFound(error);

            return Ok(new { message = "Goal deleted successfully" });
        }
    }
}

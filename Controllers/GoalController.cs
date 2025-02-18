using Data;
using Models;
using Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Controllers
{
    [ApiController]
    [Route("api/goals")]
    public class GoalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public GoalController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Create a new goal
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateGoal([FromBody] CreateGoalDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            var goal = new Goal
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                 IsCompleted = false,
                Urgency = dto.Urgency,
                Importance = dto.Importance
            };

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Goal created successfully", goal });
        }

        // Get all goals for the logged-in user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserGoals()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            var goals = await _context.Goals
                .Where(g => g.UserId == userId)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new GoalResponseDTO
                {
                    GoalId = g.GoalId,
                    Title = g.Title,
                    Description = g.Description,
                    Deadline = g.Deadline,
                    IsCompleted = g.IsCompleted,
                    Urgency = g.Urgency,
                    Importance = g.Importance,
                    CreatedAt = g.CreatedAt,
                    UpdatedAt = g.UpdatedAt
                })
                .ToListAsync();

            return Ok(goals);
        }

        // Update goal (only the owner)
        [HttpPut("{goalId}")]
        [Authorize]
        public async Task<IActionResult> UpdateGoal(Guid goalId, [FromBody] CreateGoalDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var goal = await _context.Goals.FindAsync(goalId);

            if (goal == null || goal.UserId != userId) return NotFound("Goal not found or access denied");

            bool previousCompletionStatus = goal.IsCompleted;
            
            goal.Title = dto.Title;
            goal.Description = dto.Description;
            goal.Deadline = dto.Deadline;
            goal.Urgency = dto.Urgency;
            goal.Importance = dto.Importance;
            goal.IsCompleted = previousCompletionStatus;
            goal.UpdatedAt = DateTime.UtcNow;

            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Goal updated successfully", goal });
        }

        // Mark goal as completed
        [HttpPut("{goalId}/complete")]
        [Authorize]
        public async Task<IActionResult> MarkGoalAsCompleted(Guid goalId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var goal = await _context.Goals.FindAsync(goalId);

            if (goal == null || goal.UserId != userId) return NotFound("Goal not found or access denied");

            goal.IsCompleted = true;
            goal.UpdatedAt = DateTime.UtcNow;

            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Goal marked as completed", goal });
        }

        // Delete a goal
        [HttpDelete("{goalId}")]
        [Authorize]
        public async Task<IActionResult> DeleteGoal(Guid goalId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var goal = await _context.Goals.FindAsync(goalId);

            if (goal == null || goal.UserId != userId) return NotFound("Goal not found or access denied");

            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Goal deleted successfully" });
        }
    }
}
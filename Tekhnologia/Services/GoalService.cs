using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces; 
using Microsoft.EntityFrameworkCore;

namespace Tekhnologia.Services
{
    /// <summary>
    /// Provides business logic for creating, retrieving, updating, marking completed, 
    /// and deleting goals.
    /// </summary>
    public class GoalService : IGoalService
    {
        private readonly ApplicationDbContext _context;

        public GoalService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new goal for the specified user.
        /// </summary>
        public async Task<Goal> CreateGoalAsync(string userId, CreateGoalDTO dto)
        {
            var goal = new Goal
            {
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                Urgency = dto.Urgency,
                Importance = dto.Importance,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();
            return goal;
        }

        /// <summary>
        /// Retrieves all goals for the specified user, ordered by creation date descending.
        /// </summary>
        public async Task<List<GoalResponseDTO>> GetUserGoalsAsync(string userId)
        {
            return await _context.Goals
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
        }

        /// <summary>
        /// Updates an existing goal. Returns a tuple indicating success, an error message if any, and the updated goal.
        /// </summary>
        public async Task<(bool Success, string Error, Goal? UpdatedGoal)> UpdateGoalAsync(Guid goalId, CreateGoalDTO dto, string userId)
        {
            var goal = await _context.Goals.FindAsync(goalId);
            if (goal == null || goal.UserId != userId)
                return (false, "Goal not found or access denied", null);

            // Preserve the current completion status.
            bool currentStatus = goal.IsCompleted;

            goal.Title = dto.Title;
            goal.Description = dto.Description;
            goal.Deadline = dto.Deadline;
            goal.Urgency = dto.Urgency;
            goal.Importance = dto.Importance;
            goal.IsCompleted = currentStatus; // Do not update completion status here.
            goal.UpdatedAt = DateTime.UtcNow;

            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();
            return (true, string.Empty, goal);
        }

        /// <summary>
        /// Marks the goal as completed.
        /// </summary>
        public async Task<(bool Success, string Error, Goal? UpdatedGoal)> MarkGoalAsCompletedAsync(Guid goalId, string userId)
        {
            var goal = await _context.Goals.FindAsync(goalId);
            if (goal == null || goal.UserId != userId)
                return (false, "Goal not found or access denied", null);

            goal.IsCompleted = true;
            goal.UpdatedAt = DateTime.UtcNow;

            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();
            return (true, string.Empty, goal);
        }

        /// <summary>
        /// Deletes the goal for the specified user.
        /// </summary>
        public async Task<(bool Success, string Error)> DeleteGoalAsync(Guid goalId, string userId)
        {
            var goal = await _context.Goals.FindAsync(goalId);
            if (goal == null || goal.UserId != userId)
                return (false, "Goal not found or access denied");

            _context.Goals.Remove(goal);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }
    }
}

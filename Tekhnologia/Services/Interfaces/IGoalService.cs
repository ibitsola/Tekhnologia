using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;

namespace Tekhnologia.Services.Interfaces
{
    public interface IGoalService
    {
        Task<Goal> CreateGoalAsync(string userId, CreateGoalDTO dto);
        Task<List<GoalResponseDTO>> GetUserGoalsAsync(string userId);
        Task<(bool Success, string Error, Goal? UpdatedGoal)> UpdateGoalAsync(Guid goalId, CreateGoalDTO dto, string userId);
        Task<(bool Success, string Error, Goal? UpdatedGoal)> MarkGoalAsCompletedAsync(Guid goalId, string userId);
        Task<(bool Success, string Error, Goal? UpdatedGoal)> UnmarkGoalAsCompletedAsync(Guid goalId, string userId);
        Task<(bool Success, string Error)> DeleteGoalAsync(Guid goalId, string userId);
    }
}


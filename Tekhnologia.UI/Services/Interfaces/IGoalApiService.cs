using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tekhnologia.UI.Services.Interfaces
{
    public interface IGoalApiService
    {
        Task<IReadOnlyList<GoalResponseDTO>> GetUserGoalsAsync(string userId);
        Task<(bool Success, string? Error, GoalResponseDTO? Goal)> CreateGoalAsync(string userId, CreateGoalDTO dto);
        Task<(bool Success, string? Error)> UpdateGoalAsync(string goalId, string userId, UpdateGoalDTO dto);
        Task<(bool Success, string? Error)> MarkCompleteAsync(string goalId, string userId);
        Task<(bool Success, string? Error)> DeleteGoalAsync(string goalId, string userId);
    }

    public sealed class CreateGoalDTO
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string Urgency { get; set; } = "Not Urgent";
        public string Importance { get; set; } = "Not Important";
    }

    public sealed class UpdateGoalDTO
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string Urgency { get; set; } = "Not Urgent";
        public string Importance { get; set; } = "Not Important";
    }

    public sealed class GoalResponseDTO
    {
        public string GoalId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool IsCompleted { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public string Importance { get; set; } = string.Empty;
    }
}
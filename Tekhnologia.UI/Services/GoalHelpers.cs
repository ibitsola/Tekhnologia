using System;
using Tekhnologia.UI.Services.Interfaces;

namespace Tekhnologia.UI.Services
{
    internal static class GoalHelpers
    {
        public static int DaysIntoGoal(GoalResponseDTO goal)
        {
            var now = DateTime.UtcNow.Date;
            var createdDate = goal.CreatedAt.Date;
            return Math.Max(0, (now - createdDate).Days);
        }

        public static int? DaysRemaining(GoalResponseDTO goal)
        {
            if (goal.Deadline == null) return null;
            var now = DateTime.UtcNow.Date;
            var end = goal.Deadline.Value.Date;
            return (end - now).Days;
        }

        public static double ProgressPercentage(GoalResponseDTO goal)
        {
            if (goal.Deadline == null) return 0;
            var start = goal.CreatedAt;
            var end = goal.Deadline.Value;
            var now = DateTime.UtcNow;
            var total = (end - start).TotalDays;
            if (total <= 0) return 100;
            var passed = (now - start).TotalDays;
            var pct = (passed / total) * 100.0;
            return Math.Min(Math.Max(pct, 0), 100);
        }

        public static string PriorityLabel(GoalResponseDTO goal)
        {
            var urgency = goal.Urgency;
            var importance = goal.Importance;
            if (urgency == "Urgent" && importance == "Important") return "Critical";
            if (urgency == "Urgent") return "High";
            if (importance == "Important") return "Medium";
            return "Low";
        }
    }
}
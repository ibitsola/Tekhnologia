using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Tekhnologia.UI.Services.Interfaces;

namespace Tekhnologia.UI.Services
{
    internal sealed class GoalApiService : IGoalApiService
    {
        private readonly HttpClient _http;
        public GoalApiService(HttpClient http) => _http = http;

        public async Task<IReadOnlyList<GoalResponseDTO>> GetUserGoalsAsync(string userId)
        {
            var resp = await _http.GetAsync($"/api/goal/user/{userId}");
            if (!resp.IsSuccessStatusCode) return Array.Empty<GoalResponseDTO>();
            var json = await resp.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind != JsonValueKind.Array) return Array.Empty<GoalResponseDTO>();
                var list = new List<GoalResponseDTO>();
                foreach (var el in doc.RootElement.EnumerateArray())
                {
                    list.Add(ParseGoal(el));
                }
                return list;
            }
            catch { return Array.Empty<GoalResponseDTO>(); }
        }

        public async Task<(bool Success, string? Error, GoalResponseDTO? Goal)> CreateGoalAsync(string userId, CreateGoalDTO dto)
        {
            var payload = new
            {
                userId,
                title = dto.Title,
                description = dto.Description,
                deadline = dto.Deadline,
                urgency = dto.Urgency,
                importance = dto.Importance
            };
            var resp = await _http.PostAsJsonAsync("/api/goal", payload);
            if (!resp.IsSuccessStatusCode)
            {
                return (false, await resp.Content.ReadAsStringAsync(), null);
            }
            var json = await resp.Content.ReadAsStringAsync();
            try
            {
                using var doc = JsonDocument.Parse(json);
                return (true, null, ParseGoal(doc.RootElement));
            }
            catch { return (true, null, null); }
        }

        public async Task<(bool Success, string? Error)> UpdateGoalAsync(string goalId, string userId, UpdateGoalDTO dto)
        {
            var payload = new
            {
                userId,
                title = dto.Title,
                description = dto.Description,
                deadline = dto.Deadline,
                urgency = dto.Urgency,
                importance = dto.Importance
            };
            var resp = await _http.PutAsJsonAsync($"/api/goal/{goalId}", payload);
            if (!resp.IsSuccessStatusCode)
                return (false, await resp.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> MarkCompleteAsync(string goalId, string userId)
        {
            var payload = new { userId };
            var resp = await _http.PutAsJsonAsync($"/api/goal/{goalId}/complete", payload);
            if (!resp.IsSuccessStatusCode)
                return (false, await resp.Content.ReadAsStringAsync());
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> DeleteGoalAsync(string goalId, string userId)
        {
            var resp = await _http.DeleteAsync($"/api/goal/{goalId}?userId={userId}");
            if (!resp.IsSuccessStatusCode)
                return (false, await resp.Content.ReadAsStringAsync());
            return (true, null);
        }

        private static GoalResponseDTO ParseGoal(JsonElement el)
        {
            return new GoalResponseDTO
            {
                GoalId = el.TryGetProperty("goalId", out var idEl) ? idEl.GetString() ?? string.Empty : string.Empty,
                Title = el.TryGetProperty("title", out var tEl) ? tEl.GetString() ?? string.Empty : string.Empty,
                Description = el.TryGetProperty("description", out var dEl) ? dEl.GetString() : null,
                CreatedAt = el.TryGetProperty("createdAt", out var cEl) && cEl.TryGetDateTime(out var created) ? created : DateTime.UtcNow,
                Deadline = el.TryGetProperty("deadline", out var dlEl) && dlEl.ValueKind != JsonValueKind.Null && dlEl.TryGetDateTime(out var deadline) ? deadline : null,
                IsCompleted = el.TryGetProperty("isCompleted", out var compEl) && compEl.ValueKind == JsonValueKind.True,
                Urgency = el.TryGetProperty("urgency", out var uEl) ? uEl.GetString() ?? string.Empty : string.Empty,
                Importance = el.TryGetProperty("importance", out var iEl) ? iEl.GetString() ?? string.Empty : string.Empty
            };
        }
    }
}
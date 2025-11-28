using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tekhnologia.Models.DTOs;

namespace Tekhnologia.Services.ApiClients
{
    public class GoalApiService
    {
        private readonly HttpClient _httpClient;

        public GoalApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void SetAuthorizationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<List<GoalResponseDTO>> GetGoalsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<GoalResponseDTO>>("api/goals") ?? new List<GoalResponseDTO>();
        }

        public async Task<bool> CreateGoalAsync(CreateGoalDTO goalDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/goals", goalDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateGoalAsync(Guid goalId, CreateGoalDTO goalDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/goals/{goalId}", goalDto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MarkGoalAsCompletedAsync(Guid goalId)
        {
            var response = await _httpClient.PutAsync($"api/goals/{goalId}/complete", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UnmarkGoalAsCompletedAsync(Guid goalId)
        {
            var response = await _httpClient.PutAsync($"api/goals/{goalId}/incomplete", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteGoalAsync(Guid goalId)
        {
            var response = await _httpClient.DeleteAsync($"api/goals/{goalId}");
            return response.IsSuccessStatusCode;
        }
    }
}

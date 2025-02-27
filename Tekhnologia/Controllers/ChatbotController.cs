using Microsoft.AspNetCore.Mvc;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services; 
using Microsoft.AspNetCore.Authorization;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatbotController : ControllerBase
    {
        private readonly IAIService _aiService;

        public ChatbotController(IAIService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("business-coach")]
        public async Task<IActionResult> GetBusinessCoaching([FromBody] ChatRequestDTO chatRequest)
        {
            if (chatRequest == null || string.IsNullOrEmpty(chatRequest.Message))
            {
                return BadRequest("Invalid request data.");
            }

            var response = await _aiService.GetBusinessCoachingResponse(chatRequest.Message);
            return Ok(new ChatbotResponseDTO { Response = response });
        }

    }
}

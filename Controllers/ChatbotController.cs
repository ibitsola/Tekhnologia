using Microsoft.AspNetCore.Mvc;
using Models.DTOs;
using Services; 
using Microsoft.AspNetCore.Authorization;

namespace Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatbotController : ControllerBase
    {
        private readonly AIService _aiService;

        public ChatbotController(AIService aiService)
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

            // Use the correct variable name ("chatRequest") instead of "chatRequestDto"
            var response = await _aiService.GetBusinessCoachingResponse(chatRequest.Message);
            return Ok(new { response });
        }
    }
}

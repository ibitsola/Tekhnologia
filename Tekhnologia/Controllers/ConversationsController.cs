using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces;

namespace Tekhnologia.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConversationsController : ControllerBase
    {
        private readonly IConversationService _service;
        public ConversationsController(IConversationService service) { _service = service; }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var userId = GetUserId();
            var list = await _service.GetUserConversationsAsync(userId);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var userId = GetUserId();
            var conv = await _service.GetConversationAsync(id, userId);
            if (conv == null) return NotFound();
            return Ok(conv);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ConversationCreateDTO dto)
        {
            var userId = GetUserId();
            var conv = await _service.CreateConversationAsync(userId, dto);
            return CreatedAtAction(nameof(Get), new { id = conv.ConversationId }, conv);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ConversationCreateDTO dto)
        {
            var userId = GetUserId();
            var conv = await _service.UpdateConversationAsync(id, userId, dto);
            if (conv == null) return NotFound();
            return Ok(conv);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            var ok = await _service.DeleteConversationAsync(id, userId);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}

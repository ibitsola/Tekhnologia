using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tekhnologia.Services;
using System.Security.Claims;

namespace Tekhnologia.Controllers
{
    [ApiController]
    [Route("api/journal")]
    public class JournalController : ControllerBase
    {
        private readonly JournalService _journalService;

        public JournalController(JournalService journalService)
        {
            _journalService = journalService;
        }

        // Create a new journal entry
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var entry = await _journalService.CreateJournalEntryAsync(userId, dto);
            return Ok(new { message = "Journal entry created successfully", entry });
        }

        // Get all journal entries for the logged-in user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserJournalEntries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID not found in token");

            var entries = await _journalService.GetUserJournalEntriesAsync(userId);
            return Ok(entries);
        }

        // Get a single journal entry by ID
        [HttpGet("{entryId}")]
        [Authorize]
        public async Task<IActionResult> GetJournalEntryById(Guid entryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entry = await _journalService.GetJournalEntryByIdAsync(entryId, userId!);
            if (entry == null)
                return NotFound("Entry not found or access denied");

            return Ok(entry);
        }

        // Update an existing journal entry
        [HttpPut("{entryId}")]
        [Authorize]
        public async Task<IActionResult> UpdateJournalEntry(Guid entryId, [FromBody] CreateJournalEntryDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, error, updatedEntry) = await _journalService.UpdateJournalEntryAsync(entryId, dto, userId!);
            if (!success)
                return NotFound(error);

            return Ok(new { message = "Journal entry updated successfully", entry = updatedEntry });
        }

        // Delete a journal entry
        [HttpDelete("{entryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteJournalEntry(Guid entryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var (success, error) = await _journalService.DeleteJournalEntryAsync(entryId, userId!);
            if (!success)
                return NotFound(error);

            return Ok(new { message = "Journal entry deleted successfully" });
        }
    }
}

using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/journal")]
    public class JournalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public JournalController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Create a new journal entry
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateJournalEntry([FromBody] JournalEntry entry)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            entry.UserId = userId;
            entry.CreatedAt = DateTime.UtcNow;
            entry.UpdatedAt = DateTime.UtcNow;

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Journal entry created successfully", entry });
        }

        // Get all journal entries for the logged-in user
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserJournalEntries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            var entries = await _context.JournalEntries
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return Ok(entries);
        }

        // Get a single journal entry by ID (only if it belongs to the user)
        [HttpGet("{entryId}")]
        [Authorize]
        public async Task<IActionResult> GetJournalEntryById(Guid entryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entry = await _context.JournalEntries.FindAsync(entryId);

            if (entry == null || entry.UserId != userId) return NotFound("Entry not found or access denied");

            return Ok(entry);
        }

        // Update an existing journal entry
        [HttpPut("{entryId}")]
        [Authorize]
        public async Task<IActionResult> UpdateJournalEntry(Guid entryId, [FromBody] JournalEntry updatedEntry)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entry = await _context.JournalEntries.FindAsync(entryId);

            if (entry == null || entry.UserId != userId) return NotFound("Entry not found or access denied");

            entry.EntryText = updatedEntry.EntryText;
            entry.UpdatedAt = DateTime.UtcNow;

            _context.JournalEntries.Update(entry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Journal entry updated successfully", entry });
        }

        // Delete a journal entry
        [HttpDelete("{entryId}")]
        [Authorize]
        public async Task<IActionResult> DeleteJournalEntry(Guid entryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entry = await _context.JournalEntries.FindAsync(entryId);

            if (entry == null || entry.UserId != userId) return NotFound("Entry not found or access denied");

            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Journal entry deleted successfully" });
        }
    }
}

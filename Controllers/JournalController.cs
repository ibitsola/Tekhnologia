using Data;
using Models;
using Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Controllers
{
    [ApiController]
    [Route("api/journal")] // Base route for journal APIs
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
        [Authorize] // Ensures only logged-in users can create a journal entry
        public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // Ensures the request body meets model requirements

            // Extract UserId from token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            // Map DTO to Entity
            var entry = new JournalEntry
            {
                UserId = userId,
                EntryText = dto.EntryText,
                SentimentScore = dto.SentimentScore,
                Visibility = dto.Visibility,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save to database
            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Journal entry created successfully", entry });
        }

        // Get all journal entries for the logged-in user
        [HttpGet]
        [Authorize] // Ensures only logged-in users can access their journals
        public async Task<IActionResult> GetUserJournalEntries()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User ID not found in token");

            var entries = await _context.JournalEntries
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Select(e => new JournalEntryDTO
                {
                    EntryId = e.EntryId,
                    EntryText = e.EntryText,
                    SentimentScore = e.SentimentScore,
                    Visibility = e.Visibility,
                    Date = e.Date,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                })
                .ToListAsync();

            return Ok(entries);
        }

        // Get a single journal entry by ID
        [HttpGet("{entryId}")]
        [Authorize] // Ensures only the owner of the journal entry can view it
        public async Task<IActionResult> GetJournalEntryById(Guid entryId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entry = await _context.JournalEntries.FindAsync(entryId);

            if (entry == null || entry.UserId != userId) return NotFound("Entry not found or access denied");

            return Ok(entry);
        }

        // Update an existing journal entry
        [HttpPut("{entryId}")]
        [Authorize] // Ensures only the owner can update their entry
        public async Task<IActionResult> UpdateJournalEntry(Guid entryId, [FromBody] CreateJournalEntryDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var entry = await _context.JournalEntries.FindAsync(entryId);

            if (entry == null || entry.UserId != userId) return NotFound("Entry not found or access denied");

            // Update fields
            entry.EntryText = dto.EntryText;
            entry.SentimentScore = dto.SentimentScore;
            entry.Visibility = dto.Visibility;
            entry.UpdatedAt = DateTime.UtcNow;

            _context.JournalEntries.Update(entry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Journal entry updated successfully", entry });
        }

        // Delete a journal entry
        [HttpDelete("{entryId}")]
        [Authorize] // Ensures only the owner can delete their entry
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
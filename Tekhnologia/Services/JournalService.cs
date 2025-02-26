using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Tekhnologia.Services
{
    /// <summary>
    /// Provides business logic for creating, retrieving, updating, and deleting journal entries.
    /// </summary>
    public class JournalService
    {
        private readonly ApplicationDbContext _context;

        public JournalService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new journal entry for the specified user.
        /// </summary>
        public async Task<JournalEntry> CreateJournalEntryAsync(string userId, CreateJournalEntryDTO dto)
        {
            var entry = new JournalEntry
            {
                UserId = userId,
                EntryText = dto.EntryText,
                SentimentScore = dto.SentimentScore,
                Visibility = dto.Visibility,
                Date = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry;
        }

        /// <summary>
        /// Retrieves all journal entries for the given user.
        /// </summary>
        public async Task<List<JournalEntryDTO>> GetUserJournalEntriesAsync(string userId)
        {
            return await _context.JournalEntries
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
        }

        /// <summary>
        /// Retrieves a single journal entry by its ID, ensuring it belongs to the specified user.
        /// </summary>
        public async Task<JournalEntry?> GetJournalEntryByIdAsync(Guid entryId, string userId)
        {
            var entry = await _context.JournalEntries.FindAsync(entryId);
            if (entry == null || entry.UserId != userId)
                return null;
            return entry;
        }

        /// <summary>
        /// Updates an existing journal entry for the specified user.
        /// </summary>
        public async Task<(bool Success, string Error, JournalEntry? UpdatedEntry)> UpdateJournalEntryAsync(Guid entryId, CreateJournalEntryDTO dto, string userId)
        {
            var entry = await _context.JournalEntries.FindAsync(entryId);
            if (entry == null || entry.UserId != userId)
                return (false, "Entry not found or access denied", null);

            entry.EntryText = dto.EntryText;
            entry.SentimentScore = dto.SentimentScore;
            entry.Visibility = dto.Visibility;
            entry.UpdatedAt = DateTime.UtcNow;

            _context.JournalEntries.Update(entry);
            await _context.SaveChangesAsync();
            return (true, string.Empty, entry);
        }

        /// <summary>
        /// Deletes the journal entry for the specified user.
        /// </summary>
        public async Task<(bool Success, string Error)> DeleteJournalEntryAsync(Guid entryId, string userId)
        {
            var entry = await _context.JournalEntries.FindAsync(entryId);
            if (entry == null || entry.UserId != userId)
                return (false, "Entry not found or access denied");

            _context.JournalEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }
    }
}

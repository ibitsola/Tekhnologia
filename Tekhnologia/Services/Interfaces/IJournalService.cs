using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;

namespace Tekhnologia.Services.Interfaces
{
    public interface IJournalService
    {
        Task<JournalEntry> CreateJournalEntryAsync(string userId, CreateJournalEntryDTO dto);
        Task<List<JournalEntryDTO>> GetUserJournalEntriesAsync(string userId);
        Task<JournalEntry?> GetJournalEntryByIdAsync(Guid entryId, string userId);
        Task<(bool Success, string Error, JournalEntry? UpdatedEntry)> UpdateJournalEntryAsync(Guid entryId, CreateJournalEntryDTO dto, string userId);
        Task<(bool Success, string Error)> DeleteJournalEntryAsync(Guid entryId, string userId);
    }
}

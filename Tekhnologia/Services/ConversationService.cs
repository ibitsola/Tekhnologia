using Tekhnologia.Data;
using Tekhnologia.Models;
using Tekhnologia.Models.DTOs;
using Tekhnologia.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Tekhnologia.Services
{
    public class ConversationService : IConversationService
    {
        private readonly ApplicationDbContext _db;
        public ConversationService(ApplicationDbContext db) { _db = db; }

        public async Task<List<ConversationSummaryDTO>> GetUserConversationsAsync(string userId)
        {
            return await _db.Conversations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new ConversationSummaryDTO { ConversationId = c.ConversationId, Title = c.Title, UpdatedAt = c.UpdatedAt })
                .ToListAsync();
        }

        public async Task<ConversationDetailDTO?> GetConversationAsync(Guid conversationId, string userId)
        {
            var c = await _db.Conversations.FindAsync(conversationId);
            if (c == null || c.UserId != userId) return null;
            return new ConversationDetailDTO { ConversationId = c.ConversationId, Title = c.Title, MessagesJson = c.MessagesJson, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt };
        }

        public async Task<ConversationDetailDTO> CreateConversationAsync(string userId, ConversationCreateDTO dto)
        {
            var c = new Conversation { UserId = userId, Title = dto.Title, MessagesJson = dto.MessagesJson, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            _db.Conversations.Add(c);
            await _db.SaveChangesAsync();
            return new ConversationDetailDTO { ConversationId = c.ConversationId, Title = c.Title, MessagesJson = c.MessagesJson, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt };
        }

        public async Task<bool> DeleteConversationAsync(Guid conversationId, string userId)
        {
            var c = await _db.Conversations.FindAsync(conversationId);
            if (c == null || c.UserId != userId) return false;
            _db.Conversations.Remove(c);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<ConversationDetailDTO?> UpdateConversationAsync(Guid conversationId, string userId, ConversationCreateDTO dto)
        {
            var c = await _db.Conversations.FindAsync(conversationId);
            if (c == null || c.UserId != userId) return null;
            c.Title = dto.Title;
            c.MessagesJson = dto.MessagesJson;
            c.UpdatedAt = DateTime.UtcNow;
            _db.Conversations.Update(c);
            await _db.SaveChangesAsync();
            return new ConversationDetailDTO { ConversationId = c.ConversationId, Title = c.Title, MessagesJson = c.MessagesJson, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt };
        }
    }
}

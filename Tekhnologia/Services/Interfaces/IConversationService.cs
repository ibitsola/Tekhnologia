using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tekhnologia.Models.DTOs;

namespace Tekhnologia.Services.Interfaces
{
    public interface IConversationService
    {
        Task<List<ConversationSummaryDTO>> GetUserConversationsAsync(string userId);
        Task<ConversationDetailDTO?> GetConversationAsync(Guid conversationId, string userId);
        Task<ConversationDetailDTO> CreateConversationAsync(string userId, ConversationCreateDTO dto);
            Task<ConversationDetailDTO?> UpdateConversationAsync(Guid conversationId, string userId, ConversationCreateDTO dto);
        Task<bool> DeleteConversationAsync(Guid conversationId, string userId);
    }
}

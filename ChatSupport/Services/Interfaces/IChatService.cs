using ChatSupport.Models;
using MongoDB.Bson;

namespace ChatSupport.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatSessionDto> CreateChatSessionAsync();
        Task PollChatSessionAsync(ObjectId chatSessionId);
        Task SendChatMessageAsync(ChatMessage message);
        Task SendAgentChatMessageAsync(ChatAgentMessage message);
        Task CompleteChatSessionAsync(ObjectId chatSessionId);
    }

}
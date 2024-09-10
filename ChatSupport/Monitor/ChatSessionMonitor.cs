using System;
using ChatSupport.Data;
using ChatSupport.Models;

namespace ChatSupport.Monitor;

public class ChatSessionMonitor
{
    private readonly IMongoRepository<ChatSession> _chatSessionRepository;

    public ChatSessionMonitor(
        IMongoRepository<ChatSession> chatSessionRepository
    )
    {
        _chatSessionRepository = chatSessionRepository;
    }

    public async Task MonitorChatSessions()
    {
        var chatSessions = _chatSessionRepository.GetAll(x => x.Status == ChatSessionStatus.Pending);
        foreach (var session in chatSessions)
        {
            session.MissedPolls += 1;

            if (session.MissedPolls > 3)
            {
                session.Status = ChatSessionStatus.InActive;
            }

            await _chatSessionRepository.UpdateAsync(session.Id, session);
        }
    }
}

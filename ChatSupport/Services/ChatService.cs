using ChatSupport.Data;
using ChatSupport.Queue;
using ChatSupport.Models;
using MongoDB.Bson;
using ChatSupport.Services.Interfaces;
using ChatSupport.Queue.Interfaces;


namespace ChatSupport.Services
{


    public class ChatService : IChatService
    {
        private readonly ILogger<ChatService> _logger;
        private readonly IRabbitMqService _rabbitMqService;
        private readonly IMongoRepository<ChatSession> _chatSessionRepository;
        private readonly IMongoRepository<Agent> _agentRepository;

        public ChatService(
            ILogger<ChatService> logger,
            IRabbitMqService rabbitMqService,
            IMongoRepository<ChatSession> chatSessionRepository,
            IMongoRepository<Agent> agentRepository
        )
        {
            _logger = logger;
            _rabbitMqService = rabbitMqService;
            _agentRepository = agentRepository;
            _chatSessionRepository = chatSessionRepository;
        }

        public async Task<ChatSessionDto> CreateChatSessionAsync()
        {
            var chatSessionModel = new ChatSession
            {
                CreatedAt = DateTime.UtcNow
            };

            chatSessionModel = await _chatSessionRepository.AddAsync(chatSessionModel);
            _logger.LogInformation($"Chat session saved: {chatSessionModel.Id.ToString()}");

            _rabbitMqService.PublishMessage("session_queue", chatSessionModel.Id.ToString());
            _logger.LogInformation($"Chat session created: {chatSessionModel.Id.ToString()}");

            return new ChatSessionDto(chatSessionModel);
        }

        public async Task CompleteChatSessionAsync(ObjectId chatSessionId)
        {
            var chatSessionModel = _chatSessionRepository.GetById(chatSessionId);

            chatSessionModel.Status = ChatSessionStatus.Completed;
            chatSessionModel.CompletedAt = DateTime.UtcNow;

            await _chatSessionRepository.UpdateAsync(chatSessionId, chatSessionModel);
            _logger.LogInformation($"Chat session completed: {chatSessionId}");

            _rabbitMqService.RemoveQueue("chat_session_" + chatSessionId.ToString());
        }

        public async Task PollChatSessionAsync(ObjectId chatSessionId)
        {
            var chatSession = _chatSessionRepository.GetById(chatSessionId);
            if (chatSession == null || chatSession.Status != ChatSessionStatus.Pending)
            {
                _logger.LogWarning($"Pending chat session not found: {chatSessionId}");
                return;
            }
            chatSession.MissedPolls = 0;
            await _chatSessionRepository.UpdateAsync(chatSessionId, chatSession);
            _logger.LogInformation($"Chat session polled: {chatSessionId}");
        }

        public async Task SendChatMessageAsync(ChatMessage message)
        {
            var chatSession = _chatSessionRepository.GetById(new ObjectId(message.ChatSessionId));
            if (chatSession == null || chatSession.Status != ChatSessionStatus.InProgress)
            {
                _logger.LogWarning($"Active chat session not found: {message.ChatSessionId}");
                return;
            }

            _rabbitMqService.PublishMessage("chat_queue_" + message.ChatSessionId, message.Message);
            _logger.LogInformation("Chat message sent: " + message.ChatSessionId);
        }

        public async Task SendAgentChatMessageAsync(ChatAgentMessage message)
        {
            var agent = _agentRepository.GetById(new ObjectId(message.AgentId));
            if (agent == null || !agent.IsOnline || agent.ActiveSessionId == null)
            {
                _logger.LogWarning($"Agent not found or offline: {message.AgentId}");
                return;
            }
            var session = _chatSessionRepository.GetById(new ObjectId(agent.ActiveSessionId));
            if (session == null)
            {
                _logger.LogWarning("Agent session not found: " + agent.ActiveSessionId);
                return;
            }

            if (session.Status != ChatSessionStatus.InProgress)
            {
                _logger.LogWarning("Chat session not in progress: " + session.Id);
                return;
            }

            _rabbitMqService.PublishMessage("chat_queue_" + session.Id.ToString(), message.Message);
            _logger.LogInformation("Chat message sent: " + session.Id);
        }
    }
}
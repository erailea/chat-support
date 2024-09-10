using ChatSupport.Data;
using ChatSupport.Models;
using ChatSupport.Queue.Interfaces;
using ChatSupport.Services;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;

public class ChatServiceTests
{
    private readonly Mock<ILogger<ChatService>> _mockLogger;
    private readonly Mock<IRabbitMqService> _mockRabbitMqService;
    private readonly Mock<IMongoRepository<ChatSession>> _mockChatSessionRepository;
    private readonly Mock<IMongoRepository<Agent>> _mockAgentRepository;
    private readonly ChatService _chatService;

    public ChatServiceTests()
    {
        _mockLogger = new Mock<ILogger<ChatService>>();
        _mockRabbitMqService = new Mock<IRabbitMqService>();
        _mockChatSessionRepository = new Mock<IMongoRepository<ChatSession>>();
        _mockAgentRepository = new Mock<IMongoRepository<Agent>>();
        _chatService = new ChatService(
            _mockLogger.Object,
            _mockRabbitMqService.Object,
            _mockChatSessionRepository.Object,
            _mockAgentRepository.Object
        );
    }

    [Fact]
    public async Task CreateChatSessionAsync_ShouldCreateAndReturnChatSession()
    {
        // Arrange
        var chatSession = new ChatSession();
        _mockChatSessionRepository.Setup(repo => repo.AddAsync(It.IsAny<ChatSession>()))
            .Callback<ChatSession>(cs => chatSession.Id = ObjectId.GenerateNewId())
            .ReturnsAsync(chatSession);

        // Act
        var result = await _chatService.CreateChatSessionAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(chatSession.Id.ToString(), result.Id);

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Chat session saved: {chatSession.Id.ToString()}")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once
        );
        _mockRabbitMqService.Verify(r => r.PublishMessage("session_queue", chatSession.Id.ToString()), Times.Once);
    }

    [Fact]
    public async Task CompleteChatSessionAsync_ShouldCompleteChatSession()
    {
        // Arrange
        var chatSessionId = ObjectId.GenerateNewId();
        var chatSession = new ChatSession { Id = chatSessionId, Status = ChatSessionStatus.Pending };

        _mockChatSessionRepository.Setup(repo => repo.GetById(chatSessionId))
            .Returns(chatSession);

        // Act
        await _chatService.CompleteChatSessionAsync(chatSessionId);

        // Assert
        Assert.Equal(ChatSessionStatus.Completed, chatSession.Status);
        Assert.NotNull(chatSession.CompletedAt);
        _mockChatSessionRepository.Verify(repo => repo.UpdateAsync(chatSessionId, chatSession), Times.Once);
        _mockRabbitMqService.Verify(r => r.RemoveQueue($"chat_session_{chatSessionId}"), Times.Once);
    }

    [Fact]
    public async Task PollChatSessionAsync_ShouldResetMissedPolls_WhenSessionIsPending()
    {
        // Arrange
        var chatSessionId = ObjectId.GenerateNewId();
        var chatSession = new ChatSession { Id = chatSessionId, Status = ChatSessionStatus.Pending, MissedPolls = 5 };

        _mockChatSessionRepository.Setup(repo => repo.GetById(chatSessionId))
            .Returns(chatSession);

        // Act
        await _chatService.PollChatSessionAsync(chatSessionId);

        // Assert
        Assert.Equal(0, chatSession.MissedPolls);
        _mockChatSessionRepository.Verify(repo => repo.UpdateAsync(chatSessionId, chatSession), Times.Once);
    }

    [Fact]
    public async Task SendChatMessageAsync_ShouldSendMessage_WhenSessionIsInProgress()
    {
        // Arrange
        var chatSessionId = ObjectId.GenerateNewId();
        var message = new ChatMessage { ChatSessionId = chatSessionId.ToString(), Message = "Hello" };
        var chatSession = new ChatSession { Id = chatSessionId, Status = ChatSessionStatus.InProgress };

        _mockChatSessionRepository.Setup(repo => repo.GetById(chatSessionId))
            .Returns(chatSession);

        // Act
        await _chatService.SendChatMessageAsync(message);

        // Assert
        _mockRabbitMqService.Verify(r => r.PublishMessage($"chat_queue_{chatSessionId}", message.Message), Times.Once);
    }

    [Fact]
    public async Task SendAgentChatMessageAsync_ShouldSendMessage_WhenAgentIsOnline()
    {
        // Arrange
        var agentId = ObjectId.GenerateNewId();
        var agent = new Agent { Id = agentId, IsOnline = true, ActiveSessionId = ObjectId.GenerateNewId().ToString() };
        var message = new ChatAgentMessage { AgentId = agentId.ToString(), Message = "Hello from agent" };
        var sessionId = new ObjectId(agent.ActiveSessionId);
        var session = new ChatSession { Id = sessionId, Status = ChatSessionStatus.InProgress };

        _mockAgentRepository.Setup(repo => repo.GetById(agentId)).Returns(agent);
        _mockChatSessionRepository.Setup(repo => repo.GetById(sessionId)).Returns(session);

        // Act
        await _chatService.SendAgentChatMessageAsync(message);

        // Assert
        _mockRabbitMqService.Verify(r => r.PublishMessage($"chat_queue_{sessionId}", message.Message), Times.Once);
    }
}
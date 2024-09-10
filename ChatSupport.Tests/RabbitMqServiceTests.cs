using ChatSupport.Queue;
using ChatSupport.Services.Interfaces;
using ChatSupport.Data;
using ChatSupport.Models;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class RabbitMqServiceTests
{
    private readonly Mock<IConnectionFactory> _mockConnectionFactory;
    private readonly Mock<IAgentChatCoordinatorService> _mockAgentChatCoordinatorService;
    private readonly Mock<IMongoRepository<Agent>> _mockAgentRepository;
    private readonly Mock<IMongoRepository<ChatSession>> _mockChatSessionRepository;
    private readonly Mock<ILogger<RabbitMqService>> _mockLogger;
    private readonly RabbitMqService _service;
    private readonly Mock<IModel> _channelMock;

    public RabbitMqServiceTests()
    {
        _mockConnectionFactory = new Mock<IConnectionFactory>();
        _mockAgentChatCoordinatorService = new Mock<IAgentChatCoordinatorService>();
        _mockAgentRepository = new Mock<IMongoRepository<Agent>>();
        _mockChatSessionRepository = new Mock<IMongoRepository<ChatSession>>();
        _mockLogger = new Mock<ILogger<RabbitMqService>>();

        var mockConnection = new Mock<IConnection>();
        _channelMock = new Mock<IModel>();

        mockConnection.Setup(c => c.CreateModel()).Returns(_channelMock.Object);
        _mockConnectionFactory.Setup(f => f.CreateConnection()).Returns(mockConnection.Object);
        _service = new RabbitMqService(
            _mockConnectionFactory.Object,
            _mockAgentChatCoordinatorService.Object,
            _mockAgentRepository.Object,
            _mockChatSessionRepository.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public void CreateSessionQueue_ShouldDeclareQueueAndStartListening_OnConstruction()
    {
        // Assert
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Agent Chat Coordinator Service started. Listening for chat sessions...")),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleReceivedSessionQueueMessage_ShouldLogInactiveSession()
    {
        // Arrange
        var sessionId = MongoDB.Bson.ObjectId.GenerateNewId();
        var deliveryEventArgs = new BasicDeliverEventArgs { Body = Encoding.UTF8.GetBytes(sessionId.ToString()) };

        _mockChatSessionRepository.Setup(r => r.GetById(sessionId)).Returns(new ChatSession { Status = ChatSessionStatus.InActive });

        // Act
        await _service.HandleRecievedSessionQueueMessage(deliveryEventArgs);

        // Assert
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session: {sessionId} is inactive")),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleReceivedSessionQueueMessage_ShouldLogCompletedSession()
    {
        // Arrange
        var sessionId = MongoDB.Bson.ObjectId.GenerateNewId();
        var deliveryEventArgs = new BasicDeliverEventArgs { Body = Encoding.UTF8.GetBytes(sessionId.ToString()) };

        _mockChatSessionRepository.Setup(r => r.GetById(sessionId)).Returns(new ChatSession { Status = ChatSessionStatus.Completed });

        // Act
        await _service.HandleRecievedSessionQueueMessage(deliveryEventArgs);

        // Assert
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session: {sessionId} is already completed")),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleReceivedSessionQueueMessage_ShouldLogInProgressSession()
    {
        // Arrange
        var sessionId = MongoDB.Bson.ObjectId.GenerateNewId();
        var deliveryEventArgs = new BasicDeliverEventArgs { Body = Encoding.UTF8.GetBytes(sessionId.ToString()) };

        _mockChatSessionRepository.Setup(r => r.GetById(sessionId)).Returns(new ChatSession { Status = ChatSessionStatus.InProgress });

        // Act
        await _service.HandleRecievedSessionQueueMessage(deliveryEventArgs);

        // Assert
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Session: {sessionId} is already in progress")),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleReceivedSessionQueueMessage_ShouldAssignSessionToAvailableAgent()
    {
        // Arrange
        var sessionId = MongoDB.Bson.ObjectId.GenerateNewId();
        var deliveryEventArgs = new BasicDeliverEventArgs { Body = Encoding.UTF8.GetBytes(sessionId.ToString()) };
        var availableAgentId = MongoDB.Bson.ObjectId.GenerateNewId();

        _mockChatSessionRepository.Setup(r => r.GetById(sessionId)).Returns(new ChatSession { Status = ChatSessionStatus.Pending });
        _mockAgentChatCoordinatorService.Setup(s => s.GetAvailableAgent()).ReturnsAsync(availableAgentId);
        _mockAgentRepository.Setup(r => r.GetById(availableAgentId)).Returns(new Agent());

        // Act
        await _service.HandleRecievedSessionQueueMessage(deliveryEventArgs);

        // Assert
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Assigned session: {sessionId} to agent: {availableAgentId}")),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);

        _mockAgentRepository.Verify(r => r.UpdateAsync(availableAgentId, It.IsAny<Agent>()), Times.Once);
        _mockChatSessionRepository.Verify(r => r.UpdateAsync(sessionId, It.IsAny<ChatSession>()), Times.Once);
    }

    [Fact]
    public void DeclareQueue_ShouldDeclareRabbitMqQueue()
    {
        // Arrange
        var queueName = "test_queue";

        // Act
        _service.DeclareQueue(queueName);

        // Assert
        _mockConnectionFactory.Verify(m => m.CreateConnection(), Times.Once);
    }

    [Fact]
    public void PublishMessage_ShouldPublishToQueue()
    {
        // Arrange
        var queueName = "test_queue";
        var message = "Test message";

        // Act
        _service.PublishMessage(queueName, message);

        // Assert
        _channelMock.Verify(m => m.BasicPublish(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<IBasicProperties>(),
                    It.IsAny<ReadOnlyMemory<byte>>()
                ),
                Times.Once
            );
    }

    [Fact]
    public void RemoveQueue_ShouldCallQueueDelete()
    {
        // Act
        _service.RemoveQueue("test_queue");

        // Assert
        _channelMock.Verify(m => m.QueueDelete("test_queue", false, false), Times.Once);
    }
}
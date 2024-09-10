using ChatSupport.Data;
using ChatSupport.Exceptions;
using ChatSupport.Models;
using ChatSupport.Queue.Interfaces;
using ChatSupport.Services;
using ChatSupport.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

public class AgentServiceTests
{
    private readonly Mock<ILogger<AgentService>> _loggerMock;
    private readonly Mock<IMongoRepository<Agent>> _agentRepositoryMock;
    private readonly Mock<IMongoRepository<ChatSession>> _chatSessionRepositoryMock;
    private readonly Mock<IRabbitMqService> _rabbitMqServiceMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly AgentService _agentService;

    public AgentServiceTests()
    {
        _loggerMock = new Mock<ILogger<AgentService>>();
        _agentRepositoryMock = new Mock<IMongoRepository<Agent>>();
        _chatSessionRepositoryMock = new Mock<IMongoRepository<ChatSession>>();
        _rabbitMqServiceMock = new Mock<IRabbitMqService>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _agentService = new AgentService(
            _loggerMock.Object,
            _agentRepositoryMock.Object,
            _chatSessionRepositoryMock.Object,
            _rabbitMqServiceMock.Object,
            _dateTimeProviderMock.Object
        );
    }

    [Fact]
    public async Task ConnectAsync_WhenAgentDoesNotExist_ThrowsAgentNotFoundException()
    {
        // Arrange
        var agentId = ObjectId.GenerateNewId();
        _agentRepositoryMock.Setup(a => a.GetByIdAsync(agentId)).ReturnsAsync((Agent)null);

        // Act & Assert
        await Assert.ThrowsAsync<AgentNotFoundException>(() => _agentService.ConnectAsync(agentId));
    }

    [Fact]
    public async Task ConnectAsync_WhenAgentIsAlreadyOnline_ReturnsAgent()
    {
        // Arrange
        var agentId = ObjectId.GenerateNewId();
        var agent = new Agent { IsOnline = true };
        _agentRepositoryMock.Setup(a => a.GetByIdAsync(agentId)).ReturnsAsync(agent);

        // Act
        var result = await _agentService.ConnectAsync(agentId);

        // Assert
        Assert.Equal(agent, result);
        _rabbitMqServiceMock.Verify(x => x.DeclareQueue(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ConnectAsync_WhenAgentIsNotInShift_ThrowsAgentShiftConflictException()
    {
        // Arrange
        var agentId = ObjectId.GenerateNewId();
        var agent = new Agent { IsOnline = false, Shift = AgentShift.Afternoon };
        _agentRepositoryMock.Setup(a => a.GetByIdAsync(agentId)).ReturnsAsync(agent);
        _dateTimeProviderMock.Setup(dp => dp.Now).Returns(new DateTime(2021, 1, 1, 21, 0, 0));

        // Act & Assert
        await Assert.ThrowsAsync<AgentShiftConflictException>(() => _agentService.ConnectAsync(agentId));
    }

    [Fact]
    public async Task HandleAgentQueueMessageAsync_WhenAgentIsNotOnline_NacksMessage()
    {
        // Arrange
        var agentId = ObjectId.GenerateNewId();
        var sessionId = Guid.NewGuid().ToString();
        var agent = new Agent { IsOnline = false, ActiveSessionId = sessionId };
        _agentRepositoryMock.Setup(a => a.GetByIdAsync(agentId)).ReturnsAsync(agent);

        var modelMock = new Mock<IModel>();
        var deliveryArgs = new BasicDeliverEventArgs { DeliveryTag = 1, Body = Encoding.UTF8.GetBytes(sessionId) };

        // Act
        await _agentService.HandleAgentQueueMessageAsync(modelMock.Object, deliveryArgs, agentId);

        // Assert
        modelMock.Verify(m => m.BasicNack(deliveryArgs.DeliveryTag, false, true), Times.Once);
    }

    [Fact]
    public async Task HandleAgentQueueMessageAsync_WhenChatSessionStatusCompleted()
    {
        // Arrange
        var agentId = ObjectId.GenerateNewId();
        var sessionId = ObjectId.GenerateNewId();
        var agent = new Agent { IsOnline = true, ActiveSessionId = sessionId.ToString() };
        var chatSession = new ChatSession { Status = ChatSessionStatus.Completed };
        _agentRepositoryMock.Setup(a => a.GetByIdAsync(agentId)).ReturnsAsync(agent);
        _chatSessionRepositoryMock.Setup(c => c.GetByIdAsync(It.IsAny<ObjectId>())).ReturnsAsync(chatSession);

        var modelMock = new Mock<IModel>();
        var deliveryArgs = new BasicDeliverEventArgs { DeliveryTag = (ulong)1, Body = Encoding.UTF8.GetBytes(sessionId.ToString()) };

        // Act
        await _agentService.HandleAgentQueueMessageAsync(modelMock.Object, deliveryArgs, agentId);

        // Assert
        modelMock.Verify(m => m.BasicAck(deliveryArgs.DeliveryTag, false), Times.Once);
        _rabbitMqServiceMock.Verify(x => x.DeclareQueue(It.IsAny<string>()), Times.Once);
        _rabbitMqServiceMock.Verify(x => x.AddConsumer(It.IsAny<string>(), It.IsAny<EventHandler<BasicDeliverEventArgs>>()), Times.Once);
    }
    [Fact]
    public async Task HandleAgentQueueMessageAsync_WhenChatSessionStatusInProgress()
    {
        // Arrange
        var agentId = ObjectId.GenerateNewId();
        var sessionId = ObjectId.GenerateNewId();
        var agent = new Agent { IsOnline = true, ActiveSessionId = sessionId.ToString() };
        var chatSession = new ChatSession { Status = ChatSessionStatus.InProgress };
        _agentRepositoryMock.Setup(a => a.GetByIdAsync(agentId)).ReturnsAsync(agent);
        _chatSessionRepositoryMock.Setup(c => c.GetByIdAsync(It.IsAny<ObjectId>())).ReturnsAsync(chatSession);

        var modelMock = new Mock<IModel>();
        var deliveryArgs = new BasicDeliverEventArgs { DeliveryTag = (ulong)1, Body = Encoding.UTF8.GetBytes(sessionId.ToString()) };

        // Act
        await _agentService.HandleAgentQueueMessageAsync(modelMock.Object, deliveryArgs, agentId);

        // Assert
        modelMock.Verify(m => m.BasicNack(deliveryArgs.DeliveryTag, false, true), Times.Once);
        _rabbitMqServiceMock.Verify(x => x.DeclareQueue(It.IsAny<string>()), Times.Once);
        _rabbitMqServiceMock.Verify(x => x.AddConsumer(It.IsAny<string>(), It.IsAny<EventHandler<BasicDeliverEventArgs>>()), Times.Once);
    }

    [Fact]
    public async Task HandleChatQueueMessageAsync_BasicAckCalled_WhenProcessingMessage()
    {
        // Arrange
        var modelMock = new Mock<IModel>();
        var deliveryArgs = new BasicDeliverEventArgs { DeliveryTag = 1, Body = Encoding.UTF8.GetBytes("message content") };

        // Act
        await _agentService.HandleChatQueueMessageAsync(modelMock.Object, deliveryArgs);

        // Assert
        modelMock.Verify(m => m.BasicAck(deliveryArgs.DeliveryTag, false), Times.Once);
    }

    [Fact]
    public async Task HandleChatQueueMessageAsync_BasicNackCalled_OnException()
    {
        // Arrange
        var modelMock = new Mock<IModel>();
        var deliveryArgs = new BasicDeliverEventArgs { DeliveryTag = 1, Body = Encoding.UTF8.GetBytes("message content") };
        modelMock.Setup(m => m.BasicAck(It.IsAny<ulong>(), It.IsAny<bool>())).Throws(new Exception());

        // Act
        await _agentService.HandleChatQueueMessageAsync(modelMock.Object, deliveryArgs);

        // Assert
        modelMock.Verify(m => m.BasicNack(deliveryArgs.DeliveryTag, false, true), Times.Once);
    }

    [Fact]
    public async Task HandleChatQueueMessageAsync_WhenIModelIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var deliveryArgs = new BasicDeliverEventArgs { DeliveryTag = 1, Body = Encoding.UTF8.GetBytes("message content") };

        // Act
        async Task Act() => await _agentService.HandleChatQueueMessageAsync(null, deliveryArgs);

        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(Act);
    }
}
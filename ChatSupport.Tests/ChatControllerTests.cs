using ChatSupport.Controllers;
using ChatSupport.Models;
using ChatSupport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MongoDB.Bson;
using System;
using System.Threading.Tasks;
using Xunit;

public class ChatControllerTests
{
    private readonly Mock<IChatService> _mockChatService;
    private readonly Mock<ILogger<ChatController>> _mockLogger;
    private readonly ChatController _controller;

    public ChatControllerTests()
    {
        _mockChatService = new Mock<IChatService>();
        _mockLogger = new Mock<ILogger<ChatController>>();
        _controller = new ChatController(_mockLogger.Object, _mockChatService.Object);
    }

    [Fact]
    public async Task CreateChatSession_ReturnsOk_WhenSessionIsCreated()
    {
        // Arrange
        var agentId = ObjectId.GenerateNewId();
        var chatSessionDto = new ChatSessionDto() { Id = agentId.ToString() };
        _mockChatService.Setup(s => s.CreateChatSessionAsync())
            .ReturnsAsync(chatSessionDto);

        // Act
        var result = await _controller.CreateChatSession();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ChatSessionDto>(okResult.Value);
        Assert.Equal(chatSessionDto.Id, ((ChatSessionDto)okResult.Value).Id);
    }

    [Fact]
    public async Task CreateChatSession_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
    {
        // Arrange
        _mockChatService.Setup(s => s.CreateChatSessionAsync())
            .ThrowsAsync(new Exception("Unhandled exception"));

        // Act
        var result = await _controller.CreateChatSession();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("An error occurred", objectResult.Value);
    }

    [Fact]
    public async Task PollChatSession_ReturnsOk_WhenSessionIsPolled()
    {
        // Arrange
        var chatSessionId = ObjectId.GenerateNewId().ToString();

        // Act
        var result = await _controller.PollChatSession(chatSessionId);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockChatService.Verify(s => s.PollChatSessionAsync(new ObjectId(chatSessionId)), Times.Once);
    }

    [Fact]
    public async Task PollChatSession_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
    {
        // Arrange
        var chatSessionId = ObjectId.GenerateNewId().ToString();
        _mockChatService.Setup(s => s.PollChatSessionAsync(It.IsAny<ObjectId>()))
            .ThrowsAsync(new Exception("Unhandled exception"));

        // Act
        var result = await _controller.PollChatSession(chatSessionId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("An error occurred", objectResult.Value);
    }

    [Fact]
    public async Task SendChatMessage_ReturnsOk_WhenMessageIsSent()
    {
        // Arrange
        var chatMessage = new ChatMessage { ChatSessionId = ObjectId.GenerateNewId().ToString(), Message = "Hello" };

        // Act
        var result = await _controller.SendChatMessage(chatMessage);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockChatService.Verify(s => s.SendChatMessageAsync(chatMessage), Times.Once);
    }

    [Fact]
    public async Task SendChatMessage_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
    {
        // Arrange
        var chatMessage = new ChatMessage { ChatSessionId = ObjectId.GenerateNewId().ToString(), Message = "Hello" };
        _mockChatService.Setup(s => s.SendChatMessageAsync(It.IsAny<ChatMessage>()))
            .ThrowsAsync(new Exception("Unhandled exception"));

        // Act
        var result = await _controller.SendChatMessage(chatMessage);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("An error occurred", objectResult.Value);
    }

    [Fact]
    public async Task SendAgentChatMessage_ReturnsOk_WhenMessageIsSentByAgent()
    {
        // Arrange
        var chatAgentMessage = new ChatAgentMessage { AgentId = ObjectId.GenerateNewId().ToString(), Message = "Message from agent" };

        // Act
        var result = await _controller.SendAgentChatMessage(chatAgentMessage);

        // Assert
        Assert.IsType<OkResult>(result);
        _mockChatService.Verify(s => s.SendAgentChatMessageAsync(chatAgentMessage), Times.Once);
    }

    [Fact]
    public async Task SendAgentChatMessage_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
    {
        // Arrange
        var chatAgentMessage = new ChatAgentMessage { AgentId = ObjectId.GenerateNewId().ToString(), Message = "Message from agent" };
        _mockChatService.Setup(s => s.SendAgentChatMessageAsync(It.IsAny<ChatAgentMessage>()))
            .ThrowsAsync(new Exception("Unhandled exception"));

        // Act
        var result = await _controller.SendAgentChatMessage(chatAgentMessage);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("An error occurred", objectResult.Value);
    }
}
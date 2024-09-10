using ChatSupport.Controllers;
using ChatSupport.Exceptions;
using ChatSupport.Models;
using ChatSupport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Moq;

public class AgentControllerTests
{
    private readonly Mock<IAgentService> _mockAgentService;
    private readonly Mock<ILogger<AgentController>> _mockLogger;
    private readonly AgentController _controller;

    public AgentControllerTests()
    {
        _mockAgentService = new Mock<IAgentService>();
        _mockLogger = new Mock<ILogger<AgentController>>();
        _controller = new AgentController(_mockAgentService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Connect_ReturnsOk_WhenAgentIsConnected()
    {
        // Arrange
        var agentId = MongoDB.Bson.ObjectId.GenerateNewId();
        var agentRequest = new AgentConnectRequestDto { AgentId = agentId.ToString() };
        var agentDto = new AgentDto { Id = agentId.ToString() };
        var agent = new Agent { Id = agentId, IsOnline = true };
        _mockAgentService.Setup(s => s.ConnectAsync(agentId))
        .ReturnsAsync(agent);

        // Act
        var result = await _controller.Connect(agentRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var agentResult = (AgentDto)okResult.Value;
        Assert.IsType<AgentDto>(okResult.Value);
        Assert.Equal(true, agentResult.IsOnline);
        Assert.Equal(agentId.ToString(), agentDto.Id);
    }

    [Fact]
    public async Task Connect_ReturnsBadRequest_WhenAgentIsNull()
    {
        // Act
        var result = await _controller.Connect(null);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Connect_ReturnsNotFound_WhenAgentNotFound()
    {
        // Arrange
        var agentId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var agentRequest = new AgentConnectRequestDto { AgentId = agentId };

        _mockAgentService.Setup(s => s.ConnectAsync(It.IsAny<MongoDB.Bson.ObjectId>()))
            .ThrowsAsync(new AgentNotFoundException("Agent not found"));

        // Act
        var result = await _controller.Connect(agentRequest);

        // Assert
        var objectResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Agent with id Agent not found not found.", objectResult.Value);
    }

    [Fact]
    public async Task Connect_ReturnsForbidden_WhenShiftConflictOccurs()
    {
        // Arrange
        var agentId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var agentRequest = new AgentConnectRequestDto { AgentId = agentId };

        _mockAgentService.Setup(s => s.ConnectAsync(It.IsAny<MongoDB.Bson.ObjectId>()))
            .ThrowsAsync(new AgentShiftConflictException("agent id"));

        // Act
        var result = await _controller.Connect(agentRequest);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, objectResult.StatusCode);
        Assert.Equal("Agent with id agent id is not available for the requested shift.", objectResult.Value);
    }

    [Fact]
    public async Task Connect_ReturnsInternalServerError_WhenUnhandledExceptionOccurs()
    {
        // Arrange
        var agentId = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
        var agentRequest = new AgentConnectRequestDto { AgentId = agentId };

        _mockAgentService.Setup(s => s.ConnectAsync(It.IsAny<MongoDB.Bson.ObjectId>()))
            .ThrowsAsync(new System.Exception("Unhandled exception"));

        // Act
        var result = await _controller.Connect(agentRequest);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal("An error occurred", objectResult.Value);
    }
}
using ChatSupport.Models;
using ChatSupport.Services;
using ChatSupport.Data;
using MongoDB.Bson;
using Moq;

public class AgentChatCoordinatorServiceTests
{
    private readonly Mock<IMongoRepository<Agent>> _mockAgentRepository;
    private readonly AgentChatCoordinatorService _service;

    public AgentChatCoordinatorServiceTests()
    {
        _mockAgentRepository = new Mock<IMongoRepository<Agent>>();
        _service = new AgentChatCoordinatorService(_mockAgentRepository.Object);
    }

    [Fact]
    public async Task GetAvailableAgent_ReturnsAgentId_WhenAvailableAgentExists()
    {
        // Arrange
        var agent1 = new Agent
        {
            Id = ObjectId.GenerateNewId(),
            IsOnline = true,
            Seniority = AgentSeniority.MidLevel,
            AssignedSessions = new List<string>(),
        };

        var agent2 = new Agent
        {
            Id = ObjectId.GenerateNewId(),
            IsOnline = true,
            Seniority = AgentSeniority.MidLevel,
            AssignedSessions = new List<string> { "test1", "test2" },
        };

        _mockAgentRepository.Setup(repo => repo.GetAll())
            .Returns(new List<Agent> { agent1, agent2 });

        // Act
        var availableAgentId = await _service.GetAvailableAgent();

        // Assert
        Assert.NotNull(availableAgentId);
        Assert.Equal(agent1.Id, availableAgentId);
        _mockAgentRepository.Verify(repo => repo.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetAvailableAgent_ReturnsNull_WhenNoAvailableAgents()
    {
        // Arrange
        var agent1 = new Agent
        {
            Id = ObjectId.GenerateNewId(),
            IsOnline = true,
            Seniority = AgentSeniority.Junior,
            AssignedSessions = new List<string> { "test1", "test2", "test3", "test4", "test5", "test6" },
        };

        var agent2 = new Agent
        {
            Id = ObjectId.GenerateNewId(),
            IsOnline = false,
            Seniority = AgentSeniority.Senior,
            AssignedSessions = new List<string>(),
        };

        _mockAgentRepository.Setup(repo => repo.GetAll())
            .Returns(new List<Agent> { agent1, agent2 });

        // Act
        var availableAgentId = await _service.GetAvailableAgent();

        // Assert
        Assert.Null(availableAgentId);
        _mockAgentRepository.Verify(repo => repo.GetAll(), Times.Once);
    }

    [Fact]
    public async Task GetAvailableAgent_ReturnsNull_WhenAllAgentsAreOffline()
    {
        // Arrange
        var agent1 = new Agent
        {
            Id = ObjectId.GenerateNewId(),
            IsOnline = false,
            Seniority = AgentSeniority.MidLevel,
            AssignedSessions = new List<string>(),
        };

        var agent2 = new Agent
        {
            Id = ObjectId.GenerateNewId(),
            IsOnline = false,
            Seniority = AgentSeniority.MidLevel,
            AssignedSessions = new List<string>(),
        };

        _mockAgentRepository.Setup(repo => repo.GetAll())
            .Returns(new List<Agent> { agent1, agent2 });

        // Act
        var availableAgentId = await _service.GetAvailableAgent();

        // Assert
        Assert.Null(availableAgentId);
        _mockAgentRepository.Verify(repo => repo.GetAll(), Times.Once);
    }
}
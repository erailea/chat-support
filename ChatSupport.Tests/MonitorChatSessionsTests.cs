using ChatSupport.Data;
using ChatSupport.Models;
using ChatSupport.Monitor;
using MongoDB.Bson;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

public class ChatSessionMonitorTests
{
    private readonly Mock<IMongoRepository<ChatSession>> _chatSessionRepositoryMock;
    private readonly ChatSessionMonitor _chatSessionMonitor;

    public ChatSessionMonitorTests()
    {
        _chatSessionRepositoryMock = new Mock<IMongoRepository<ChatSession>>();
        _chatSessionMonitor = new ChatSessionMonitor(_chatSessionRepositoryMock.Object);
    }

    [Fact]
    public async Task MonitorChatSessions_WhenNoPendingSessions_DoesNotUpdateAnySession()
    {
        // Arrange
        _chatSessionRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<ChatSession, bool>>>()))
            .ReturnsAsync(new List<ChatSession>());

        // Act
        await _chatSessionMonitor.MonitorChatSessions();

        // Assert
        _chatSessionRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<ObjectId>(), It.IsAny<ChatSession>()), Times.Never);
    }

    [Fact]
    public async Task MonitorChatSessions_WhenSessionsHaveMissedPolls_LessThanOrEqualThree_UpdatesMissedPolls()
    {
        // Arrange
        var session1 = new ChatSession { Id = ObjectId.GenerateNewId(), Status = ChatSessionStatus.Pending, MissedPolls = 1 };
        var session2 = new ChatSession { Id = ObjectId.GenerateNewId(), Status = ChatSessionStatus.Pending, MissedPolls = 2 };
        var chatSessions = new List<ChatSession> { session1, session2 };

        _chatSessionRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<ChatSession, bool>>>()))
            .ReturnsAsync(chatSessions);

        // Act
        await _chatSessionMonitor.MonitorChatSessions();

        // Assert
        Assert.Equal(2, session1.MissedPolls);
        Assert.Equal(3, session2.MissedPolls);
        _chatSessionRepositoryMock.Verify(repo => repo.UpdateAsync(session1.Id, session1), Times.Once);
        _chatSessionRepositoryMock.Verify(repo => repo.UpdateAsync(session2.Id, session2), Times.Once);
    }

    [Fact]
    public async Task MonitorChatSessions_WhenSessionMissedPollsExceedsThree_UpdatesStatusToInactive()
    {
        // Arrange
        var session = new ChatSession { Id = ObjectId.GenerateNewId(), Status = ChatSessionStatus.Pending, MissedPolls = 3 };
        _chatSessionRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<ChatSession, bool>>>()))
            .ReturnsAsync(new List<ChatSession> { session });

        // Act
        await _chatSessionMonitor.MonitorChatSessions();

        // Assert
        Assert.Equal(ChatSessionStatus.InActive, session.Status);
        _chatSessionRepositoryMock.Verify(repo => repo.UpdateAsync(session.Id, session), Times.Once);
    }

    [Fact]
    public async Task MonitorChatSessions_WhenSessionMissedPollsDoesNotExceedThree_StatusRemainsPending()
    {
        // Arrange
        var session = new ChatSession { Id = ObjectId.GenerateNewId(), Status = ChatSessionStatus.Pending, MissedPolls = 2 };
        _chatSessionRepositoryMock.Setup(repo => repo.GetAllAsync(It.IsAny<Expression<Func<ChatSession, bool>>>()))
            .ReturnsAsync(new List<ChatSession> { session });

        // Act
        await _chatSessionMonitor.MonitorChatSessions();

        // Assert
        Assert.Equal(ChatSessionStatus.Pending, session.Status);
        _chatSessionRepositoryMock.Verify(repo => repo.UpdateAsync(session.Id, session), Times.Once);
    }
}
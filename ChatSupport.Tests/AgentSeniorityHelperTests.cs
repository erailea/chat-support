using ChatSupport.Helper;
using ChatSupport.Models;

public class AgentSeniorityHelperTests
{
    [Theory]
    [InlineData(AgentSeniority.Junior, false, 4)]
    [InlineData(AgentSeniority.MidLevel, false, 6)]
    [InlineData(AgentSeniority.Senior, false, 8)]
    [InlineData(AgentSeniority.TeamLead, false, 5)]
    [InlineData(AgentSeniority.Junior, true, 4)]
    [InlineData(AgentSeniority.MidLevel, true, 4)] //even if midlevel, overflow agents have the same capacity as junior agents
    public void GetCapacity_ShouldReturnExpectedCapacity(AgentSeniority seniority, bool isOverflowAgent, double expectedCapacity)
    {
        // Act
        var result = AgentSeniorityHelper.GetCapacity(seniority, isOverflowAgent);

        // Assert
        Assert.Equal(expectedCapacity, result);
    }
}

using ChatSupport.Models;
using Moq;

public class ShiftHelperTests
{
    [Theory]
    [InlineData(AgentShift.Morning, 9, true)]  // Before 12 PM
    [InlineData(AgentShift.Morning, 12, false)] // Exactly 12 PM
    [InlineData(AgentShift.Afternoon, 12, true)] // Exactly 12 PM
    [InlineData(AgentShift.Afternoon, 17, true)] // Before 6 PM
    [InlineData(AgentShift.Afternoon, 18, false)] // Exactly 6 PM
    [InlineData(AgentShift.Night, 18, true)] // Exactly 6 PM
    [InlineData(AgentShift.Night, 23, true)] // Before midnight
    public void IsInShift_ShouldReturnExpectedShiftStatus(AgentShift shift, int hour, bool expectedInShift)
    {
        // Arrange
        var dateTime = new DateTime(2023, 01, 01, hour, 0, 0);

        // Act
        var result = ShiftHelper.IsInShift(shift, dateTime);

        // Assert
        Assert.Equal(expectedInShift, result);
    }

    [Fact]
    public void IsInShift_Should_Throw_ArgumentOutOfRangeException_For_Invalid_AgentShift()
    {
        // Arrange
        var shift = (AgentShift)100;
        var dateTime = new DateTime(2024, 9, 10, 10, 0, 0);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            ShiftHelper.IsInShift(shift, dateTime));

        // Verify exception details
        Assert.Equal(nameof(shift), exception.ParamName);
        Assert.Equal(shift, exception.ActualValue);
    }
}
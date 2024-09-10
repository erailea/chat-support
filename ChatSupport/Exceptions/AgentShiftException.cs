namespace ChatSupport.Exceptions
{
    public class AgentShiftConflictException : Exception
    {
        public AgentShiftConflictException(string agentId) :
            base(
                $"Agent with id {agentId} is not available for the requested shift."
            )
        {
        }
    }
}

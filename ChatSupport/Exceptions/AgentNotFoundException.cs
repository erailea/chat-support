namespace ChatSupport.Exceptions
{
    public class AgentNotFoundException : Exception
    {
        public AgentNotFoundException(string agentId) :
            base($"Agent with id {agentId} not found.")
        {
        }
    }
}

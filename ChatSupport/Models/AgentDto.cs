namespace ChatSupport.Models
{
    public class AgentDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public string ActiveSessionId { get; set; }
        public AgentSeniority Seniority { get; set; }
        public AgentShift Shift { get; set; }
        public bool IsOnline { get; set; }

        public AgentDto() { }

        public AgentDto(Agent agent)
        {
            Id = agent.Id.ToString();
            Name = agent.Name;
            QueueName = agent.QueueName;
            ActiveSessionId = agent.ActiveSessionId;
            Seniority = agent.Seniority;
            Shift = agent.Shift;
            IsOnline = agent.IsOnline;
        }
    }
}

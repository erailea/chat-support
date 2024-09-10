using ChatSupport.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatSupport.Models
{
    [CollectionName("agents")]
    public class Agent
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string QueueName { get; set; } = string.Empty;
        public string ActiveSessionId { get; set; } = string.Empty;
        public string Team { get; set; } = string.Empty;
        public AgentSeniority Seniority { get; set; }
        public AgentShift Shift { get; set; }
        public bool IsOnline { get; set; }
        public bool IsOverFlowAgent { get; set; }
        public List<string> AssignedSessions { get; set; } = new List<string>();

        public double GetCapacity()
        {
            return AgentSeniorityHelper.GetCapacity(this.Seniority, IsOverFlowAgent);
        }
    }
}

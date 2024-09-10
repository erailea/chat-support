using ChatSupport.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatSupport.Models
{
    [CollectionName("sessions")]
    public class ChatSession
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ChatSessionStatus Status { get; set; } = ChatSessionStatus.Pending;
        public string AgentId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int MissedPolls { get; set; } = 0;
    }

    public class ChatSessionDto
    {
        public string Id { get; set; }
        public ChatSessionStatus Status { get; set; }
        public string AgentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int MissedPolls { get; set; }

        public ChatSessionDto() { }

        public ChatSessionDto(ChatSession chatSession)
        {
            Id = chatSession.Id.ToString();
            Status = chatSession.Status;
            AgentId = chatSession.AgentId;
            CreatedAt = chatSession.CreatedAt;
            CompletedAt = chatSession.CompletedAt;
            MissedPolls = chatSession.MissedPolls;
        }
    }
}

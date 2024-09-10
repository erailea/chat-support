using MongoDB.Bson;

namespace ChatSupport.Services.Interfaces
{
    public interface IAgentChatCoordinatorService
    {
        Task<ObjectId?> GetAvailableAgent();
    }
}
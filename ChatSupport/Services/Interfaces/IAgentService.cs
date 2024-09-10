using ChatSupport.Models;
using MongoDB.Bson;

namespace ChatSupport.Services.Interfaces
{
    public interface IAgentService
    {
        Task<Agent> ConnectAsync(ObjectId agentId);
    }
}
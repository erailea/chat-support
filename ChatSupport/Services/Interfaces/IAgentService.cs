using ChatSupport.Models;
using MongoDB.Bson;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatSupport.Services.Interfaces
{
    public interface IAgentService
    {
        Task<Agent> ConnectAsync(ObjectId agentId);
        Task HandleAgentQueueMessageAsync(IModel model, BasicDeliverEventArgs ea, ObjectId agentId);
        Task HandleChatQueueMessageAsync(IModel chatModel, BasicDeliverEventArgs chatEa);
    }
}
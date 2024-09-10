using ChatSupport.Data;
using ChatSupport.Models;
using ChatSupport.Queue;
using MongoDB.Bson;
using System.Text;
using RabbitMQ.Client;
using ChatSupport.Services.Interfaces;
using ChatSupport.Queue.Interfaces;
using ChatSupport.Exceptions;

namespace ChatSupport.Services
{
    
    public class AgentService : IAgentService
    {
        private readonly ILogger<AgentService> _logger;
        private readonly IMongoRepository<Agent> _agentRepository;
        private readonly IMongoRepository<ChatSession> _chatSessionRepository;
        private readonly IRabbitMqService _rabbitMqService;

        public AgentService(
            ILogger<AgentService> logger,
            IMongoRepository<Agent> agentRepository,
            IMongoRepository<ChatSession> chatSessionRepository,
            IRabbitMqService rabbitMqService
        )
        {
            _logger = logger;
            _agentRepository = agentRepository;
            _chatSessionRepository = chatSessionRepository;
            _rabbitMqService = rabbitMqService;
        }

        public async Task<Agent> ConnectAsync(ObjectId agentId)
        {
            var agent = await _agentRepository.GetByIdAsync(agentId);
            if (agent == null)
            {
                throw new AgentNotFoundException(agentId.ToString());
            }

            if (agent.IsOnline)
            {
                return agent;
            }

            if (ShiftHelper.IsInShift(agent.Shift))
            {
                throw new AgentShiftConflictException(agentId.ToString());
            }

            agent.IsOnline = true;
            agent.QueueName = "agent_queue_" + agent.Name.Replace(" ", "") + "_" + Guid.NewGuid().ToString().Replace("-", "_");

            await _agentRepository.UpdateAsync(agentId, agent);

            _rabbitMqService.DeclareQueue(agent.QueueName);

            _rabbitMqService.AddConsumer(agent.QueueName, async (model, ea) =>
            {
                var iModel = (IModel?) model;
                if(iModel == null)
                {
                    return;
                }
                var body = ea.Body.ToArray();
                var sessionId = Encoding.UTF8.GetString(body);

                try
                {
                    //check if agent is online and free
                    var consumerAgent = await _agentRepository.GetByIdAsync(agentId);

                    if (!consumerAgent.IsOnline || consumerAgent.ActiveSessionId != sessionId)
                    {
                        iModel.BasicNack(ea.DeliveryTag, false, true);
                        return;
                    }

                    _rabbitMqService.DeclareQueue("chat_queue_" + sessionId);

                    //add consumer to chat queue
                    _rabbitMqService.AddConsumer("chat_queue_" + sessionId, (chatModel, chatEa) =>
                    {
                        var chatIModel = (IModel?)chatModel;
                        if(chatIModel == null)
                        {
                            return;
                        }
                        var chatBody = chatEa.Body.ToArray();
                        var message = Encoding.UTF8.GetString(chatBody);

                        try
                        {
                            chatIModel.BasicAck(chatEa.DeliveryTag, false);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error processing chat message");
                            chatIModel.BasicNack(chatEa.DeliveryTag, false, true);
                        }
                    });

                    var session = await _chatSessionRepository.GetByIdAsync(new ObjectId(sessionId));

                    if (session.Status == ChatSessionStatus.Completed)
                    {
                        iModel.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        iModel.BasicNack(ea.DeliveryTag, false, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing chat message");
                    iModel.BasicNack(ea.DeliveryTag, false, true);
                }
            });

            return agent;
        }
    }
}

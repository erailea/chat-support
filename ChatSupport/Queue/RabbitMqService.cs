using System.Text;
using ChatSupport.Data;
using ChatSupport.Models;
using ChatSupport.Queue.Interfaces;
using ChatSupport.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatSupport.Queue
{
    public class RabbitMqService : IRabbitMqService
    {
        private readonly ILogger<RabbitMqService> _logger;
        private readonly IModel _channel;
        private readonly IAgentChatCoordinatorService _agentChatCoordinatorService;
        private readonly IMongoRepository<Agent> _agentRepository;
        private readonly IMongoRepository<ChatSession> _chatSessionRepository;

        public RabbitMqService(IConnectionFactory factory,
                                IAgentChatCoordinatorService agentChatCoordinatorService,
                                IMongoRepository<Agent> agentRepository,
                                IMongoRepository<ChatSession> chatSessionRepository,
                                ILogger<RabbitMqService> logger)
        {
            _logger = logger;
            _agentChatCoordinatorService = agentChatCoordinatorService;
            _agentRepository = agentRepository;
            _chatSessionRepository = chatSessionRepository;
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            CreateSessionQueue();
        }

        public void CreateSessionQueue()
        {
            DeclareQueue("session_queue");
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) => await HandleRecievedSessionQueueMessage(ea);
            AddConsumer("session_queue", async (_, ea) => await HandleRecievedSessionQueueMessage(ea));
            _logger.LogInformation("Agent Chat Coordinator Service started. Listening for chat sessions...");
        }

        public async Task HandleRecievedSessionQueueMessage(BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();

            var sessionId = new MongoDB.Bson.ObjectId(Encoding.UTF8.GetString(body));

            _logger.LogInformation("Received session: {0}", sessionId);

            var session = _chatSessionRepository.GetById(sessionId);

            if (session.Status == ChatSessionStatus.InActive)
            {
                _logger.LogInformation("Session: {0} is inactive", sessionId);
                return;
            }

            if (session.Status == ChatSessionStatus.Completed)
            {
                _logger.LogInformation("Session: {0} is already completed", sessionId);
                return;
            }

            if (session.Status == ChatSessionStatus.InProgress)
            {
                _logger.LogInformation("Session: {0} is already in progress", sessionId);
                return;
            }

            var agentId = await _agentChatCoordinatorService.GetAvailableAgent();

            if (agentId == null)
            {
                _logger.LogInformation("No available agent found for session: {0}", sessionId);
                return;
            }

            var agent = _agentRepository.GetById(agentId.Value);
            agent.ActiveSessionId = sessionId.ToString();
            await _agentRepository.UpdateAsync(agentId.Value, agent);

            session.AgentId = agentId.Value.ToString();
            session.Status = ChatSessionStatus.InProgress;
            await _chatSessionRepository.UpdateAsync(sessionId, session);

            //TODO: open a chat queue for communication between agent and client

            _logger.LogInformation("Assigned session: {0} to agent: {1}", sessionId, agentId);
        }

        public void DeclareQueue(string queueName)
        {
            _channel
                .QueueDeclare(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public void PublishMessage(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel
                .BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: null,
                    mandatory: true,
                    body: body
                );
        }

        public void AddConsumer(string queueName, EventHandler<BasicDeliverEventArgs> received)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += received;
            _channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        public void RemoveQueue(string queueName)
        {
            _channel.QueueDelete(queueName);
        }
    }
}

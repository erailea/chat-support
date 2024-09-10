using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using ChatSupport.Data;
using ChatSupport.Models;
using MongoDB.Bson;
using ChatSupport.Services.Interfaces;

namespace ChatSupport.Services
{
    public class AgentChatCoordinatorService : IAgentChatCoordinatorService
    {
        private readonly IMongoRepository<Agent> _agentRepository;

        public AgentChatCoordinatorService(
                IMongoRepository<Agent> agentRepository
        )
        {
            _agentRepository = agentRepository;
        }


        public async Task<ObjectId?> GetAvailableAgent()
        {
            var agents = await _agentRepository.GetAllAsync();
            var onlineAgents = agents.Where(x => x.IsOnline).ToList();

            var agentCapacities = onlineAgents.ToDictionary(agent => agent.Id, agent => agent.GetCapacity());
            var agentAssignments = onlineAgents.ToDictionary(agent => agent.Id, agent => agent.AssignedSessions.Count);

            var availableAgents = onlineAgents
                .Where(agent => agentAssignments[agent.Id] < agentCapacities[agent.Id])
                .OrderBy(agent => (int)agent.Seniority)
                .ToList();

            if (availableAgents.Any())
            {
                var selectedAgent = availableAgents.First();

                agentAssignments[selectedAgent.Id]++;

                return selectedAgent.Id;
            }

            return null;
        }
    }
}

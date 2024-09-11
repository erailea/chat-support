using ChatSupport.Data;
using ChatSupport.Models;
using ChatSupport.Services.Interfaces;

namespace ChatSupport.Services
{
    public class SeedService : ISeedService
    {
        private readonly IMongoRepository<Agent> _agentRepository;
        public SeedService(IMongoRepository<Agent> agentRepository)
        {
            _agentRepository = agentRepository;

        }
        public async Task SeedData()
        {
            if (await _agentRepository.CountAsync() > 0)
                return;

            var agents = new List<Agent>
            {
                // Team A
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f1a"),
                    Name = "Alice",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team A"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f2a"),
                    Name = "Bob",
                    Seniority = AgentSeniority.MidLevel,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team A"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f3a"),
                    Name = "Charlie",
                    Seniority = AgentSeniority.MidLevel,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team A"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f4a"),
                    Name = "David",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team A"
                },

                // Team B
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f5a"),
                    Name = "Eve",
                    Seniority = AgentSeniority.Senior,
                    Shift = AgentShift.Morning,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team B"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f6a"),
                    Name = "Frank",
                    Seniority = AgentSeniority.MidLevel,
                    Shift = AgentShift.Morning,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team B"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f7a"),
                    Name = "Grace",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Morning,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team B"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f8a"),
                    Name = "Hank",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Morning,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team B"
                },

                // Team C (Night shift team)
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f1b"),
                    Name = "Ivy",
                    Seniority = AgentSeniority.MidLevel,
                    Shift = AgentShift.Night,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team C"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f2b"),
                    Name = "Jack",
                    Seniority = AgentSeniority.MidLevel,
                    Shift = AgentShift.Night,
                    IsOnline = false,
                    IsOverFlowAgent = false,
                    Team = "Team C"
                },

                // Overflow team
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f3b"),
                    Name = "Karen",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = true,
                    Team = "Overflow"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f4b"),
                    Name = "Leo",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = true,
                    Team = "Overflow"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f5b"),
                    Name = "Mona",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = true,
                    Team = "Overflow"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f6b"),
                    Name = "Nina",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = true,
                    Team = "Overflow"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f7b"),
                    Name = "Oscar",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = true,
                    Team = "Overflow"
                },
                new Agent
                {
                    Id = new MongoDB.Bson.ObjectId("5f8f4b3b7b3f3b0b3c7b3f8b"),
                    Name = "Paul",
                    Seniority = AgentSeniority.Junior,
                    Shift = AgentShift.Afternoon,
                    IsOnline = false,
                    IsOverFlowAgent = true,
                    Team = "Overflow"
                }
            };


            foreach (var agent in agents)
            {
                await _agentRepository.AddAsync(agent);
            }
        }

    }
}

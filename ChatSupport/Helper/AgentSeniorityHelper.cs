using ChatSupport.Models;

namespace ChatSupport.Helper
{
    public static class AgentSeniorityHelper
    {
        private const int MaxConcurrency = 10;

        private static readonly Dictionary<AgentSeniority, double> SeniorityMultipliers = new()
        {
            { AgentSeniority.Junior, 0.4 },
            { AgentSeniority.MidLevel, 0.6 },
            { AgentSeniority.Senior, 0.8 },
            { AgentSeniority.TeamLead, 0.5 }
        };

        public static double GetCapacity(AgentSeniority seniority, bool isOverflowAgent)
        {
            double multiplier = isOverflowAgent ? SeniorityMultipliers[AgentSeniority.Junior] : SeniorityMultipliers[seniority];
            return MaxConcurrency * multiplier;
        }
    }
}
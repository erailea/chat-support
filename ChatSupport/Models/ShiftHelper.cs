namespace ChatSupport.Models
{
    public static class ShiftHelper
    {
        public static bool IsInShift(AgentShift shift)
        {
            var hour = DateTime.Now.Hour;
            switch (shift)
            {
                case AgentShift.Morning:
                    return hour < 12;
                case AgentShift.Afternoon:
                    return hour >= 12 && hour < 18;
                case AgentShift.Night:
                    return hour >= 18;
                default:
                    return false;
            }
        }
    }
}

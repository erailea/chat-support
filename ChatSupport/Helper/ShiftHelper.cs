namespace ChatSupport.Models
{
    public static class ShiftHelper
    {
        public static bool IsInShift(AgentShift shift, DateTime dateTime)
        {
            var hour = dateTime.Hour;
            switch (shift)
            {
                case AgentShift.Morning:
                    return hour < 12;
                case AgentShift.Afternoon:
                    return hour >= 12 && hour < 18;
                case AgentShift.Night:
                    return hour >= 18;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shift), shift, null);
            }
        }
    }
}
